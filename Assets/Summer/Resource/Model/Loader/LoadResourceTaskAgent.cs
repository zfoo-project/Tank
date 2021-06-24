using System;
using System.Collections.Generic;
using System.IO;
using Spring.Core;
using Spring.Util;
using Summer.Base.Model;
using Summer.Base.TaskPool;
using Summer.FileSystem.Model;
using Summer.Resource.Manager;
using Summer.Resource.Model.Constant;
using Summer.Resource.Model.Vo;
using UnityEngine;

namespace Summer.Resource.Model.Loader
{
    /// <summary>
    /// 加载资源代理。
    /// </summary>
    public sealed class LoadResourceTaskAgent : ITaskAgent<LoadResourceTaskBase>
    {
        private static readonly Dictionary<string, string> CachedResourceNames = new Dictionary<string, string>(StringComparer.Ordinal);

        private static readonly HashSet<string> LoadingAssetNames = new HashSet<string>(StringComparer.Ordinal);
        private static readonly HashSet<string> LoadingResourceNames = new HashSet<string>(StringComparer.Ordinal);

        public readonly LoadResourceAgentMono loadResourceAgentMono;
        private LoadResourceTaskBase task;

        /// <summary>
        /// 初始化加载资源代理的新实例。
        /// </summary>
        /// <param name="loadResourceAgent">加载资源代理辅助器。</param>
        /// <param name="resourceHelper">资源辅助器。</param>
        /// <param name="resourceLoader">加载资源器。</param>
        /// <param name="readOnlyPath">资源只读区路径。</param>
        /// <param name="readWritePath">资源读写区路径。</param>
        /// <param name="decryptResourceCallback">解密资源回调函数。</param>
        public LoadResourceTaskAgent(LoadResourceAgentMono loadResourceAgent)
        {
            AssertionUtils.NotNull(loadResourceAgent);
            this.loadResourceAgentMono = loadResourceAgent;
            this.task = null;
        }


        /// <summary>
        /// 获取加载资源任务。
        /// </summary>
        public LoadResourceTaskBase Task
        {
            get { return task; }
        }


        public void Initialize()
        {
            this.loadResourceAgentMono.loadResourceTaskAgent = this;
        }

        /// <summary>
        /// 加载资源代理轮询。
        /// </summary>
        /// <param name="elapseSeconds">逻辑流逝时间，以秒为单位。</param>
        /// <param name="realElapseSeconds">真实流逝时间，以秒为单位。</param>
        public void Update(float elapseSeconds, float realElapseSeconds)
        {
        }

        /// <summary>
        /// 关闭并清理加载资源代理。
        /// </summary>
        public void Shutdown()
        {
            Reset();
        }

        public static void Clear()
        {
            CachedResourceNames.Clear();
            LoadingAssetNames.Clear();
            LoadingResourceNames.Clear();
        }

        /// <summary>
        /// 开始处理加载资源任务。
        /// </summary>
        /// <param name="task">要处理的加载资源任务。</param>
        /// <returns>开始处理任务的状态。</returns>
        public StartTaskStatus Start(LoadResourceTaskBase task)
        {
            if (task == null)
            {
                throw new GameFrameworkException("Task is invalid.");
            }

            this.task = task;
            this.task.StartTime = DateTime.UtcNow;
            ResourceInfo resourceInfo = this.task.ResourceInfo;

            if (!resourceInfo.Ready)
            {
                this.task.StartTime = default(DateTime);
                return StartTaskStatus.HasToWait;
            }

            if (IsAssetLoading(this.task.AssetName))
            {
                this.task.StartTime = default(DateTime);
                return StartTaskStatus.HasToWait;
            }

            var resourceLoader = SpringContext.GetBean<ResourceLoader>();
            if (!this.task.IsScene)
            {
                AssetObject assetObject = resourceLoader.assetPool.Spawn(this.task.AssetName);
                if (assetObject != null)
                {
                    OnAssetObjectReady(assetObject);
                    return StartTaskStatus.Done;
                }
            }

            foreach (string dependencyAssetName in this.task.GetDependencyAssetNames())
            {
                if (!resourceLoader.assetPool.CanSpawn(dependencyAssetName))
                {
                    this.task.StartTime = default(DateTime);
                    return StartTaskStatus.HasToWait;
                }
            }

            string resourceName = resourceInfo.ResourceName.Name;
            if (IsResourceLoading(resourceName))
            {
                this.task.StartTime = default(DateTime);
                return StartTaskStatus.HasToWait;
            }

            LoadingAssetNames.Add(this.task.AssetName);

            ResourceObject resourceObject = resourceLoader.resourcePool.Spawn(resourceName);
            if (resourceObject != null)
            {
                OnResourceObjectReady(resourceObject);
                return StartTaskStatus.CanResume;
            }

            LoadingResourceNames.Add(resourceName);

            string fullPath = null;
            if (!CachedResourceNames.TryGetValue(resourceName, out fullPath))
            {
                var readOnlyPath = SpringContext.GetBean<IResourceManager>().ReadOnlyPath;
                var readWritePath = SpringContext.GetBean<IResourceManager>().ReadWritePath;
                fullPath = PathUtils.GetRegularPath(Path.Combine(resourceInfo.StorageInReadOnly ? readOnlyPath : readWritePath, resourceInfo.UseFileSystem ? resourceInfo.FileSystemName : resourceInfo.ResourceName.FullName));
                CachedResourceNames.Add(resourceName, fullPath);
            }

            if (Application.platform == RuntimePlatform.WebGLPlayer)
            {
                loadResourceAgentMono.ReadBytes(fullPath);
            }
            else if (resourceInfo.LoadType == LoadType.LoadFromFile)
            {
                if (resourceInfo.UseFileSystem)
                {
                    IFileSystem fileSystem =
                        resourceLoader.resourceManager.GetFileSystem(resourceInfo.FileSystemName,
                            resourceInfo.StorageInReadOnly);
                    loadResourceAgentMono.ReadFile(fileSystem, resourceInfo.ResourceName.FullName);
                }
                else
                {
                    loadResourceAgentMono.ReadFile(fullPath);
                }
            }
            else if (resourceInfo.LoadType == LoadType.LoadFromMemory || resourceInfo.LoadType == LoadType.LoadFromMemoryAndQuickDecrypt || resourceInfo.LoadType == LoadType.LoadFromMemoryAndDecrypt)
            {
                if (resourceInfo.UseFileSystem)
                {
                    IFileSystem fileSystem =
                        resourceLoader.resourceManager.GetFileSystem(resourceInfo.FileSystemName,
                            resourceInfo.StorageInReadOnly);
                    loadResourceAgentMono.ReadBytes(fileSystem, resourceInfo.ResourceName.FullName);
                }
                else
                {
                    loadResourceAgentMono.ReadBytes(fullPath);
                }
            }
            else
            {
                throw new GameFrameworkException(StringUtils.Format("Resource load type '{}' is not supported.",
                    resourceInfo.LoadType.ToString()));
            }

            return StartTaskStatus.CanResume;
        }

