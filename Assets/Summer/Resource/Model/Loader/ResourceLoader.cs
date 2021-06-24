using System;
using System.Collections.Generic;
using System.IO;
using Summer.Base.Model;
using Summer.Base.TaskPool;
using Summer.FileSystem.Model;
using Summer.ObjectPool;
using Summer.Resource.Manager;
using Summer.Resource.Model.Callback;
using Summer.Resource.Model.Constant;
using Summer.Resource.Model.Vo;
using Spring.Collection.Reference;
using Spring.Core;
using Spring.Util;

namespace Summer.Resource.Model.Loader
{
    /// <summary>
    /// 加载资源器。
    /// </summary>
    public sealed class ResourceLoader
    {
        [Autowired]
        public ResourceManager resourceManager;

        [Autowired]
        private IObjectPoolManager objectPoolManager;

        private readonly TaskPool<LoadResourceTaskBase> taskPool;
        public readonly Dictionary<object, int> assetDependencyCount;
        public readonly Dictionary<object, int> resourceDependencyCount;
        public readonly Dictionary<object, object> assetToResourceMap;
        public readonly Dictionary<string, object> sceneToAssetMap;
        public readonly LoadBytesCallbacks loadBytesCallbacks;
        public IObjectPool<AssetObject> assetPool;
        public IObjectPool<ResourceObject> resourcePool;

        public ResourceLoader()
        {
            taskPool = new TaskPool<LoadResourceTaskBase>();
            assetDependencyCount = new Dictionary<object, int>();
            resourceDependencyCount = new Dictionary<object, int>();
            assetToResourceMap = new Dictionary<object, object>();
            sceneToAssetMap = new Dictionary<string, object>(StringComparer.Ordinal);
            loadBytesCallbacks = new LoadBytesCallbacks(OnLoadBinarySuccess, OnLoadBinaryFailure);
            assetPool = null;
            resourcePool = null;
        }

        /// <summary>
        /// 获取加载资源代理总数量。
        /// </summary>
        public int TotalAgentCount
        {
            get { return taskPool.TotalAgentCount; }
        }

        /// <summary>
        /// 获取可用加载资源代理数量。
        /// </summary>
        public int FreeAgentCount
        {
            get { return taskPool.FreeAgentCount; }
        }

        /// <summary>
        /// 获取工作中加载资源代理数量。
        /// </summary>
        public int WorkingAgentCount
        {
            get { return taskPool.WorkingAgentCount; }
        }

        /// <summary>
        /// 获取等待加载资源任务数量。
        /// </summary>
        public int WaitingTaskCount
        {
            get { return taskPool.WaitingTaskCount; }
        }

        /// <summary>
        /// 获取或设置资源对象池自动释放可释放对象的间隔秒数。
        /// </summary>
        public float AssetAutoReleaseInterval
        {
            get { return assetPool.AutoReleaseInterval; }
            set { assetPool.AutoReleaseInterval = value; }
        }

        /// <summary>
        /// 获取或设置资源对象池的容量。
        /// </summary>
        public int AssetCapacity
        {
            get { return assetPool.Capacity; }
            set { assetPool.Capacity = value; }
        }

        /// <summary>
        /// 获取或设置资源对象池对象过期秒数。
        /// </summary>
        public float AssetExpireTime
        {
            get { return assetPool.ExpireTime; }
            set { assetPool.ExpireTime = value; }
        }

        /// <summary>
        /// 获取或设置资源对象池的优先级。
        /// </summary>
        public int AssetPriority
        {
            get { return assetPool.Priority; }
            set { assetPool.Priority = value; }
        }

        /// <summary>
        /// 获取或设置资源对象池自动释放可释放对象的间隔秒数。
        /// </summary>
        public float ResourceAutoReleaseInterval
        {
            get { return resourcePool.AutoReleaseInterval; }
            set { resourcePool.AutoReleaseInterval = value; }
        }

        /// <summary>
        /// 获取或设置资源对象池的容量。
        /// </summary>
        public int ResourceCapacity
        {
            get { return resourcePool.Capacity; }
            set { resourcePool.Capacity = value; }
        }

