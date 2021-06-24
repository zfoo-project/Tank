using System;
using System.Collections.Generic;
using System.IO;
using Summer.Base.Model;
using Summer.Base.TaskPool;
using Summer.FileSystem;
using Summer.FileSystem.Model;
using Summer.Resource.Manager;
using Summer.Resource.Model.Callback;
using Summer.Resource.Model.Constant;
using Summer.Resource.Model.Eve;
using Summer.Resource.Model.Group;
using Summer.Resource.Model.Loader;
using Summer.Resource.Model.UpdatableVersion;
using Summer.Resource.Model.Vo;
using Spring.Core;
using Spring.Event;
using Spring.Util;

namespace Summer.Resource
{
    /// <summary>
    /// 资源管理器。
    /// </summary>
    public sealed class ResourceManager : AbstractManager, IResourceManager
    {
        public const string RemoteVersionListFileName = "GameFrameworkVersion.dat";
        public const string LocalVersionListFileName = "GameFrameworkList.dat";
        public const string DefaultExtension = "dat";
        public const string BackupExtension = "bak";
        public const int FileSystemMaxFileCount = 1024 * 16;
        public const int FileSystemMaxBlockCount = 1024 * 256;

        public Dictionary<string, AssetInfo> assetInfos;
        public Dictionary<ResourceName, ResourceInfo> resourceInfos;

        public SortedDictionary<ResourceName, ReadWriteResourceInfo> readWriteResourceInfos;
        public readonly Dictionary<string, IFileSystem> readOnlyFileSystems;
        public readonly Dictionary<string, IFileSystem> readWriteFileSystems;
        public readonly Dictionary<string, ResourceGroup> resourceGroups;

        [Autowired]
        public IFileSystemManager fileSystemManager;

        [Autowired]
        public ResourceIniter resourceIniter;

        [Autowired]
        public VersionListProcessor versionListProcessor;

        [Autowired]
        public ResourceChecker resourceChecker;

        [Autowired]
        public ResourceUpdater resourceUpdater;

        [Autowired]
        public ResourceLoader resourceLoader;

        [Autowired]
        public SimpleLoadResourceMono simpleLoadResource;

        public string readOnlyPath;
        public string readWritePath;
        public ResourceMode resourceMode;
        public string updatePrefixUri;


        public string applicableGameVersion;
        public int internalResourceVersion;
        public MemoryStream decompressCachedStream;
        public UpdateVersionListCallbacks updateVersionListCallbacks;
        public UpdateResourcesCompleteCallback updateResourcesCompleteCallback;

        /// <summary>
        /// 初始化资源管理器的新实例。
        /// </summary>
        public ResourceManager()
        {
            assetInfos = null;
            resourceInfos = null;
            readWriteResourceInfos = null;
            readOnlyFileSystems = new Dictionary<string, IFileSystem>(StringComparer.Ordinal);
            readWriteFileSystems = new Dictionary<string, IFileSystem>(StringComparer.Ordinal);
            resourceGroups = new Dictionary<string, ResourceGroup>(StringComparer.Ordinal);

            readOnlyPath = null;
            readWritePath = null;
            resourceMode = ResourceMode.Unspecified;
            updatePrefixUri = null;
            applicableGameVersion = null;
            internalResourceVersion = 0;
            decompressCachedStream = null;
            updateVersionListCallbacks = null;
            updateResourcesCompleteCallback = null;
        }

        /// <summary>
        /// 获取游戏框架模块优先级。
        /// </summary>
        /// <remarks>优先级较高的模块会优先轮询，并且关闭操作会后进行。</remarks>
        public override int Priority
        {
            get { return 70; }
        }

        /// <summary>
        /// 获取资源只读区路径。
        /// </summary>
        public string ReadOnlyPath
        {
            get { return readOnlyPath; }
        }

        /// <summary>
        /// 获取资源读写区路径。
        /// </summary>
        public string ReadWritePath
        {
            get { return readWritePath; }
        }

        /// <summary>
        /// 获取资源模式。
        /// </summary>
        public ResourceMode ResourceMode
        {
            get { return resourceMode; }
        }


        /// <summary>
        /// 获取当前资源适用的游戏版本号。
        /// </summary>
        public string ApplicableGameVersion
        {
            get { return applicableGameVersion; }
        }

        /// <summary>
        /// 获取当前内部资源版本号。
        /// </summary>
        public int InternalResourceVersion
        {
            get { return internalResourceVersion; }
        }

        /// <summary>
        /// 获取资源数量。
        /// </summary>
        public int AssetCount
        {
            get { return assetInfos != null ? assetInfos.Count : 0; }
        }

        /// <summary>
        /// 获取资源数量。
        /// </summary>
        public int ResourceCount
        {
            get { return resourceInfos != null ? resourceInfos.Count : 0; }
        }

        /// <summary>
        /// 获取资源组数量。
        /// </summary>
        public int ResourceGroupCount
        {
            get { return resourceGroups.Count; }
        }

        /// <summary>
        /// 获取或设置资源更新下载地址前缀。
        /// </summary>
        public string UpdatePrefixUri
        {
            get { return updatePrefixUri; }
            set { updatePrefixUri = value; }
        }

        /// <summary>
        /// 获取或设置每更新多少字节的资源，重新生成一次版本资源列表。
        /// </summary>
        public int GenerateReadWriteVersionListLength
        {
            get { return resourceUpdater != null ? resourceUpdater.GenerateReadWriteVersionListLength : 0; }
            set
            {
                if (resourceUpdater == null)
                {
                    throw new GameFrameworkException("You can not use GenerateReadWriteVersionListLength at this time.");
                }

                resourceUpdater.GenerateReadWriteVersionListLength = value;
            }
        }

        /// <summary>
        /// 获取正在应用的资源包路径。
        /// </summary>
        public string ApplyingResourcePackPath
        {
            get { return resourceUpdater != null ? resourceUpdater.ApplyingResourcePackPath : null; }
        }

        /// <summary>
        /// 获取等待应用资源数量。
        /// </summary>
        public int ApplyWaitingCount
        {
            get { return resourceUpdater != null ? resourceUpdater.ApplyWaitingCount : 0; }
        }

        /// <summary>
        /// 获取或设置资源更新重试次数。
        /// </summary>
        public int UpdateRetryCount
        {
            get { return resourceUpdater != null ? resourceUpdater.UpdateRetryCount : 0; }
            set
            {
                if (resourceUpdater == null)
                {
                    throw new GameFrameworkException("You can not use UpdateRetryCount at this time.");
                }

                resourceUpdater.UpdateRetryCount = value;
            }
        }

        /// <summary>
        /// 获取正在更新的资源组。
        /// </summary>
        public IResourceGroup UpdatingResourceGroup
        {
            get { return resourceUpdater != null ? resourceUpdater.UpdatingResourceGroup : null; }
        }

