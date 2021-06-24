using System;
using System.Collections.Generic;
using System.IO;
using Summer.Base.Model;
using Summer.FileSystem.Model;
using Summer.Resource.Model.Callback;
using Summer.Resource.Model.Constant;
using Summer.Resource.Model.Group;
using Summer.Resource.Model.LocalVersion;
using Summer.Resource.Model.UpdatableVersion;
using Summer.Resource.Model.Vo;
using Spring.Core;
using Spring.Util;

namespace Summer.Resource.Manager
{
    /// <summary>
    /// 资源检查器。
    /// </summary>
    public sealed class ResourceChecker
    {
        [Autowired]
        private readonly ResourceManager resourceManager;

        [Autowired]
        public UpdatableVersionListSerializer updatableVersionListSerializer;

        [Autowired]
        public LocalVersionListSerializer localVersionListSerializer;

        private readonly Dictionary<ResourceName, CheckInfo> checkInfos = new Dictionary<ResourceName, CheckInfo>();
        private bool updatableVersionListReady;
        private bool readOnlyVersionListReady;
        private bool readWriteVersionListReady;

        public GameFrameworkAction<ResourceName, string, LoadType, int, int, int, int> resourceNeedUpdate;
        public GameFrameworkAction<int, int, int, long, long> resourceCheckComplete;


        /// <summary>
        /// 关闭并清理资源检查器。
        /// </summary>
        public void Shutdown()
        {
            checkInfos.Clear();
        }

        public void CheckResources()
        {
            TryRecoverReadWriteVersionList();

            if (resourceManager.simpleLoadResource == null)
            {
                throw new GameFrameworkException("Resource helper is invalid.");
            }

            if (string.IsNullOrEmpty(resourceManager.readOnlyPath))
            {
                throw new GameFrameworkException("Readonly path is invalid.");
            }

            if (string.IsNullOrEmpty(resourceManager.readWritePath))
            {
                throw new GameFrameworkException("Read-write path is invalid.");
            }

            resourceManager.simpleLoadResource.LoadBytes(PathUtils.GetRemotePath(Path.Combine(resourceManager.readWritePath, ResourceManager.RemoteVersionListFileName)),
                new LoadBytesCallbacks(OnLoadUpdatableVersionListSuccess, OnLoadUpdatableVersionListFailure), null);
            resourceManager.simpleLoadResource.LoadBytes(PathUtils.GetRemotePath(Path.Combine(resourceManager.readOnlyPath, ResourceManager.LocalVersionListFileName)),
                new LoadBytesCallbacks(OnLoadReadOnlyVersionListSuccess, OnLoadReadOnlyVersionListFailure), null);
            resourceManager.simpleLoadResource.LoadBytes(PathUtils.GetRemotePath(Path.Combine(resourceManager.readWritePath, ResourceManager.LocalVersionListFileName)),
                new LoadBytesCallbacks(OnLoadReadWriteVersionListSuccess, OnLoadReadWriteVersionListFailure), null);
        }

        private void SetCachedFileSystemName(ResourceName resourceName, string fileSystemName)
        {
            GetOrAddCheckInfo(resourceName).SetCachedFileSystemName(fileSystemName);
        }

        private void SetVersionInfo(ResourceName resourceName, LoadType loadType, int length, int hashCode,
            int zipLength, int zipHashCode)
        {
            GetOrAddCheckInfo(resourceName).SetVersionInfo(loadType, length, hashCode, zipLength, zipHashCode);
        }

        private void SetReadOnlyInfo(ResourceName resourceName, LoadType loadType, int length, int hashCode)
        {
            GetOrAddCheckInfo(resourceName).SetReadOnlyInfo(loadType, length, hashCode);
        }

        private void SetReadWriteInfo(ResourceName resourceName, LoadType loadType, int length, int hashCode)
        {
            GetOrAddCheckInfo(resourceName).SetReadWriteInfo(loadType, length, hashCode);
        }