        /// <summary>
        /// 获取或设置资源对象池对象过期秒数。
        /// </summary>
        public float ResourceExpireTime
        {
            get { return resourcePool.ExpireTime; }
            set { resourcePool.ExpireTime = value; }
        }

        /// <summary>
        /// 获取或设置资源对象池的优先级。
        /// </summary>
        public int ResourcePriority
        {
            get { return resourcePool.Priority; }
            set { resourcePool.Priority = value; }
        }

        /// <summary>
        /// 设置对象池管理器。
        /// </summary>
        /// <param name="objectPoolManager">对象池管理器。</param>
        [BeforePostConstruct]
        private void Init()
        {
            assetPool = objectPoolManager.CreateMultiSpawnObjectPool<AssetObject>();
            resourcePool = objectPoolManager.CreateMultiSpawnObjectPool<ResourceObject>();
        }

        /// <summary>
        /// 加载资源器轮询。
        /// </summary>
        /// <param name="elapseSeconds">逻辑流逝时间，以秒为单位。</param>
        /// <param name="realElapseSeconds">真实流逝时间，以秒为单位。</param>
        public void Update(float elapseSeconds, float realElapseSeconds)
        {
            taskPool.Update(elapseSeconds, realElapseSeconds);
        }

        /// <summary>
        /// 关闭并清理加载资源器。
        /// </summary>
        public void Shutdown()
        {
            taskPool.Shutdown();
            assetDependencyCount.Clear();
            resourceDependencyCount.Clear();
            assetToResourceMap.Clear();
            sceneToAssetMap.Clear();
            LoadResourceTaskAgent.Clear();
        }


        /// <summary>
        /// 增加加载资源代理辅助器。
        /// </summary>
        /// <param name="loadResourceAgent">要增加的加载资源代理辅助器。</param>
        /// <param name="resourceHelper">资源辅助器。</param>
        /// <param name="readOnlyPath">资源只读区路径。</param>
        /// <param name="readWritePath">资源读写区路径。</param>
        /// <param name="decryptResourceCallback">要设置的解密资源回调函数。</param>
        public void AddLoadResourceAgent(LoadResourceAgentMono loadResourceAgent)
        {
            AssertionUtils.NotNull(loadResourceAgent);
            var agent = new LoadResourceTaskAgent(loadResourceAgent);
            taskPool.AddAgent(agent);
        }

        /// <summary>
        /// 检查资源是否存在。
        /// </summary>
        /// <param name="assetName">要检查资源的名称。</param>
        /// <returns>检查资源是否存在的结果。</returns>
        public HasAssetResult HasAsset(string assetName)
        {
            ResourceInfo resourceInfo = GetResourceInfo(assetName);
            if (resourceInfo == null)
            {
                return HasAssetResult.NotExist;
            }

            if (!resourceInfo.Ready)
            {
                return HasAssetResult.NotReady;
            }

            if (resourceInfo.UseFileSystem)
            {
                return resourceInfo.IsLoadFromBinary ? HasAssetResult.BinaryOnFileSystem : HasAssetResult.AssetOnFileSystem;
            }
            else
            {
                return resourceInfo.IsLoadFromBinary ? HasAssetResult.BinaryOnDisk : HasAssetResult.AssetOnDisk;
            }
        }