        /// <summary>
        /// 获取等待更新资源数量。
        /// </summary>
        public int UpdateWaitingCount
        {
            get { return resourceUpdater != null ? resourceUpdater.UpdateWaitingCount : 0; }
        }

        /// <summary>
        /// 获取候选更新资源数量。
        /// </summary>
        public int UpdateCandidateCount
        {
            get { return resourceUpdater != null ? resourceUpdater.UpdateCandidateCount : 0; }
        }

        /// <summary>
        /// 获取正在更新资源数量。
        /// </summary>
        public int UpdatingCount
        {
            get { return resourceUpdater != null ? resourceUpdater.UpdatingCount : 0; }
        }

        /// <summary>
        /// 获取加载资源代理总数量。
        /// </summary>
        public int LoadTotalAgentCount
        {
            get { return resourceLoader.TotalAgentCount; }
        }

        /// <summary>
        /// 获取可用加载资源代理数量。
        /// </summary>
        public int LoadFreeAgentCount
        {
            get { return resourceLoader.FreeAgentCount; }
        }

        /// <summary>
        /// 获取工作中加载资源代理数量。
        /// </summary>
        public int LoadWorkingAgentCount
        {
            get { return resourceLoader.WorkingAgentCount; }
        }

        /// <summary>
        /// 获取等待加载资源任务数量。
        /// </summary>
        public int LoadWaitingTaskCount
        {
            get { return resourceLoader.WaitingTaskCount; }
        }

        /// <summary>
        /// 获取或设置资源对象池自动释放可释放对象的间隔秒数。
        /// </summary>
        public float AssetAutoReleaseInterval
        {
            get { return resourceLoader.AssetAutoReleaseInterval; }
            set { resourceLoader.AssetAutoReleaseInterval = value; }
        }

        /// <summary>
        /// 获取或设置资源对象池的容量。
        /// </summary>
        public int AssetCapacity
        {
            get { return resourceLoader.AssetCapacity; }
            set { resourceLoader.AssetCapacity = value; }
        }

        /// <summary>
        /// 获取或设置资源对象池对象过期秒数。
        /// </summary>
        public float AssetExpireTime
        {
            get { return resourceLoader.AssetExpireTime; }
            set { resourceLoader.AssetExpireTime = value; }
        }

        /// <summary>
        /// 获取或设置资源对象池的优先级。
        /// </summary>
        public int AssetPriority
        {
            get { return resourceLoader.AssetPriority; }
            set { resourceLoader.AssetPriority = value; }
        }

        /// <summary>
        /// 获取或设置资源对象池自动释放可释放对象的间隔秒数。
        /// </summary>
        public float ResourceAutoReleaseInterval
        {
            get { return resourceLoader.ResourceAutoReleaseInterval; }
            set { resourceLoader.ResourceAutoReleaseInterval = value; }
        }

        /// <summary>
        /// 获取或设置资源对象池的容量。
        /// </summary>
        public int ResourceCapacity
        {
            get { return resourceLoader.ResourceCapacity; }
            set { resourceLoader.ResourceCapacity = value; }
        }

        /// <summary>
        /// 获取或设置资源对象池对象过期秒数。
        /// </summary>
        public float ResourceExpireTime
        {
            get { return resourceLoader.ResourceExpireTime; }
            set { resourceLoader.ResourceExpireTime = value; }
        }

        /// <summary>
        /// 获取或设置资源对象池的优先级。
        /// </summary>
        public int ResourcePriority
        {
            get { return resourceLoader.ResourcePriority; }
            set { resourceLoader.ResourcePriority = value; }
        }


        /// <summary>
        /// 资源管理器轮询。
        /// </summary>
        /// <param name="elapseSeconds">逻辑流逝时间，以秒为单位。</param>
        /// <param name="realElapseSeconds">真实流逝时间，以秒为单位。</param>
        public override void Update(float elapseSeconds, float realElapseSeconds)
        {
            resourceUpdater.Update(elapseSeconds, realElapseSeconds);

            resourceLoader.Update(elapseSeconds, realElapseSeconds);
        }

        /// <summary>
        /// 关闭并清理资源管理器。
        /// </summary>
        public override void Shutdown()
        {
            if (versionListProcessor != null)
            {
                versionListProcessor.VersionListUpdateSuccess -= OnVersionListProcessorUpdateSuccess;
                versionListProcessor.VersionListUpdateFailure -= OnVersionListProcessorUpdateFailure;
                versionListProcessor.Shutdown();
                versionListProcessor = null;
            }

            if (resourceChecker != null)
            {
                resourceChecker.resourceNeedUpdate -= OnCheckerResourceNeedUpdate;
                resourceChecker.resourceCheckComplete -= OnCheckerResourceCheckComplete;
                resourceChecker.Shutdown();
                resourceChecker = null;
            }

            if (resourceUpdater != null)
            {
                resourceUpdater.ResourceApplySuccess -= OnResourceApplySuccess;
                resourceUpdater.ResourceApplyFailure -= OnResourceApplyFailure;
                resourceUpdater.ResourceApplyComplete -= OnResourceApplyComplete;
                resourceUpdater.ResourceUpdateStart -= OnUpdaterResourceUpdateStart;
                resourceUpdater.ResourceUpdateChanged -= OnUpdaterResourceUpdateChanged;
                resourceUpdater.ResourceUpdateSuccess -= OnUpdaterResourceUpdateSuccess;
                resourceUpdater.ResourceUpdateFailure -= OnUpdaterResourceUpdateFailure;
                resourceUpdater.ResourceUpdateComplete -= OnUpdaterResourceUpdateComplete;
                resourceUpdater.Shutdown();

                if (readWriteResourceInfos != null)
                {
                    readWriteResourceInfos.Clear();
                    readWriteResourceInfos = null;
                }

                if (decompressCachedStream != null)
                {
                    decompressCachedStream.Dispose();
                    decompressCachedStream = null;
                }
            }

            if (resourceLoader != null)
            {
                resourceLoader.Shutdown();
                resourceLoader = null;
            }

            if (assetInfos != null)
            {
                assetInfos.Clear();
                assetInfos = null;
            }

            if (resourceInfos != null)
            {
                resourceInfos.Clear();
                resourceInfos = null;
            }

            readOnlyFileSystems.Clear();
            readWriteFileSystems.Clear();
            resourceGroups.Clear();
        }

        /// <summary>
        /// 设置资源只读区路径。
        /// </summary>
        /// <param name="readOnlyPath">资源只读区路径。</param>
        public void SetReadOnlyPath(string readOnlyPath)
        {
            if (string.IsNullOrEmpty(readOnlyPath))
            {
                throw new GameFrameworkException("Readonly path is invalid.");
            }

            if (resourceLoader.TotalAgentCount > 0)
            {
                throw new GameFrameworkException("You must set readonly path before add load resource agent helper.");
            }

            this.readOnlyPath = readOnlyPath;
        }