        /// <summary>
        /// 重置加载资源代理。
        /// </summary>
        public void Reset()
        {
            loadResourceAgentMono.Reset();
            task = null;
        }

        private static bool IsAssetLoading(string assetName)
        {
            return LoadingAssetNames.Contains(assetName);
        }

        private static bool IsResourceLoading(string resourceName)
        {
            return LoadingResourceNames.Contains(resourceName);
        }

        private void OnAssetObjectReady(AssetObject assetObject)
        {
            var resourceLoader = SpringContext.GetBean<ResourceLoader>();

            loadResourceAgentMono.Reset();

            object asset = assetObject.Target;
            if (task.IsScene)
            {
                resourceLoader.sceneToAssetMap.Add(task.AssetName, asset);
            }

            task.OnLoadAssetSuccess(this, asset, (float) (DateTime.UtcNow - task.StartTime).TotalSeconds);
            task.Done = true;
        }

        private void OnResourceObjectReady(ResourceObject resourceObject)
        {
            task.LoadMain(this, resourceObject);
        }

        public void OnError(LoadResourceStatus status, string errorMessage)
        {
            loadResourceAgentMono.Reset();
            task.OnLoadAssetFailure(this, status, errorMessage);
            LoadingAssetNames.Remove(task.AssetName);
            LoadingResourceNames.Remove(task.ResourceInfo.ResourceName.Name);
            task.Done = true;
        }

        public void OnLoadResourceAgentHelperUpdate(LoadResourceProgress type, float progress)
        {
            task.OnLoadAssetUpdate(this, type, progress);
        }

        public void OnLoadResourceAgentHelperReadFileComplete(object resource)
        {
            var resourceLoader = SpringContext.GetBean<ResourceLoader>();

            var resourceObject = ResourceObject.Create(task.ResourceInfo.ResourceName.Name, resource);
            resourceLoader.resourcePool.Register(resourceObject, true);
            LoadingResourceNames.Remove(task.ResourceInfo.ResourceName.Name);
            OnResourceObjectReady(resourceObject);
        }

        public void OnLoadResourceAgentHelperReadBytesComplete(byte[] bytes)
        {
            loadResourceAgentMono.ParseBytes(bytes);
        }

        public void OnLoadResourceAgentHelperParseBytesComplete(object resource)
        {
            var resourceLoader = SpringContext.GetBean<ResourceLoader>();
            var resourceObject = ResourceObject.Create(task.ResourceInfo.ResourceName.Name, resource);
            resourceLoader.resourcePool.Register(resourceObject, true);
            LoadingResourceNames.Remove(task.ResourceInfo.ResourceName.Name);
            OnResourceObjectReady(resourceObject);
        }

        public void OnLoadResourceAgentHelperLoadComplete(object resource)
        {
            var resourceLoader = SpringContext.GetBean<ResourceLoader>();
            AssetObject assetObject = null;
            if (task.IsScene)
            {
                assetObject = resourceLoader.assetPool.Spawn(task.AssetName);
            }

            if (assetObject == null)
            {
                List<object> dependencyAssets = task.GetDependencyAssets();
                assetObject = AssetObject.Create(task.AssetName, resource, dependencyAssets, task.ResourceObject.Target);
                resourceLoader.assetPool.Register(assetObject, true);
                resourceLoader.assetToResourceMap.Add(resource, task.ResourceObject.Target);
                foreach (object dependencyAsset in dependencyAssets)
                {
                    object dependencyResource = null;
                    if (resourceLoader.assetToResourceMap.TryGetValue(dependencyAsset, out dependencyResource))
                    {
                        task.ResourceObject.AddDependencyResource(dependencyResource);
                    }
                    else
                    {
                        throw new GameFrameworkException("Can not find dependency resource.");
                    }
                }
            }

            LoadingAssetNames.Remove(task.AssetName);
            OnAssetObjectReady(assetObject);
        }
    }
}