        /// <summary>
        /// 异步加载资源。
        /// </summary>
        /// <param name="assetName">要加载资源的名称。</param>
        /// <param name="assetType">要加载资源的类型。</param>
        /// <param name="priority">加载资源的优先级。</param>
        /// <param name="loadAssetCallbacks">加载资源回调函数集。</param>
        /// <param name="userData">用户自定义数据。</param>
        public void LoadAsset(string assetName, Type assetType, int priority, LoadAssetCallbacks loadAssetCallbacks, object userData)
        {
            ResourceInfo resourceInfo = null;
            string[] dependencyAssetNames = null;
            if (!CheckAsset(assetName, out resourceInfo, out dependencyAssetNames))
            {
                string errorMessage = StringUtils.Format("Can not load asset '{}'.", assetName);
                loadAssetCallbacks.LoadAssetFailureCallback(assetName, resourceInfo != null && !resourceInfo.Ready ? LoadResourceStatus.NotReady : LoadResourceStatus.NotExist, errorMessage, userData);
                return;
            }

            if (resourceInfo.IsLoadFromBinary)
            {
                string errorMessage = StringUtils.Format("Can not load asset '{}' which is a binary asset.", assetName);
                loadAssetCallbacks.LoadAssetFailureCallback(assetName, LoadResourceStatus.TypeError, errorMessage, userData);
                return;
            }

            var mainTask = LoadAssetTask.Create(assetName, assetType, priority, resourceInfo, dependencyAssetNames, loadAssetCallbacks, userData);
            foreach (string dependencyAssetName in dependencyAssetNames)
            {
                if (!LoadDependencyAsset(dependencyAssetName, priority, mainTask, userData))
                {
                    string errorMessage = StringUtils.Format("Can not load dependency asset '{}' when load asset '{}'.", dependencyAssetName, assetName);
                    loadAssetCallbacks.LoadAssetFailureCallback(assetName, LoadResourceStatus.DependencyError, errorMessage, userData);
                    return;
                }
            }

            taskPool.AddTask(mainTask);
            if (!resourceInfo.Ready)
            {
                resourceManager.UpdateResource(resourceInfo.ResourceName);
            }
        }

        /// <summary>
        /// 卸载资源。
        /// </summary>
        /// <param name="asset">要卸载的资源。</param>
        public void UnloadAsset(object asset)
        {
            assetPool.Unspawn(asset);
        }

        /// <summary>
        /// 异步加载场景。
        /// </summary>
        /// <param name="sceneAssetName">要加载场景资源的名称。</param>
        /// <param name="priority">加载场景资源的优先级。</param>
        /// <param name="loadSceneCallbacks">加载场景回调函数集。</param>
        /// <param name="userData">用户自定义数据。</param>
        public void LoadScene(string sceneAssetName, int priority, LoadSceneCallbacks loadSceneCallbacks, object userData)
        {
            ResourceInfo resourceInfo = null;
            string[] dependencyAssetNames = null;
            if (!CheckAsset(sceneAssetName, out resourceInfo, out dependencyAssetNames))
            {
                string errorMessage = StringUtils.Format("Can not load scene '{}'.", sceneAssetName);
                loadSceneCallbacks.LoadSceneFailureCallback(sceneAssetName, resourceInfo != null && !resourceInfo.Ready ? LoadResourceStatus.NotReady : LoadResourceStatus.NotExist, errorMessage, userData);
                return;
            }

            if (resourceInfo.IsLoadFromBinary)
            {
                string errorMessage = StringUtils.Format("Can not load scene asset '{}' which is a binary asset.", sceneAssetName);
                loadSceneCallbacks.LoadSceneFailureCallback(sceneAssetName, LoadResourceStatus.TypeError, errorMessage, userData);
                return;
            }

            LoadSceneTask mainTask = LoadSceneTask.Create(sceneAssetName, priority, resourceInfo, dependencyAssetNames, loadSceneCallbacks, userData);
            foreach (string dependencyAssetName in dependencyAssetNames)
            {
                if (!LoadDependencyAsset(dependencyAssetName, priority, mainTask, userData))
                {
                    string errorMessage = StringUtils.Format("Can not load dependency asset '{}' when load scene '{}'.", dependencyAssetName, sceneAssetName);
                    loadSceneCallbacks.LoadSceneFailureCallback(sceneAssetName, LoadResourceStatus.DependencyError, errorMessage, userData);
                    return;
                }
            }

            taskPool.AddTask(mainTask);
            if (!resourceInfo.Ready)
            {
                resourceManager.UpdateResource(resourceInfo.ResourceName);
            }
        }