        /// <summary>
        /// 设置资源读写区路径。
        /// </summary>
        /// <param name="readWritePath">资源读写区路径。</param>
        public void SetReadWritePath(string readWritePath)
        {
            if (string.IsNullOrEmpty(readWritePath))
            {
                throw new GameFrameworkException("Read-write path is invalid.");
            }

            if (resourceLoader.TotalAgentCount > 0)
            {
                throw new GameFrameworkException("You must set read-write path before add load resource agent helper.");
            }

            this.readWritePath = readWritePath;
        }

        /// <summary>
        /// 设置资源模式。
        /// </summary>
        /// <param name="resourceMode">资源模式。</param>
        public void SetResourceMode(ResourceMode resourceMode)
        {
            if (resourceMode == ResourceMode.Unspecified)
            {
                throw new GameFrameworkException("Resource mode is invalid.");
            }

            if (this.resourceMode == ResourceMode.Unspecified)
            {
                this.resourceMode = resourceMode;

                if (this.resourceMode == ResourceMode.Package)
                {
                }
                else if (this.resourceMode == ResourceMode.Updatable)
                {
                    versionListProcessor.VersionListUpdateSuccess += OnVersionListProcessorUpdateSuccess;
                    versionListProcessor.VersionListUpdateFailure += OnVersionListProcessorUpdateFailure;

                    resourceChecker.resourceNeedUpdate += OnCheckerResourceNeedUpdate;
                    resourceChecker.resourceCheckComplete += OnCheckerResourceCheckComplete;

                    resourceUpdater.ResourceApplySuccess += OnResourceApplySuccess;
                    resourceUpdater.ResourceApplyFailure += OnResourceApplyFailure;
                    resourceUpdater.ResourceApplyComplete += OnResourceApplyComplete;
                    resourceUpdater.ResourceUpdateStart += OnUpdaterResourceUpdateStart;
                    resourceUpdater.ResourceUpdateChanged += OnUpdaterResourceUpdateChanged;
                    resourceUpdater.ResourceUpdateSuccess += OnUpdaterResourceUpdateSuccess;
                    resourceUpdater.ResourceUpdateFailure += OnUpdaterResourceUpdateFailure;
                    resourceUpdater.ResourceUpdateComplete += OnUpdaterResourceUpdateComplete;
                }
            }
            else if (this.resourceMode != resourceMode)
            {
                throw new GameFrameworkException("You can not change resource mode at this time.");
            }
        }


        /// <summary>
        /// 使用可更新模式并检查版本资源列表。
        /// </summary>
        /// <param name="latestInternalResourceVersion">最新的内部资源版本号。</param>
        /// <returns>检查版本资源列表结果。</returns>
        public CheckVersionListResult CheckVersionList(int latestInternalResourceVersion)
        {
            if (resourceMode == ResourceMode.Unspecified)
            {
                throw new GameFrameworkException("You must set resource mode first.");
            }

            if (resourceMode != ResourceMode.Updatable)
            {
                throw new GameFrameworkException("You can not use CheckVersionList without updatable resource mode.");
            }

            if (versionListProcessor == null)
            {
                throw new GameFrameworkException("You can not use CheckVersionList at this time.");
            }

            return versionListProcessor.CheckVersionList(latestInternalResourceVersion);
        }

        /// <summary>
        /// 使用可更新模式并更新版本资源列表。
        /// </summary>
        /// <param name="versionListLength">版本资源列表大小。</param>
        /// <param name="versionListHashCode">版本资源列表哈希值。</param>
        /// <param name="versionListZipLength">版本资源列表压缩后大小。</param>
        /// <param name="versionListZipHashCode">版本资源列表压缩后哈希值。</param>
        /// <param name="updateVersionListCallbacks">版本资源列表更新回调函数集。</param>
        public void UpdateVersionList(int versionListLength, int versionListHashCode, int versionListZipLength, int versionListZipHashCode, UpdateVersionListCallbacks updateVersionListCallbacks)
        {
            if (updateVersionListCallbacks == null)
            {
                throw new GameFrameworkException("Update version list callbacks is invalid.");
            }

            if (resourceMode == ResourceMode.Unspecified)
            {
                throw new GameFrameworkException("You must set resource mode first.");
            }

            if (resourceMode != ResourceMode.Updatable)
            {
                throw new GameFrameworkException("You can not use UpdateVersionList without updatable resource mode.");
            }

            if (versionListProcessor == null)
            {
                throw new GameFrameworkException("You can not use UpdateVersionList at this time.");
            }

            this.updateVersionListCallbacks = updateVersionListCallbacks;
            versionListProcessor.UpdateVersionList(versionListLength, versionListHashCode, versionListZipLength, versionListZipHashCode);
        }

        /// <summary>
        /// 使用可更新模式并检查资源。
        /// </summary>
        /// <param name="ignoreOtherVariant">是否忽略处理其它变体的资源，若不忽略，将会移除其它变体的资源。</param>
        /// <param name="checkResourcesCompleteCallback">使用可更新模式并检查资源完成时的回调函数。</param>
        public void CheckResources()
        {
            if (resourceMode == ResourceMode.Unspecified)
            {
                throw new GameFrameworkException("You must set resource mode first.");
            }

            if (resourceMode != ResourceMode.Updatable)
            {
                throw new GameFrameworkException("You can not use CheckResources without updatable resource mode.");
            }

            if (resourceChecker == null)
            {
                throw new GameFrameworkException("You can not use CheckResources at this time.");
            }

            resourceChecker.CheckResources();
        }


        /// <summary>
        /// 使用可更新模式并更新全部资源。
        /// </summary>
        /// <param name="updateResourcesCompleteCallback">使用可更新模式并更新默认资源组完成时的回调函数。</param>
        public void UpdateResources(UpdateResourcesCompleteCallback updateResourcesCompleteCallback)
        {
            UpdateResources(string.Empty, updateResourcesCompleteCallback);
        }

        /// <summary>
        /// 使用可更新模式并更新指定资源组的资源。
        /// </summary>
        /// <param name="resourceGroupName">要更新的资源组名称。</param>
        /// <param name="updateResourcesCompleteCallback">使用可更新模式并更新指定资源组完成时的回调函数。</param>
        public void UpdateResources(string resourceGroupName, UpdateResourcesCompleteCallback updateResourcesCompleteCallback)
        {
            if (updateResourcesCompleteCallback == null)
            {
                throw new GameFrameworkException("Update resources complete callback is invalid.");
            }

            if (resourceMode == ResourceMode.Unspecified)
            {
                throw new GameFrameworkException("You must set resource mode first.");
            }

            if (resourceMode != ResourceMode.Updatable)
            {
                throw new GameFrameworkException("You can not use UpdateResources without updatable resource mode.");
            }

            if (resourceUpdater == null)
            {
                throw new GameFrameworkException("You can not use UpdateResources at this time.");
            }

            ResourceGroup resourceGroup = (ResourceGroup) GetResourceGroup(resourceGroupName);
            if (resourceGroup == null)
            {
                throw new GameFrameworkException(StringUtils.Format("Can not find resource group '{}'.", resourceGroupName));
            }

            this.updateResourcesCompleteCallback = updateResourcesCompleteCallback;
            resourceUpdater.UpdateResources(resourceGroup);
        }