        private CheckInfo GetOrAddCheckInfo(ResourceName resourceName)
        {
            CheckInfo checkInfo = null;
            if (checkInfos.TryGetValue(resourceName, out checkInfo))
            {
                return checkInfo;
            }

            checkInfo = new CheckInfo(resourceName);
            checkInfos.Add(checkInfo.ResourceName, checkInfo);

            return checkInfo;
        }

        private void RefreshCheckInfoStatus()
        {
            if (!updatableVersionListReady || !readOnlyVersionListReady || !readWriteVersionListReady)
            {
                return;
            }

            int movedCount = 0;
            int removedCount = 0;
            int updateCount = 0;
            long updateTotalLength = 0L;
            long updateTotalZipLength = 0L;
            foreach (KeyValuePair<ResourceName, CheckInfo> checkInfo in checkInfos)
            {
                CheckInfo ci = checkInfo.Value;
                ci.RefreshStatus();
                if (ci.Status == CheckStatus.StorageInReadOnly)
                {
                    resourceManager.resourceInfos.Add(ci.ResourceName,
                        new ResourceInfo(ci.ResourceName, ci.FileSystemName, ci.LoadType, ci.Length, ci.HashCode, true, true));
                }
                else if (ci.Status == CheckStatus.StorageInReadWrite)
                {
                    if (ci.NeedMoveToDisk || ci.NeedMoveToFileSystem)
                    {
                        movedCount++;
                        string resourceFullName = ci.ResourceName.FullName;
                        string resourcePath =
                            PathUtils.GetRegularPath(Path.Combine(resourceManager.readWritePath, resourceFullName));
                        if (ci.NeedMoveToDisk)
                        {
                            IFileSystem fileSystem = resourceManager.GetFileSystem(ci.ReadWriteFileSystemName, false);
                            if (!fileSystem.SaveAsFile(resourceFullName, resourcePath))
                            {
                                throw new GameFrameworkException(StringUtils.Format(
                                    "Save as file '{}' to '{}' from file system '{}' error.", resourceFullName, resourcePath, fileSystem.FullPath));
                            }

                            fileSystem.DeleteFile(resourceFullName);
                        }

                        if (ci.NeedMoveToFileSystem)
                        {
                            IFileSystem fileSystem = resourceManager.GetFileSystem(ci.FileSystemName, false);
                            if (!fileSystem.WriteFile(resourceFullName, resourcePath))
                            {
                                throw new GameFrameworkException(StringUtils.Format(
                                    "Write resource '{}' to file system '{}' error.", resourceFullName, fileSystem.FullPath));
                            }

                            if (File.Exists(resourcePath))
                            {
                                File.Delete(resourcePath);
                            }
                        }
                    }

                    resourceManager.resourceInfos.Add(ci.ResourceName,
                        new ResourceInfo(ci.ResourceName, ci.FileSystemName, ci.LoadType, ci.Length, ci.HashCode, false,
                            true));
                    resourceManager.readWriteResourceInfos.Add(ci.ResourceName,
                        new ReadWriteResourceInfo(ci.FileSystemName, ci.LoadType, ci.Length, ci.HashCode));
                }
                else if (ci.Status == CheckStatus.Update)
                {
                    resourceManager.resourceInfos.Add(ci.ResourceName,
                        new ResourceInfo(ci.ResourceName, ci.FileSystemName, ci.LoadType, ci.Length, ci.HashCode, false,
                            false));
                    updateCount++;
                    updateTotalLength += ci.Length;
                    updateTotalZipLength += ci.ZipLength;
                    resourceNeedUpdate(ci.ResourceName, ci.FileSystemName, ci.LoadType, ci.Length, ci.HashCode,
                        ci.ZipLength, ci.ZipHashCode);
                }
                else if (ci.Status == CheckStatus.Unavailable || ci.Status == CheckStatus.Disuse)
                {
                    // Do nothing.
                }
                else
                {
                    throw new GameFrameworkException(
                        StringUtils.Format("Check resources '{}' error with unknown status.",
                            ci.ResourceName.FullName));
                }

                if (ci.NeedRemove)
                {
                    removedCount++;
                    if (ci.ReadWriteUseFileSystem)
                    {
                        IFileSystem fileSystem = resourceManager.GetFileSystem(ci.ReadWriteFileSystemName, false);
                        fileSystem.DeleteFile(ci.ResourceName.FullName);
                    }
                    else
                    {
                        string resourcePath =
                            PathUtils.GetRegularPath(Path.Combine(resourceManager.readWritePath,
                                ci.ResourceName.FullName));
                        if (File.Exists(resourcePath))
                        {
                            File.Delete(resourcePath);
                        }
                    }
                }
            }

            if (movedCount > 0 || removedCount > 0)
            {
                RemoveEmptyFileSystems();
                PathUtils.RemoveEmptyDirectory(resourceManager.readWritePath);
            }

            resourceCheckComplete(movedCount, removedCount, updateCount, updateTotalLength, updateTotalZipLength);
        }