        /// <summary>
        /// 异步卸载场景。
        /// </summary>
        /// <param name="sceneAssetName">要卸载场景资源的名称。</param>
        /// <param name="unloadSceneCallbacks">卸载场景回调函数集。</param>
        /// <param name="userData">用户自定义数据。</param>
        public void UnloadScene(string sceneAssetName, UnloadSceneCallbacks unloadSceneCallbacks, object userData)
        {
            object asset = null;
            if (sceneToAssetMap.TryGetValue(sceneAssetName, out asset))
            {
                sceneToAssetMap.Remove(sceneAssetName);
                assetPool.Unspawn(asset);
                assetPool.ReleaseObject(asset);
            }
            else
            {
                throw new GameFrameworkException(StringUtils.Format("Can not find asset of scene '{}'.", sceneAssetName));
            }

            resourceManager.simpleLoadResource.UnloadScene(sceneAssetName, unloadSceneCallbacks, userData);
        }

        /// <summary>
        /// 获取二进制资源的实际路径。
        /// </summary>
        /// <param name="binaryAssetName">要获取实际路径的二进制资源的名称。</param>
        /// <returns>二进制资源的实际路径。</returns>
        /// <remarks>此方法仅适用于二进制资源存储在磁盘（而非文件系统）中的情况。若二进制资源存储在文件系统中时，返回值将始终为空。</remarks>
        public string GetBinaryPath(string binaryAssetName)
        {
            ResourceInfo resourceInfo = GetResourceInfo(binaryAssetName);
            if (resourceInfo == null)
            {
                return null;
            }

            if (!resourceInfo.Ready)
            {
                return null;
            }

            if (!resourceInfo.IsLoadFromBinary)
            {
                return null;
            }

            if (resourceInfo.UseFileSystem)
            {
                return null;
            }

            return PathUtils.GetRegularPath(Path.Combine(resourceInfo.StorageInReadOnly ? resourceManager.readOnlyPath : resourceManager.readWritePath, resourceInfo.ResourceName.FullName));
        }

        /// <summary>
        /// 获取二进制资源的实际路径。
        /// </summary>
        /// <param name="binaryAssetName">要获取实际路径的二进制资源的名称。</param>
        /// <param name="storageInReadOnly">二进制资源是否存储在只读区中。</param>
        /// <param name="storageInFileSystem">二进制资源是否存储在文件系统中。</param>
        /// <param name="relativePath">二进制资源或存储二进制资源的文件系统，相对于只读区或者读写区的相对路径。</param>
        /// <param name="fileName">若二进制资源存储在文件系统中，则指示二进制资源在文件系统中的名称，否则此参数返回空。</param>
        /// <returns>是否获取二进制资源的实际路径成功。</returns>
        public bool GetBinaryPath(string binaryAssetName, out bool storageInReadOnly, out bool storageInFileSystem,
            out string relativePath, out string fileName)
        {
            storageInReadOnly = false;
            storageInFileSystem = false;
            relativePath = null;
            fileName = null;

            ResourceInfo resourceInfo = GetResourceInfo(binaryAssetName);
            if (resourceInfo == null)
            {
                return false;
            }

            if (!resourceInfo.Ready)
            {
                return false;
            }

            if (!resourceInfo.IsLoadFromBinary)
            {
                return false;
            }

            storageInReadOnly = resourceInfo.StorageInReadOnly;
            if (resourceInfo.UseFileSystem)
            {
                storageInFileSystem = true;
                relativePath = StringUtils.Format("{}.{}", resourceInfo.FileSystemName, ResourceManager.DefaultExtension);
                fileName = resourceInfo.ResourceName.FullName;
            }
            else
            {
                relativePath = resourceInfo.ResourceName.FullName;
            }

            return true;
        }

        /// <summary>
        /// 获取二进制资源的长度。
        /// </summary>
        /// <param name="binaryAssetName">要获取长度的二进制资源的名称。</param>
        /// <returns>二进制资源的长度。</returns>
        public int GetBinaryLength(string binaryAssetName)
        {
            ResourceInfo resourceInfo = GetResourceInfo(binaryAssetName);
            if (resourceInfo == null)
            {
                return -1;
            }

            if (!resourceInfo.Ready)
            {
                return -1;
            }

            if (!resourceInfo.IsLoadFromBinary)
            {
                return -1;
            }

            return resourceInfo.Length;
        }