        /// <summary>
        /// 检查资源是否存在。
        /// </summary>
        /// <param name="assetName">要检查资源的名称。</param>
        /// <returns>检查资源是否存在的结果。</returns>
        public HasAssetResult HasAsset(string assetName)
        {
            if (string.IsNullOrEmpty(assetName))
            {
                throw new GameFrameworkException("Asset name is invalid.");
            }

            return resourceLoader.HasAsset(assetName);
        }

        /// <summary>
        /// 异步加载资源。
        /// </summary>
        /// <param name="assetName">要加载资源的名称。</param>
        /// <param name="loadAssetCallbacks">加载资源回调函数集。</param>
        public void LoadAsset(string assetName, LoadAssetCallbacks loadAssetCallbacks)
        {
            if (string.IsNullOrEmpty(assetName))
            {
                throw new GameFrameworkException("Asset name is invalid.");
            }

            if (loadAssetCallbacks == null)
            {
                throw new GameFrameworkException("Load asset callbacks is invalid.");
            }

            resourceLoader.LoadAsset(assetName, null, ResourceConstant.DefaultPriority, loadAssetCallbacks, null);
        }

        /// <summary>
        /// 异步加载资源。
        /// </summary>
        /// <param name="assetName">要加载资源的名称。</param>
        /// <param name="assetType">要加载资源的类型。</param>
        /// <param name="loadAssetCallbacks">加载资源回调函数集。</param>
        public void LoadAsset(string assetName, Type assetType, LoadAssetCallbacks loadAssetCallbacks)
        {
            if (string.IsNullOrEmpty(assetName))
            {
                throw new GameFrameworkException("Asset name is invalid.");
            }

            if (loadAssetCallbacks == null)
            {
                throw new GameFrameworkException("Load asset callbacks is invalid.");
            }

            resourceLoader.LoadAsset(assetName, assetType, ResourceConstant.DefaultPriority, loadAssetCallbacks, null);
        }

        /// <summary>
        /// 异步加载资源。
        /// </summary>
        /// <param name="assetName">要加载资源的名称。</param>
        /// <param name="priority">加载资源的优先级。</param>
        /// <param name="loadAssetCallbacks">加载资源回调函数集。</param>
        public void LoadAsset(string assetName, int priority, LoadAssetCallbacks loadAssetCallbacks)
        {
            if (string.IsNullOrEmpty(assetName))
            {
                throw new GameFrameworkException("Asset name is invalid.");
            }

            if (loadAssetCallbacks == null)
            {
                throw new GameFrameworkException("Load asset callbacks is invalid.");
            }

            resourceLoader.LoadAsset(assetName, null, priority, loadAssetCallbacks, null);
        }

        /// <summary>
        /// 异步加载资源。
        /// </summary>
        /// <param name="assetName">要加载资源的名称。</param>
        /// <param name="loadAssetCallbacks">加载资源回调函数集。</param>
        /// <param name="userData">用户自定义数据。</param>
        public void LoadAsset(string assetName, LoadAssetCallbacks loadAssetCallbacks, object userData)
        {
            if (string.IsNullOrEmpty(assetName))
            {
                throw new GameFrameworkException("Asset name is invalid.");
            }

            if (loadAssetCallbacks == null)
            {
                throw new GameFrameworkException("Load asset callbacks is invalid.");
            }

            resourceLoader.LoadAsset(assetName, null, ResourceConstant.DefaultPriority, loadAssetCallbacks, userData);
        }

        /// <summary>
        /// 异步加载资源。
        /// </summary>
        /// <param name="assetName">要加载资源的名称。</param>
        /// <param name="assetType">要加载资源的类型。</param>
        /// <param name="priority">加载资源的优先级。</param>
        /// <param name="loadAssetCallbacks">加载资源回调函数集。</param>
        public void LoadAsset(string assetName, Type assetType, int priority, LoadAssetCallbacks loadAssetCallbacks)
        {
            if (string.IsNullOrEmpty(assetName))
            {
                throw new GameFrameworkException("Asset name is invalid.");
            }

            if (loadAssetCallbacks == null)
            {
                throw new GameFrameworkException("Load asset callbacks is invalid.");
            }

            resourceLoader.LoadAsset(assetName, assetType, priority, loadAssetCallbacks, null);
        }

        /// <summary>
        /// 异步加载资源。
        /// </summary>
        /// <param name="assetName">要加载资源的名称。</param>
        /// <param name="assetType">要加载资源的类型。</param>
        /// <param name="loadAssetCallbacks">加载资源回调函数集。</param>
        /// <param name="userData">用户自定义数据。</param>
        public void LoadAsset(string assetName, Type assetType, LoadAssetCallbacks loadAssetCallbacks, object userData)
        {
            if (string.IsNullOrEmpty(assetName))
            {
                throw new GameFrameworkException("Asset name is invalid.");
            }

            if (loadAssetCallbacks == null)
            {
                throw new GameFrameworkException("Load asset callbacks is invalid.");
            }

            resourceLoader.LoadAsset(assetName, assetType, ResourceConstant.DefaultPriority, loadAssetCallbacks, userData);
        }

        /// <summary>
        /// 异步加载资源。
        /// </summary>
        /// <param name="assetName">要加载资源的名称。</param>
        /// <param name="priority">加载资源的优先级。</param>
        /// <param name="loadAssetCallbacks">加载资源回调函数集。</param>
        /// <param name="userData">用户自定义数据。</param>
        public void LoadAsset(string assetName, int priority, LoadAssetCallbacks loadAssetCallbacks, object userData)
        {
            if (string.IsNullOrEmpty(assetName))
            {
                throw new GameFrameworkException("Asset name is invalid.");
            }

            if (loadAssetCallbacks == null)
            {
                throw new GameFrameworkException("Load asset callbacks is invalid.");
            }

            resourceLoader.LoadAsset(assetName, null, priority, loadAssetCallbacks, userData);
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
            if (string.IsNullOrEmpty(assetName))
            {
                throw new GameFrameworkException("Asset name is invalid.");
            }

            if (loadAssetCallbacks == null)
            {
                throw new GameFrameworkException("Load asset callbacks is invalid.");
            }

            resourceLoader.LoadAsset(assetName, assetType, priority, loadAssetCallbacks, userData);
        }

        /// <summary>
        /// 卸载资源。
        /// </summary>
        /// <param name="asset">要卸载的资源。</param>
        public void UnloadAsset(object asset)
        {
            if (asset == null)
            {
                throw new GameFrameworkException("Asset is invalid.");
            }

            if (resourceLoader == null)
            {
                return;
            }

            resourceLoader.UnloadAsset(asset);
        }

