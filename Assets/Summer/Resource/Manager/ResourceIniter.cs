using System;
using System.Collections.Generic;
using System.IO;
using Summer.Base.Model;
using Summer.Resource.Model.Callback;
using Summer.Resource.Model.Eve;
using Summer.Resource.Model.Group;
using Summer.Resource.Model.PackageVersion;
using Summer.Resource.Model.Vo;
using Spring.Core;
using Spring.Event;
using Spring.Logger;
using Spring.Util;
using Summer.Resource.Model.Constant;

namespace Summer.Resource.Manager
{
    /// <summary>
    /// 资源初始化器。
    /// </summary>
    public sealed class ResourceIniter
    {
        [Autowired]
        private ResourceManager resourceManager;

        [Autowired]
        private PackageVersionListSerializer packageVersionListSerializer;

        private readonly Dictionary<ResourceName, string> cachedFileSystemNames = new Dictionary<ResourceName, string>();


        /// <summary>
        /// 初始化资源。
        /// </summary>
        public void InitResources()
        {
            if (string.IsNullOrEmpty(resourceManager.readOnlyPath))
            {
                throw new GameFrameworkException("Readonly path is invalid.");
            }

            var fileUri = PathUtils.GetRemotePath(Path.Combine(resourceManager.readOnlyPath, ResourceManager.RemoteVersionListFileName));

            Log.Info("ResourceIniter资源路径[{}]", fileUri);

            resourceManager.simpleLoadResource.LoadBytes(fileUri, new LoadBytesCallbacks(OnLoadPackageVersionListSuccess, OnLoadPackageVersionListFailure), null);
        }

        private void OnLoadPackageVersionListSuccess(string fileUri, byte[] bytes, float duration, object userData)
        {
            MemoryStream memoryStream = null;
            try
            {
                memoryStream = new MemoryStream(bytes, false);
                PackageVersionList versionList = packageVersionListSerializer.Deserialize(memoryStream);
                if (!versionList.IsValid)
                {
                    throw new GameFrameworkException("Deserialize package version list failure.");
                }

                PackageVersionAsset[] assets = versionList.GetAssets();
                PackageVersionResource[] resources = versionList.GetResources();
                PackageVersionFileSystem[] fileSystems = versionList.GetFileSystems();
                PackageVersionResourceGroup[] resourceGroups = versionList.GetResourceGroups();
                resourceManager.applicableGameVersion = versionList.ApplicableGameVersion;
                resourceManager.internalResourceVersion = versionList.InternalResourceVersion;
                resourceManager.assetInfos = new Dictionary<string, AssetInfo>(assets.Length, StringComparer.Ordinal);
                resourceManager.resourceInfos = new Dictionary<ResourceName, ResourceInfo>(resources.Length, ResourceNameComparer.COMPARER);
                ResourceGroup defaultResourceGroup = resourceManager.GetOrAddResourceGroup(string.Empty);

                foreach (PackageVersionFileSystem fileSystem in fileSystems)
                {
                    int[] resourceIndexes = fileSystem.GetResourceIndexes();
                    foreach (int resourceIndex in resourceIndexes)
                    {
                        PackageVersionResource packageVersionResource = resources[resourceIndex];

                        cachedFileSystemNames.Add(new ResourceName(packageVersionResource.Name, packageVersionResource.Variant, packageVersionResource.Extension), fileSystem.Name);
                    }
                }

                foreach (PackageVersionResource resource in resources)
                {
                    ResourceName resourceName = new ResourceName(resource.Name, resource.Variant, resource.Extension);
                    int[] assetIndexes = resource.GetAssetIndexes();
                    foreach (int assetIndex in assetIndexes)
                    {
                        PackageVersionAsset packageVersionAsset = assets[assetIndex];
                        int[] dependencyAssetIndexes = packageVersionAsset.GetDependencyAssetIndexes();
                        int index = 0;
                        string[] dependencyAssetNames = new string[dependencyAssetIndexes.Length];
                        foreach (int dependencyAssetIndex in dependencyAssetIndexes)
                        {
                            dependencyAssetNames[index++] = assets[dependencyAssetIndex].Name;
                        }

                        resourceManager.assetInfos.Add(packageVersionAsset.Name,
                            new AssetInfo(packageVersionAsset.Name, resourceName, dependencyAssetNames));
                    }

                    string fileSystemName;
                    cachedFileSystemNames.TryGetValue(resourceName, out fileSystemName);

                    resourceManager.resourceInfos.Add(resourceName, new ResourceInfo(resourceName, fileSystemName, (LoadType) resource.LoadType, resource.Length,
                        resource.HashCode, true, true));
                    defaultResourceGroup.AddResource(resourceName, resource.Length, resource.Length);
                }

                foreach (PackageVersionResourceGroup resourceGroup in resourceGroups)
                {
                    ResourceGroup group = resourceManager.GetOrAddResourceGroup(resourceGroup.Name);
                    int[] resourceIndexes = resourceGroup.GetResourceIndexes();
                    foreach (int resourceIndex in resourceIndexes)
                    {
                        PackageVersionResource packageVersionResource = resources[resourceIndex];
                        group.AddResource(new ResourceName(packageVersionResource.Name, packageVersionResource.Variant, packageVersionResource.Extension),
                            packageVersionResource.Length, packageVersionResource.Length);
                    }
                }

                EventBus.SyncSubmit(ResourceInitCompleteEvent.ValueOf());
            }
            catch (Exception exception)
            {
                if (exception is GameFrameworkException)
                {
                    throw;
                }

                throw new GameFrameworkException(StringUtils.Format("Parse package version list exception '{}'.", exception.ToString()), exception);
            }
            finally
            {
                cachedFileSystemNames.Clear();
                if (memoryStream != null)
                {
                    memoryStream.Dispose();
                }
            }
        }

        private void OnLoadPackageVersionListFailure(string fileUri, string errorMessage, object userData)
        {
            throw new GameFrameworkException(StringUtils.Format(
                "Package version list '{}' is invalid, error message is '{}'.", fileUri,
                string.IsNullOrEmpty(errorMessage) ? "<Empty>" : errorMessage));
        }
    }
}