        /// <summary>
        /// 异步加载二进制资源。
        /// </summary>
        /// <param name="binaryAssetName">要加载二进制资源的名称。</param>
        /// <param name="loadBinaryCallbacks">加载二进制资源回调函数集。</param>
        /// <param name="userData">用户自定义数据。</param>
        public void LoadBinary(string binaryAssetName, LoadBinaryCallbacks loadBinaryCallbacks, object userData)
        {
            ResourceInfo resourceInfo = GetResourceInfo(binaryAssetName);
            if (resourceInfo == null)
            {
                string errorMessage = StringUtils.Format("Can not load binary '{}' which is not exist.", binaryAssetName);
                loadBinaryCallbacks.LoadBinaryFailureCallback(binaryAssetName, LoadResourceStatus.NotExist, errorMessage, userData);
                return;
            }

            if (!resourceInfo.Ready)
            {
                string errorMessage = StringUtils.Format("Can not load binary '{}' which is not ready.", binaryAssetName);
                loadBinaryCallbacks.LoadBinaryFailureCallback(binaryAssetName, LoadResourceStatus.NotReady, errorMessage, userData);
                return;
            }

            if (!resourceInfo.IsLoadFromBinary)
            {
                string errorMessage = StringUtils.Format("Can not load binary '{}' which is not a binary asset.", binaryAssetName);
                loadBinaryCallbacks.LoadBinaryFailureCallback(binaryAssetName, LoadResourceStatus.TypeError, errorMessage, userData);
                return;
            }

            if (resourceInfo.UseFileSystem)
            {
                loadBinaryCallbacks.LoadBinarySuccessCallback(binaryAssetName, LoadBinaryFromFileSystem(binaryAssetName), 0f, userData);
            }
            else
            {
                string path = PathUtils.GetRemotePath(Path.Combine(
                    resourceInfo.StorageInReadOnly ? resourceManager.readOnlyPath : resourceManager.readWritePath, resourceInfo.ResourceName.FullName));
                resourceManager.simpleLoadResource.LoadBytes(path, loadBytesCallbacks, LoadBinaryInfo.Create(binaryAssetName, resourceInfo, loadBinaryCallbacks, userData));
            }
        }

        /// <summary>
        /// 从文件系统中加载二进制资源。
        /// </summary>
        /// <param name="binaryAssetName">要加载二进制资源的名称。</param>
        /// <returns>存储加载二进制资源的二进制流。</returns>
        public byte[] LoadBinaryFromFileSystem(string binaryAssetName)
        {
            ResourceInfo resourceInfo = GetResourceInfo(binaryAssetName);
            if (resourceInfo == null)
            {
                throw new Exception(StringUtils.Format("Can not load binary '{}' from file system which is not exist.", binaryAssetName));
            }

            if (!resourceInfo.Ready)
            {
                throw new Exception(StringUtils.Format("Can not load binary '{}' from file system which is not ready.", binaryAssetName));
            }

            if (!resourceInfo.IsLoadFromBinary)
            {
                throw new Exception(StringUtils.Format("Can not load binary '{}' from file system which is not a binary asset.", binaryAssetName));
            }

            if (!resourceInfo.UseFileSystem)
            {
                throw new Exception(StringUtils.Format("Can not load binary '{}' from file system which is not use file system.", binaryAssetName));
            }

            IFileSystem fileSystem = resourceManager.GetFileSystem(resourceInfo.FileSystemName, resourceInfo.StorageInReadOnly);
            byte[] bytes = fileSystem.ReadFile(resourceInfo.ResourceName.FullName);
            return bytes;
        }