        /// <summary>
        /// 异步加载场景。
        /// </summary>
        /// <param name="sceneAssetName">要加载场景资源的名称。</param>
        /// <param name="loadSceneCallbacks">加载场景回调函数集。</param>
        public void LoadScene(string sceneAssetName, LoadSceneCallbacks loadSceneCallbacks)
        {
            if (string.IsNullOrEmpty(sceneAssetName))
            {
                throw new GameFrameworkException("Scene asset name is invalid.");
            }

            if (loadSceneCallbacks == null)
            {
                throw new GameFrameworkException("Load scene callbacks is invalid.");
            }

            resourceLoader.LoadScene(sceneAssetName, ResourceConstant.DefaultPriority, loadSceneCallbacks, null);
        }

        /// <summary>
        /// 异步加载场景。
        /// </summary>
        /// <param name="sceneAssetName">要加载场景资源的名称。</param>
        /// <param name="priority">加载场景资源的优先级。</param>
        /// <param name="loadSceneCallbacks">加载场景回调函数集。</param>
        public void LoadScene(string sceneAssetName, int priority, LoadSceneCallbacks loadSceneCallbacks)
        {
            if (string.IsNullOrEmpty(sceneAssetName))
            {
                throw new GameFrameworkException("Scene asset name is invalid.");
            }

            if (loadSceneCallbacks == null)
            {
                throw new GameFrameworkException("Load scene callbacks is invalid.");
            }

            resourceLoader.LoadScene(sceneAssetName, priority, loadSceneCallbacks, null);
        }

        /// <summary>
        /// 异步加载场景。
        /// </summary>
        /// <param name="sceneAssetName">要加载场景资源的名称。</param>
        /// <param name="loadSceneCallbacks">加载场景回调函数集。</param>
        /// <param name="userData">用户自定义数据。</param>
        public void LoadScene(string sceneAssetName, LoadSceneCallbacks loadSceneCallbacks, object userData)
        {
            if (string.IsNullOrEmpty(sceneAssetName))
            {
                throw new GameFrameworkException("Scene asset name is invalid.");
            }

            if (loadSceneCallbacks == null)
            {
                throw new GameFrameworkException("Load scene callbacks is invalid.");
            }

            resourceLoader.LoadScene(sceneAssetName, ResourceConstant.DefaultPriority, loadSceneCallbacks, userData);
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
            if (string.IsNullOrEmpty(sceneAssetName))
            {
                throw new GameFrameworkException("Scene asset name is invalid.");
            }

            if (loadSceneCallbacks == null)
            {
                throw new GameFrameworkException("Load scene callbacks is invalid.");
            }

            resourceLoader.LoadScene(sceneAssetName, priority, loadSceneCallbacks, userData);
        }

        /// <summary>
        /// 异步卸载场景。
        /// </summary>
        /// <param name="sceneAssetName">要卸载场景资源的名称。</param>
        /// <param name="unloadSceneCallbacks">卸载场景回调函数集。</param>
        public void UnloadScene(string sceneAssetName, UnloadSceneCallbacks unloadSceneCallbacks)
        {
            if (string.IsNullOrEmpty(sceneAssetName))
            {
                throw new GameFrameworkException("Scene asset name is invalid.");
            }

            if (unloadSceneCallbacks == null)
            {
                throw new GameFrameworkException("Unload scene callbacks is invalid.");
            }

            resourceLoader.UnloadScene(sceneAssetName, unloadSceneCallbacks, null);
        }

        /// <summary>
        /// 异步卸载场景。
        /// </summary>
        /// <param name="sceneAssetName">要卸载场景资源的名称。</param>
        /// <param name="unloadSceneCallbacks">卸载场景回调函数集。</param>
        /// <param name="userData">用户自定义数据。</param>
        public void UnloadScene(string sceneAssetName, UnloadSceneCallbacks unloadSceneCallbacks, object userData)
        {
            if (string.IsNullOrEmpty(sceneAssetName))
            {
                throw new GameFrameworkException("Scene asset name is invalid.");
            }

            if (unloadSceneCallbacks == null)
            {
                throw new GameFrameworkException("Unload scene callbacks is invalid.");
            }

            resourceLoader.UnloadScene(sceneAssetName, unloadSceneCallbacks, userData);
        }