        /// <summary>
        /// 尝试恢复读写区版本资源列表。
        /// </summary>
        /// <returns>是否恢复成功。</returns>
        private bool TryRecoverReadWriteVersionList()
        {
            string file = PathUtils.GetRegularPath(Path.Combine(resourceManager.readWritePath, ResourceManager.LocalVersionListFileName));
            string backupFile = StringUtils.Format("{}.{}", file, ResourceManager.BackupExtension);

            try
            {
                if (!File.Exists(backupFile))
                {
                    return false;
                }

                if (File.Exists(file))
                {
                    File.Delete(file);
                }

                File.Move(backupFile, file);
            }
            catch
            {
                return false;
            }

            return true;
        }

        private void RemoveEmptyFileSystems()
        {
            List<string> removedFileSystemNames = null;
            foreach (KeyValuePair<string, IFileSystem> fileSystem in resourceManager.readWriteFileSystems)
            {
                if (fileSystem.Value.FileCount <= 0)
                {
                    if (removedFileSystemNames == null)
                    {
                        removedFileSystemNames = new List<string>();
                    }

                    resourceManager.fileSystemManager.DestroyFileSystem(fileSystem.Value, true);
                    removedFileSystemNames.Add(fileSystem.Key);
                }
            }

            if (removedFileSystemNames != null)
            {
                foreach (string removedFileSystemName in removedFileSystemNames)
                {
                    resourceManager.readWriteFileSystems.Remove(removedFileSystemName);
                }
            }
        }