        /// <summary>
        /// 从文件系统中加载二进制资源。
        /// </summary>
        /// <param name="binaryAssetName">要加载二进制资源的名称。</param>
        /// <param name="buffer">存储加载二进制资源的二进制流。</param>
        /// <param name="startIndex">存储加载二进制资源的二进制流的起始位置。</param>
        /// <param name="length">存储加载二进制资源的二进制流的长度。</param>
        /// <returns>实际加载了多少字节。</returns>
        public int LoadBinaryFromFileSystem(string binaryAssetName, byte[] buffer, int startIndex, int length)
        {
            ResourceInfo resourceInfo = GetResourceInfo(binaryAssetName);
            if (resourceInfo == null)
            {
                throw new Exception(StringUtils.Format("Can not load binary '{}' from file system which is not exist.", binaryAssetName));
            }

            if (!resourceInfo.Ready)
            {
                throw new Exception(StringUtils.Format("Can not load binary '{}' from file system which is not ready.", binaryAssetName));
            }

            if (!resourceInfo.IsLoadFromBinary)
            {
                throw new Exception(StringUtils.Format("Can not load binary '{}' from file system which is not a binary asset.", binaryAssetName));
            }

            if (!resourceInfo.UseFileSystem)
            {
                throw new Exception(StringUtils.Format("Can not load binary '{}' from file system which is not use file system.", binaryAssetName));
            }

            IFileSystem fileSystem = resourceManager.GetFileSystem(resourceInfo.FileSystemName, resourceInfo.StorageInReadOnly);
            int bytesRead = fileSystem.ReadFile(resourceInfo.ResourceName.FullName, buffer, startIndex, length);
            return bytesRead;
        }

        /// <summary>
        /// 从文件系统中加载二进制资源的片段。
        /// </summary>
        /// <param name="binaryAssetName">要加载片段的二进制资源的名称。</param>
        /// <param name="offset">要加载片段的偏移。</param>
        /// <param name="length">要加载片段的长度。</param>
        /// <returns>存储加载二进制资源片段内容的二进制流。</returns>
        public byte[] LoadBinarySegmentFromFileSystem(string binaryAssetName, int offset, int length)
        {
            ResourceInfo resourceInfo = GetResourceInfo(binaryAssetName);
            if (resourceInfo == null)
            {
                throw new Exception(StringUtils.Format("Can not load binary '{}' from file system which is not exist.", binaryAssetName));
            }

            if (!resourceInfo.Ready)
            {
                throw new Exception(StringUtils.Format("Can not load binary '{}' from file system which is not ready.", binaryAssetName));
            }

            if (!resourceInfo.IsLoadFromBinary)
            {
                throw new Exception(StringUtils.Format("Can not load binary '{}' from file system which is not a binary asset.", binaryAssetName));
            }

            if (!resourceInfo.UseFileSystem)
            {
                throw new Exception(StringUtils.Format("Can not load binary '{}' from file system which is not use file system.", binaryAssetName));
            }

            IFileSystem fileSystem = resourceManager.GetFileSystem(resourceInfo.FileSystemName, resourceInfo.StorageInReadOnly);
            byte[] bytes = fileSystem.ReadFileSegment(resourceInfo.ResourceName.FullName, offset, length);
            if (bytes == null)
            {
                return null;
            }

            return bytes;
        }

        /// <summary>
        /// 从文件系统中加载二进制资源的片段。
        /// </summary>
        /// <param name="binaryAssetName">要加载片段的二进制资源的名称。</param>
        /// <param name="offset">要加载片段的偏移。</param>
        /// <param name="buffer">存储加载二进制资源片段内容的二进制流。</param>
        /// <param name="startIndex">存储加载二进制资源片段内容的二进制流的起始位置。</param>
        /// <param name="length">要加载片段的长度。</param>
        /// <returns>实际加载了多少字节。</returns>
        public int LoadBinarySegmentFromFileSystem(string binaryAssetName, int offset, byte[] buffer, int startIndex,
            int length)
        {
            ResourceInfo resourceInfo = GetResourceInfo(binaryAssetName);
            if (resourceInfo == null)
            {
                throw new Exception(StringUtils.Format("Can not load binary '{}' from file system which is not exist.",
                    binaryAssetName));
            }

            if (!resourceInfo.Ready)
            {
                throw new Exception(StringUtils.Format("Can not load binary '{}' from file system which is not ready.", binaryAssetName));
            }

            if (!resourceInfo.IsLoadFromBinary)
            {
                throw new Exception(StringUtils.Format("Can not load binary '{}' from file system which is not a binary asset.", binaryAssetName));
            }

            if (!resourceInfo.UseFileSystem)
            {
                throw new Exception(StringUtils.Format("Can not load binary '{}' from file system which is not use file system.", binaryAssetName));
            }

            IFileSystem fileSystem = resourceManager.GetFileSystem(resourceInfo.FileSystemName, resourceInfo.StorageInReadOnly);
            int bytesRead = fileSystem.ReadFileSegment(resourceInfo.ResourceName.FullName, offset, buffer, startIndex, length);
            return bytesRead;
        }