        /// <summary>
        /// 获取二进制资源的实际路径。
        /// </summary>
        /// <param name="binaryAssetName">要获取实际路径的二进制资源的名称。</param>
        /// <returns>二进制资源的实际路径。</returns>
        /// <remarks>此方法仅适用于二进制资源存储在磁盘（而非文件系统）中的情况。若二进制资源存储在文件系统中时，返回值将始终为空。</remarks>
        public string GetBinaryPath(string binaryAssetName)
        {
            if (string.IsNullOrEmpty(binaryAssetName))
            {
                throw new GameFrameworkException("Binary asset name is invalid.");
            }

            return resourceLoader.GetBinaryPath(binaryAssetName);
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
        public bool GetBinaryPath(string binaryAssetName, out bool storageInReadOnly, out bool storageInFileSystem, out string relativePath, out string fileName)
        {
            return resourceLoader.GetBinaryPath(binaryAssetName, out storageInReadOnly, out storageInFileSystem, out relativePath, out fileName);
        }

        /// <summary>
        /// 获取二进制资源的长度。
        /// </summary>
        /// <param name="binaryAssetName">要获取长度的二进制资源的名称。</param>
        /// <returns>二进制资源的长度。</returns>
        public int GetBinaryLength(string binaryAssetName)
        {
            return resourceLoader.GetBinaryLength(binaryAssetName);
        }

        /// <summary>
        /// 异步加载二进制资源。
        /// </summary>
        /// <param name="binaryAssetName">要加载二进制资源的名称。</param>
        /// <param name="loadBinaryCallbacks">加载二进制资源回调函数集。</param>
        public void LoadBinary(string binaryAssetName, LoadBinaryCallbacks loadBinaryCallbacks)
        {
            if (string.IsNullOrEmpty(binaryAssetName))
            {
                throw new GameFrameworkException("Binary asset name is invalid.");
            }

            if (loadBinaryCallbacks == null)
            {
                throw new GameFrameworkException("Load binary callbacks is invalid.");
            }

            resourceLoader.LoadBinary(binaryAssetName, loadBinaryCallbacks, null);
        }

        /// <summary>
        /// 异步加载二进制资源。
        /// </summary>
        /// <param name="binaryAssetName">要加载二进制资源的名称。</param>
        /// <param name="loadBinaryCallbacks">加载二进制资源回调函数集。</param>
        /// <param name="userData">用户自定义数据。</param>
        public void LoadBinary(string binaryAssetName, LoadBinaryCallbacks loadBinaryCallbacks, object userData)
        {
            if (string.IsNullOrEmpty(binaryAssetName))
            {
                throw new GameFrameworkException("Binary asset name is invalid.");
            }

            if (loadBinaryCallbacks == null)
            {
                throw new GameFrameworkException("Load binary callbacks is invalid.");
            }

            resourceLoader.LoadBinary(binaryAssetName, loadBinaryCallbacks, userData);
        }

        /// <summary>
        /// 从文件系统中加载二进制资源。
        /// </summary>
        /// <param name="binaryAssetName">要加载二进制资源的名称。</param>
        /// <returns>存储加载二进制资源的二进制流。</returns>
        public byte[] LoadBinaryFromFileSystem(string binaryAssetName)
        {
            if (string.IsNullOrEmpty(binaryAssetName))
            {
                throw new GameFrameworkException("Binary asset name is invalid.");
            }

            return resourceLoader.LoadBinaryFromFileSystem(binaryAssetName);
        }

        /// <summary>
        /// 从文件系统中加载二进制资源。
        /// </summary>
        /// <param name="binaryAssetName">要加载二进制资源的名称。</param>
        /// <param name="buffer">存储加载二进制资源的二进制流。</param>
        /// <returns>实际加载了多少字节。</returns>
        public int LoadBinaryFromFileSystem(string binaryAssetName, byte[] buffer)
        {
            if (string.IsNullOrEmpty(binaryAssetName))
            {
                throw new GameFrameworkException("Binary asset name is invalid.");
            }

            if (buffer == null)
            {
                throw new GameFrameworkException("Buffer is invalid.");
            }

            return resourceLoader.LoadBinaryFromFileSystem(binaryAssetName, buffer, 0, buffer.Length);
        }

        /// <summary>
        /// 从文件系统中加载二进制资源。
        /// </summary>
        /// <param name="binaryAssetName">要加载二进制资源的名称。</param>
        /// <param name="buffer">存储加载二进制资源的二进制流。</param>
        /// <param name="startIndex">存储加载二进制资源的二进制流的起始位置。</param>
        /// <returns>实际加载了多少字节。</returns>
        public int LoadBinaryFromFileSystem(string binaryAssetName, byte[] buffer, int startIndex)
        {
            if (string.IsNullOrEmpty(binaryAssetName))
            {
                throw new GameFrameworkException("Binary asset name is invalid.");
            }

            if (buffer == null)
            {
                throw new GameFrameworkException("Buffer is invalid.");
            }

            return resourceLoader.LoadBinaryFromFileSystem(binaryAssetName, buffer, startIndex, buffer.Length - startIndex);
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
            if (string.IsNullOrEmpty(binaryAssetName))
            {
                throw new GameFrameworkException("Binary asset name is invalid.");
            }

            if (buffer == null)
            {
                throw new GameFrameworkException("Buffer is invalid.");
            }

            return resourceLoader.LoadBinaryFromFileSystem(binaryAssetName, buffer, startIndex, length);
        }

        /// <summary>
        /// 从文件系统中加载二进制资源的片段。
        /// </summary>
        /// <param name="binaryAssetName">要加载片段的二进制资源的名称。</param>
        /// <param name="length">要加载片段的长度。</param>
        /// <returns>存储加载二进制资源片段内容的二进制流。</returns>
        public byte[] LoadBinarySegmentFromFileSystem(string binaryAssetName, int length)
        {
            if (string.IsNullOrEmpty(binaryAssetName))
            {
                throw new GameFrameworkException("Binary asset name is invalid.");
            }

            return resourceLoader.LoadBinarySegmentFromFileSystem(binaryAssetName, 0, length);
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
            if (string.IsNullOrEmpty(binaryAssetName))
            {
                throw new GameFrameworkException("Binary asset name is invalid.");
            }

            return resourceLoader.LoadBinarySegmentFromFileSystem(binaryAssetName, offset, length);
        }

        /// <summary>
        /// 从文件系统中加载二进制资源的片段。
        /// </summary>
        /// <param name="binaryAssetName">要加载片段的二进制资源的名称。</param>
        /// <param name="buffer">存储加载二进制资源片段内容的二进制流。</param>
        /// <returns>实际加载了多少字节。</returns>
        public int LoadBinarySegmentFromFileSystem(string binaryAssetName, byte[] buffer)
        {
            if (string.IsNullOrEmpty(binaryAssetName))
            {
                throw new GameFrameworkException("Binary asset name is invalid.");
            }

            if (buffer == null)
            {
                throw new GameFrameworkException("Buffer is invalid.");
            }

            return resourceLoader.LoadBinarySegmentFromFileSystem(binaryAssetName, 0, buffer, 0, buffer.Length);
        }

        /// <summary>
        /// 从文件系统中加载二进制资源的片段。
        /// </summary>
        /// <param name="binaryAssetName">要加载片段的二进制资源的名称。</param>
        /// <param name="buffer">存储加载二进制资源片段内容的二进制流。</param>
        /// <param name="length">要加载片段的长度。</param>
        /// <returns>实际加载了多少字节。</returns>
        public int LoadBinarySegmentFromFileSystem(string binaryAssetName, byte[] buffer, int length)
        {
            if (string.IsNullOrEmpty(binaryAssetName))
            {
                throw new GameFrameworkException("Binary asset name is invalid.");
            }

            if (buffer == null)
            {
                throw new GameFrameworkException("Buffer is invalid.");
            }

            return resourceLoader.LoadBinarySegmentFromFileSystem(binaryAssetName, 0, buffer, 0, length);
        }

        /// <summary>
        /// 从文件系统中加载二进制资源的片段。
        /// </summary>
        /// <param name="binaryAssetName">要加载片段的二进制资源的名称。</param>
        /// <param name="buffer">存储加载二进制资源片段内容的二进制流。</param>
        /// <param name="startIndex">存储加载二进制资源片段内容的二进制流的起始位置。</param>
        /// <param name="length">要加载片段的长度。</param>
        /// <returns>实际加载了多少字节。</returns>
        public int LoadBinarySegmentFromFileSystem(string binaryAssetName, byte[] buffer, int startIndex, int length)
        {
            if (string.IsNullOrEmpty(binaryAssetName))
            {
                throw new GameFrameworkException("Binary asset name is invalid.");
            }

            if (buffer == null)
            {
                throw new GameFrameworkException("Buffer is invalid.");
            }

            return resourceLoader.LoadBinarySegmentFromFileSystem(binaryAssetName, 0, buffer, startIndex, length);
        }

        /// <summary>
        /// 从文件系统中加载二进制资源的片段。
        /// </summary>
        /// <param name="binaryAssetName">要加载片段的二进制资源的名称。</param>
        /// <param name="offset">要加载片段的偏移。</param>
        /// <param name="buffer">存储加载二进制资源片段内容的二进制流。</param>
        /// <returns>实际加载了多少字节。</returns>
        public int LoadBinarySegmentFromFileSystem(string binaryAssetName, int offset, byte[] buffer)
        {
            if (string.IsNullOrEmpty(binaryAssetName))
            {
                throw new GameFrameworkException("Binary asset name is invalid.");
            }

            if (buffer == null)
            {
                throw new GameFrameworkException("Buffer is invalid.");
            }

            return resourceLoader.LoadBinarySegmentFromFileSystem(binaryAssetName, offset, buffer, 0, buffer.Length);
        }

        /// <summary>
        /// 从文件系统中加载二进制资源的片段。
        /// </summary>
        /// <param name="binaryAssetName">要加载片段的二进制资源的名称。</param>
        /// <param name="offset">要加载片段的偏移。</param>
        /// <param name="buffer">存储加载二进制资源片段内容的二进制流。</param>
        /// <param name="length">要加载片段的长度。</param>
        /// <returns>实际加载了多少字节。</returns>
        public int LoadBinarySegmentFromFileSystem(string binaryAssetName, int offset, byte[] buffer, int length)
        {
            if (string.IsNullOrEmpty(binaryAssetName))
            {
                throw new GameFrameworkException("Binary asset name is invalid.");
            }

            if (buffer == null)
            {
                throw new GameFrameworkException("Buffer is invalid.");
            }

            return resourceLoader.LoadBinarySegmentFromFileSystem(binaryAssetName, offset, buffer, 0, length);
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
        public int LoadBinarySegmentFromFileSystem(string binaryAssetName, int offset, byte[] buffer, int startIndex, int length)
        {
            if (string.IsNullOrEmpty(binaryAssetName))
            {
                throw new GameFrameworkException("Binary asset name is invalid.");
            }

            if (buffer == null)
            {
                throw new GameFrameworkException("Buffer is invalid.");
            }

            return resourceLoader.LoadBinarySegmentFromFileSystem(binaryAssetName, offset, buffer, startIndex, length);
        }

        /// <summary>
        /// 检查资源组是否存在。
        /// </summary>
        /// <param name="resourceGroupName">要检查资源组的名称。</param>
        /// <returns>资源组是否存在。</returns>
        public bool HasResourceGroup(string resourceGroupName)
        {
            return resourceGroups.ContainsKey(resourceGroupName ?? string.Empty);
        }

        /// <summary>
        /// 获取默认资源组。
        /// </summary>
        /// <returns>默认资源组。</returns>
        public IResourceGroup GetResourceGroup()
        {
            return GetResourceGroup(string.Empty);
        }

        /// <summary>
        /// 获取资源组。
        /// </summary>
        /// <param name="resourceGroupName">要获取的资源组名称。</param>
        /// <returns>要获取的资源组。</returns>
        public IResourceGroup GetResourceGroup(string resourceGroupName)
        {
            ResourceGroup resourceGroup = null;
            if (resourceGroups.TryGetValue(resourceGroupName ?? string.Empty, out resourceGroup))
            {
                return resourceGroup;
            }

            return null;
        }

        /// <summary>
        /// 获取所有加载资源任务的信息。
        /// </summary>
        /// <returns>所有加载资源任务的信息。</returns>
        public TaskInfo[] GetAllLoadAssetInfos()
        {
            return resourceLoader.GetAllLoadAssetInfos();
        }

        public void UpdateResource(ResourceName resourceName)
        {
            resourceUpdater.UpdateResource(resourceName);
        }

        public ResourceGroup GetOrAddResourceGroup(string resourceGroupName)
        {
            if (resourceGroupName == null)
            {
                resourceGroupName = string.Empty;
            }

            ResourceGroup resourceGroup = null;
            if (!resourceGroups.TryGetValue(resourceGroupName, out resourceGroup))
            {
                resourceGroup = new ResourceGroup(resourceGroupName, resourceInfos);
                resourceGroups.Add(resourceGroupName, resourceGroup);
            }

            return resourceGroup;
        }

        public AssetInfo GetAssetInfo(string assetName)
        {
            if (string.IsNullOrEmpty(assetName))
            {
                throw new GameFrameworkException("Asset name is invalid.");
            }

            if (assetInfos == null)
            {
                return null;
            }

            AssetInfo assetInfo = null;
            if (assetInfos.TryGetValue(assetName, out assetInfo))
            {
                return assetInfo;
            }

            return null;
        }

        public ResourceInfo GetResourceInfo(ResourceName resourceName)
        {
            if (resourceInfos == null)
            {
                return null;
            }

            ResourceInfo resourceInfo = null;
            if (resourceInfos.TryGetValue(resourceName, out resourceInfo))
            {
                return resourceInfo;
            }

            return null;
        }

        public IFileSystem GetFileSystem(string fileSystemName, bool storageInReadOnly)
        {
            if (string.IsNullOrEmpty(fileSystemName))
            {
                throw new GameFrameworkException("File system name is invalid.");
            }

            IFileSystem fileSystem = null;
            if (storageInReadOnly)
            {
                if (!readOnlyFileSystems.TryGetValue(fileSystemName, out fileSystem))
                {
                    string fullPath = PathUtils.GetRegularPath(Path.Combine(readOnlyPath, StringUtils.Format("{}.{}", fileSystemName, DefaultExtension)));
                    fileSystem = fileSystemManager.GetFileSystem(fullPath);
                    if (fileSystem == null)
                    {
                        fileSystem = fileSystemManager.LoadFileSystem(fullPath, FileSystemAccess.Read);
                        readOnlyFileSystems.Add(fileSystemName, fileSystem);
                    }
                }
            }
            else
            {
                if (!readWriteFileSystems.TryGetValue(fileSystemName, out fileSystem))
                {
                    string fullPath = PathUtils.GetRegularPath(Path.Combine(readWritePath, StringUtils.Format("{}.{}", fileSystemName, DefaultExtension)));
                    fileSystem = fileSystemManager.GetFileSystem(fullPath);
                    if (fileSystem == null)
                    {
                        if (File.Exists(fullPath))
                        {
                            fileSystem = fileSystemManager.LoadFileSystem(fullPath, FileSystemAccess.ReadWrite);
                        }
                        else
                        {
                            string directory = Path.GetDirectoryName(fullPath);
                            if (!Directory.Exists(directory))
                            {
                                Directory.CreateDirectory(directory);
                            }

                            fileSystem = fileSystemManager.CreateFileSystem(fullPath, FileSystemAccess.ReadWrite, FileSystemMaxFileCount, FileSystemMaxBlockCount);
                        }

                        readWriteFileSystems.Add(fileSystemName, fileSystem);
                    }
                }
            }

            return fileSystem;
        }


        private void OnVersionListProcessorUpdateSuccess(string downloadPath, string downloadUri)
        {
            updateVersionListCallbacks.UpdateVersionListSuccessCallback(downloadPath, downloadUri);
        }

        private void OnVersionListProcessorUpdateFailure(string downloadUri, string errorMessage)
        {
            if (updateVersionListCallbacks.UpdateVersionListFailureCallback != null)
            {
                updateVersionListCallbacks.UpdateVersionListFailureCallback(downloadUri, errorMessage);
            }
        }

        private void OnCheckerResourceNeedUpdate(ResourceName resourceName, string fileSystemName, LoadType loadType, int length, int hashCode, int zipLength, int zipHashCode)
        {
            resourceUpdater.AddResourceUpdate(resourceName, fileSystemName, loadType, length, hashCode, zipLength, zipHashCode, PathUtils.GetRegularPath(Path.Combine(readWritePath, resourceName.FullName)));
        }

        private void OnCheckerResourceCheckComplete(int movedCount, int removedCount, int updateCount, long updateTotalLength, long updateTotalZipLength)
        {
            versionListProcessor.VersionListUpdateSuccess -= OnVersionListProcessorUpdateSuccess;
            versionListProcessor.VersionListUpdateFailure -= OnVersionListProcessorUpdateFailure;
            versionListProcessor.Shutdown();
            versionListProcessor = null;
            updateVersionListCallbacks = null;

            resourceChecker.resourceNeedUpdate -= OnCheckerResourceNeedUpdate;
            resourceChecker.resourceCheckComplete -= OnCheckerResourceCheckComplete;
            resourceChecker.Shutdown();
            resourceChecker = null;

            resourceUpdater.CheckResourceComplete(movedCount > 0 || removedCount > 0);

            if (updateCount <= 0)
            {
                resourceUpdater.ResourceApplySuccess -= OnResourceApplySuccess;
                resourceUpdater.ResourceApplyFailure -= OnResourceApplyFailure;
                resourceUpdater.ResourceApplyComplete -= OnResourceApplyComplete;
                resourceUpdater.ResourceUpdateStart -= OnUpdaterResourceUpdateStart;
                resourceUpdater.ResourceUpdateChanged -= OnUpdaterResourceUpdateChanged;
                resourceUpdater.ResourceUpdateSuccess -= OnUpdaterResourceUpdateSuccess;
                resourceUpdater.ResourceUpdateFailure -= OnUpdaterResourceUpdateFailure;
                resourceUpdater.ResourceUpdateComplete -= OnUpdaterResourceUpdateComplete;
                resourceUpdater.Shutdown();

                readWriteResourceInfos.Clear();
                readWriteResourceInfos = null;

                if (decompressCachedStream != null)
                {
                    decompressCachedStream.Dispose();
                    decompressCachedStream = null;
                }
            }

            EventBus.SyncSubmit(ResourceCheckCompleteEvent.ValueOf(movedCount, removedCount, updateCount, updateTotalLength, updateTotalZipLength));
        }

        private void OnResourceApplySuccess(ResourceName resourceName, string applyPath, string resourcePackPath, int length, int zipLength)
        {
            EventBus.SyncSubmit(ResourceApplySuccessEvent.ValueOf(resourceName.FullName, applyPath, resourcePackPath, length, zipLength));
        }

        private void OnResourceApplyFailure(ResourceName resourceName, string resourcePackPath, string errorMessage)
        {
            EventBus.SyncSubmit(ResourceApplyFailureEvent.ValueOf(resourceName.FullName, resourcePackPath, errorMessage));
        }

        private void OnResourceApplyComplete(string resourcePackPath, bool result, bool isAllDone)
        {
            if (isAllDone)
            {
                resourceUpdater.ResourceApplySuccess -= OnResourceApplySuccess;
                resourceUpdater.ResourceApplyFailure -= OnResourceApplyFailure;
                resourceUpdater.ResourceApplyComplete -= OnResourceApplyComplete;
                resourceUpdater.ResourceUpdateStart -= OnUpdaterResourceUpdateStart;
                resourceUpdater.ResourceUpdateChanged -= OnUpdaterResourceUpdateChanged;
                resourceUpdater.ResourceUpdateSuccess -= OnUpdaterResourceUpdateSuccess;
                resourceUpdater.ResourceUpdateFailure -= OnUpdaterResourceUpdateFailure;
                resourceUpdater.ResourceUpdateComplete -= OnUpdaterResourceUpdateComplete;
                resourceUpdater.Shutdown();

                readWriteResourceInfos.Clear();
                readWriteResourceInfos = null;

                if (decompressCachedStream != null)
                {
                    decompressCachedStream.Dispose();
                    decompressCachedStream = null;
                }

                PathUtils.RemoveEmptyDirectory(readWritePath);
            }
        }

        private void OnUpdaterResourceUpdateStart(ResourceName resourceName, string downloadPath, string downloadUri, int currentLength, int zipLength, int retryCount)
        {
            EventBus.SyncSubmit(ResourceUpdateStartEvent.ValueOf(resourceName.FullName, downloadPath, downloadUri, currentLength, zipLength, retryCount));
        }

        private void OnUpdaterResourceUpdateChanged(ResourceName resourceName, string downloadPath, string downloadUri, int currentLength, int zipLength)
        {
            EventBus.SyncSubmit(ResourceUpdateChangedEvent.ValueOf(resourceName.FullName, downloadPath, downloadUri, currentLength, zipLength));
        }

        private void OnUpdaterResourceUpdateSuccess(ResourceName resourceName, string downloadPath, string downloadUri, int length, int zipLength)
        {
            EventBus.SyncSubmit(ResourceUpdateSuccessEvent.ValueOf(resourceName.FullName, downloadPath, downloadUri, length, zipLength));
        }

        private void OnUpdaterResourceUpdateFailure(ResourceName resourceName, string downloadUri, int retryCount, int totalRetryCount, string errorMessage)
        {
            EventBus.SyncSubmit(ResourceUpdateFailureEvent.ValueOf(resourceName.FullName, downloadUri, retryCount, totalRetryCount, errorMessage));
        }

        private void OnUpdaterResourceUpdateComplete(ResourceGroup resourceGroup, bool result, bool isAllDone)
        {
            if (isAllDone)
            {
                resourceUpdater.ResourceApplySuccess -= OnResourceApplySuccess;
                resourceUpdater.ResourceApplyFailure -= OnResourceApplyFailure;
                resourceUpdater.ResourceApplyComplete -= OnResourceApplyComplete;
                resourceUpdater.ResourceUpdateStart -= OnUpdaterResourceUpdateStart;
                resourceUpdater.ResourceUpdateChanged -= OnUpdaterResourceUpdateChanged;
                resourceUpdater.ResourceUpdateSuccess -= OnUpdaterResourceUpdateSuccess;
                resourceUpdater.ResourceUpdateFailure -= OnUpdaterResourceUpdateFailure;
                resourceUpdater.ResourceUpdateComplete -= OnUpdaterResourceUpdateComplete;
                resourceUpdater.Shutdown();

                readWriteResourceInfos.Clear();
                readWriteResourceInfos = null;

                if (decompressCachedStream != null)
                {
                    decompressCachedStream.Dispose();
                    decompressCachedStream = null;
                }

                PathUtils.RemoveEmptyDirectory(readWritePath);
            }

            UpdateResourcesCompleteCallback updateResourcesCompleteCallback = this.updateResourcesCompleteCallback;
            this.updateResourcesCompleteCallback = null;
            updateResourcesCompleteCallback(resourceGroup, result);
        }
    }
}