        private void OnLoadUpdatableVersionListSuccess(string fileUri, byte[] bytes, float duration, object userData)
        {
            if (updatableVersionListReady)
            {
                throw new GameFrameworkException("Updatable version list has been parsed.");
            }

            MemoryStream memoryStream = null;
            try
            {
                memoryStream = new MemoryStream(bytes, false);
                UpdatableVersionList versionList = updatableVersionListSerializer.Deserialize(memoryStream);
                if (!versionList.IsValid)
                {
                    throw new GameFrameworkException("Deserialize updatable version list failure.");
                }

                UpdatableVersionAsset[] assets = versionList.GetAssets();
                UpdatableVersionResource[] resources = versionList.GetResources();
                UpdatableVersionFileSystem[] fileSystems = versionList.GetFileSystems();
                UpdatableVersionResourceGroup[] resourceGroups = versionList.GetResourceGroups();
                resourceManager.applicableGameVersion = versionList.ApplicableGameVersion;
                resourceManager.internalResourceVersion = versionList.InternalResourceVersion;
                resourceManager.assetInfos = new Dictionary<string, AssetInfo>(assets.Length, StringComparer.Ordinal);
                resourceManager.resourceInfos = new Dictionary<ResourceName, ResourceInfo>(resources.Length, ResourceNameComparer.COMPARER);
                resourceManager.readWriteResourceInfos = new SortedDictionary<ResourceName, ReadWriteResourceInfo>(ResourceNameComparer.COMPARER);
                ResourceGroup defaultResourceGroup = resourceManager.GetOrAddResourceGroup(string.Empty);

                foreach (UpdatableVersionFileSystem fileSystem in fileSystems)
                {
                    int[] resourceIndexes = fileSystem.GetResourceIndexes();
                    foreach (int resourceIndex in resourceIndexes)
                    {
                        UpdatableVersionResource updatableVersionResource = resources[resourceIndex];
                        SetCachedFileSystemName(new ResourceName(updatableVersionResource.Name, updatableVersionResource.Variant, updatableVersionResource.Extension), fileSystem.Name);
                    }
                }

                foreach (UpdatableVersionResource resource in resources)
                {
                    ResourceName resourceName = new ResourceName(resource.Name, resource.Variant, resource.Extension);
                    int[] assetIndexes = resource.GetAssetIndexes();
                    foreach (int assetIndex in assetIndexes)
                    {
                        UpdatableVersionAsset updatableVersionAsset = assets[assetIndex];
                        int[] dependencyAssetIndexes = updatableVersionAsset.GetDependencyAssetIndexes();
                        int index = 0;
                        string[] dependencyAssetNames = new string[dependencyAssetIndexes.Length];
                        foreach (int dependencyAssetIndex in dependencyAssetIndexes)
                        {
                            dependencyAssetNames[index++] = assets[dependencyAssetIndex].Name;
                        }

                        resourceManager.assetInfos.Add(updatableVersionAsset.Name, new AssetInfo(updatableVersionAsset.Name, resourceName, dependencyAssetNames));
                    }

                    SetVersionInfo(resourceName, (LoadType) resource.LoadType, resource.Length, resource.HashCode, resource.ZipLength, resource.ZipHashCode);
                    defaultResourceGroup.AddResource(resourceName, resource.Length, resource.ZipLength);
                }

                foreach (UpdatableVersionResourceGroup resourceGroup in resourceGroups)
                {
                    ResourceGroup group = resourceManager.GetOrAddResourceGroup(resourceGroup.Name);
                    int[] resourceIndexes = resourceGroup.GetResourceIndexes();
                    foreach (int resourceIndex in resourceIndexes)
                    {
                        UpdatableVersionResource updatableVersionResource = resources[resourceIndex];
                        group.AddResource(new ResourceName(updatableVersionResource.Name, updatableVersionResource.Variant, updatableVersionResource.Extension), updatableVersionResource.Length, updatableVersionResource.ZipLength);
                    }
                }

                updatableVersionListReady = true;
                RefreshCheckInfoStatus();
            }
            catch (Exception exception)
            {
                if (exception is GameFrameworkException)
                {
                    throw;
                }

                throw new GameFrameworkException(
                    StringUtils.Format("Parse updatable version list exception '{}'.", exception.ToString()),
                    exception);
            }
            finally
            {
                if (memoryStream != null)
                {
                    memoryStream.Dispose();
                    memoryStream = null;
                }
            }
        }

        private void OnLoadUpdatableVersionListFailure(string fileUri, string errorMessage, object userData)
        {
            throw new GameFrameworkException(StringUtils.Format(
                "Updatable version list '{}' is invalid, error message is '{}'.", fileUri,
                string.IsNullOrEmpty(errorMessage) ? "<Empty>" : errorMessage));
        }