        /// <summary>
        /// 获取所有加载资源任务的信息。
        /// </summary>
        /// <returns>所有加载资源任务的信息。</returns>
        public TaskInfo[] GetAllLoadAssetInfos()
        {
            return taskPool.GetAllTaskInfos();
        }

        private bool LoadDependencyAsset(string assetName, int priority, LoadResourceTaskBase mainTask, object userData)
        {
            if (mainTask == null)
            {
                throw new GameFrameworkException("Main task is invalid.");
            }

            ResourceInfo resourceInfo = null;
            string[] dependencyAssetNames = null;
            if (!CheckAsset(assetName, out resourceInfo, out dependencyAssetNames))
            {
                return false;
            }

            if (resourceInfo.IsLoadFromBinary)
            {
                return false;
            }

            LoadDependencyAssetTask dependencyTask = LoadDependencyAssetTask.Create(assetName, priority, resourceInfo, dependencyAssetNames, mainTask, userData);
            foreach (string dependencyAssetName in dependencyAssetNames)
            {
                if (!LoadDependencyAsset(dependencyAssetName, priority, dependencyTask, userData))
                {
                    return false;
                }
            }

            taskPool.AddTask(dependencyTask);
            if (!resourceInfo.Ready)
            {
                resourceManager.UpdateResource(resourceInfo.ResourceName);
            }

            return true;
        }

        private ResourceInfo GetResourceInfo(string assetName)
        {
            if (string.IsNullOrEmpty(assetName))
            {
                return null;
            }

            AssetInfo assetInfo = resourceManager.GetAssetInfo(assetName);
            if (assetInfo == null)
            {
                return null;
            }

            return resourceManager.GetResourceInfo(assetInfo.ResourceName);
        }

        private bool CheckAsset(string assetName, out ResourceInfo resourceInfo, out string[] dependencyAssetNames)
        {
            resourceInfo = null;
            dependencyAssetNames = null;

            if (string.IsNullOrEmpty(assetName))
            {
                return false;
            }

            AssetInfo assetInfo = resourceManager.GetAssetInfo(assetName);
            if (assetInfo == null)
            {
                return false;
            }

            resourceInfo = resourceManager.GetResourceInfo(assetInfo.ResourceName);
            if (resourceInfo == null)
            {
                return false;
            }

            dependencyAssetNames = assetInfo.GetDependencyAssetNames();
            return resourceInfo.Ready;
        }


        private void OnLoadBinarySuccess(string fileUri, byte[] bytes, float duration, object userData)
        {
            LoadBinaryInfo loadBinaryInfo = (LoadBinaryInfo) userData;
            if (loadBinaryInfo == null)
            {
                throw new Exception("Load binary info is invalid.");
            }

            loadBinaryInfo.LoadBinaryCallbacks.LoadBinarySuccessCallback(loadBinaryInfo.BinaryAssetName, bytes, duration, loadBinaryInfo.UserData);
            ReferenceCache.Release(loadBinaryInfo);
        }

        private void OnLoadBinaryFailure(string fileUri, string errorMessage, object userData)
        {
            LoadBinaryInfo loadBinaryInfo = (LoadBinaryInfo) userData;
            if (loadBinaryInfo == null)
            {
                throw new Exception("Load binary info is invalid.");
            }

            loadBinaryInfo.LoadBinaryCallbacks.LoadBinaryFailureCallback(loadBinaryInfo.BinaryAssetName, LoadResourceStatus.AssetError, errorMessage, loadBinaryInfo.UserData);
            ReferenceCache.Release(loadBinaryInfo);
        }
    }
}