        private void OnLoadReadOnlyVersionListSuccess(string fileUri, byte[] bytes, float duration, object userData)
        {
            if (readOnlyVersionListReady)
            {
                throw new GameFrameworkException("Read only version list has been parsed.");
            }

            MemoryStream memoryStream = null;
            try
            {
                memoryStream = new MemoryStream(bytes, false);
                LocalVersionList versionList = localVersionListSerializer.Deserialize(memoryStream);
                if (!versionList.IsValid)
                {
                    throw new GameFrameworkException("Deserialize read only version list failure.");
                }

                LocalVersionResource[] resources = versionList.GetResources();
                LocalVersionFileSystem[] fileSystems = versionList.GetFileSystems();

                foreach (LocalVersionFileSystem fileSystem in fileSystems)
                {
                    int[] resourceIndexes = fileSystem.GetResourceIndexes();
                    foreach (int resourceIndex in resourceIndexes)
                    {
                        LocalVersionResource localVersionResource = resources[resourceIndex];
                        SetCachedFileSystemName(new ResourceName(localVersionResource.Name, localVersionResource.Variant, localVersionResource.Extension),
                            fileSystem.Name);
                    }
                }

                foreach (LocalVersionResource resource in resources)
                {
                    SetReadOnlyInfo(new ResourceName(resource.Name, resource.Variant, resource.Extension),
                        (LoadType) resource.LoadType, resource.Length, resource.HashCode);
                }

                readOnlyVersionListReady = true;
                RefreshCheckInfoStatus();
            }
            catch (Exception exception)
            {
                if (exception is GameFrameworkException)
                {
                    throw;
                }

                throw new GameFrameworkException(
                    StringUtils.Format("Parse read only version list exception '{}'.", exception.ToString()),
                    exception);
            }
            finally
            {
                if (memoryStream != null)
                {
                    memoryStream.Dispose();
                    memoryStream = null;
                }
            }
        }

        private void OnLoadReadOnlyVersionListFailure(string fileUri, string errorMessage, object userData)
        {
            if (readOnlyVersionListReady)
            {
                throw new GameFrameworkException("Read only version list has been parsed.");
            }

            readOnlyVersionListReady = true;
            RefreshCheckInfoStatus();
        }

        private void OnLoadReadWriteVersionListSuccess(string fileUri, byte[] bytes, float duration, object userData)
        {
            if (readWriteVersionListReady)
            {
                throw new GameFrameworkException("Read write version list has been parsed.");
            }

            MemoryStream memoryStream = null;
            try
            {
                memoryStream = new MemoryStream(bytes, false);
                LocalVersionList versionList = localVersionListSerializer.Deserialize(memoryStream);
                if (!versionList.IsValid)
                {
                    throw new GameFrameworkException("Deserialize read write version list failure.");
                }

                LocalVersionResource[] resources = versionList.GetResources();
                LocalVersionFileSystem[] fileSystems = versionList.GetFileSystems();

                foreach (LocalVersionFileSystem fileSystem in fileSystems)
                {
                    int[] resourceIndexes = fileSystem.GetResourceIndexes();
                    foreach (int resourceIndex in resourceIndexes)
                    {
                        LocalVersionResource localVersionResource = resources[resourceIndex];
                        SetCachedFileSystemName(new ResourceName(localVersionResource.Name, localVersionResource.Variant, localVersionResource.Extension),
                            fileSystem.Name);
                    }
                }

                foreach (LocalVersionResource resource in resources)
                {
                    ResourceName resourceName = new ResourceName(resource.Name, resource.Variant, resource.Extension);
                    SetReadWriteInfo(resourceName, (LoadType) resource.LoadType, resource.Length, resource.HashCode);
                }

                readWriteVersionListReady = true;
                RefreshCheckInfoStatus();
            }
            catch (Exception exception)
            {
                if (exception is GameFrameworkException)
                {
                    throw;
                }

                throw new GameFrameworkException(
                    StringUtils.Format("Parse read write version list exception '{}'.", exception.ToString()),
                    exception);
            }
            finally
            {
                if (memoryStream != null)
                {
                    memoryStream.Dispose();
                    memoryStream = null;
                }
            }
        }

        private void OnLoadReadWriteVersionListFailure(string fileUri, string errorMessage, object userData)
        {
            if (readWriteVersionListReady)
            {
                throw new GameFrameworkException("Read write version list has been parsed.");
            }

            readWriteVersionListReady = true;
            RefreshCheckInfoStatus();
        }
    }
}