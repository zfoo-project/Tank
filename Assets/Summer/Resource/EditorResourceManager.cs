using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using Summer.Base.Model;
using Summer.Base.TaskPool;
using Summer.Resource.Model.Callback;
using Summer.Resource.Model.Constant;
using Summer.Resource.Model.Group;
using Summer.Resource.Model.UpdatableVersion;
using Spring.Collection;
using Spring.Logger;
using Spring.Util;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace Summer.Resource
{
    /// <summary>
    /// 编辑器资源组件。
    /// </summary>
    public sealed class EditorResourceManager : AbstractManager, IResourceManager
    {
        private const int DefaultPriority = 0;
        private static readonly int AssetsStringLength = "Assets".Length;

        private int loadAssetCountPerFrame = 100;

        private float minLoadAssetRandomDelaySeconds = 0f;

        private float maxLoadAssetRandomDelaySeconds = 0f;

        private string readOnlyPath;
        private string readWritePath;
        private Dictionary<string, Object> cachedAssets;
        private CachedLinkedList<LoadAssetInfo> loadAssetInfos;
        private CachedLinkedList<LoadSceneInfo> loadSceneInfos;
        private CachedLinkedList<UnloadSceneInfo> unloadSceneInfos;

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
            get { return ResourceMode.Unspecified; }
        }


        /// <summary>
        /// 获取当前资源适用的游戏版本号。
        /// </summary>
        public string ApplicableGameVersion
        {
            get { throw new NotSupportedException("ApplicableGameVersion"); }
        }

        /// <summary>
        /// 获取当前内部资源版本号。
        /// </summary>
        public int InternalResourceVersion
        {
            get { throw new NotSupportedException("InternalResourceVersion"); }
        }

        /// <summary>
        /// 获取已准备完毕资源数量。
        /// </summary>
        public int AssetCount
        {
            get { throw new NotSupportedException("AssetCount"); }
        }

        /// <summary>
        /// 获取已准备完毕资源数量。
        /// </summary>
        public int ResourceCount
        {
            get { throw new NotSupportedException("ResourceCount"); }
        }

        /// <summary>
        /// 获取资源组个数。
        /// </summary>
        public int ResourceGroupCount
        {
            get { throw new NotSupportedException("ResourceGroupCount"); }
        }

        /// <summary>
        /// 获取或设置资源更新下载地址。
        /// </summary>
        public string UpdatePrefixUri
        {
            get { throw new NotSupportedException("UpdatePrefixUri"); }
            set { throw new NotSupportedException("UpdatePrefixUri"); }
        }

        /// <summary>
        /// 获取或设置每更新多少字节的资源，重新生成一次版本资源列表。
        /// </summary>
        public int GenerateReadWriteVersionListLength
        {
            get { throw new NotSupportedException("GenerateReadWriteVersionListLength"); }
            set { throw new NotSupportedException("GenerateReadWriteVersionListLength"); }
        }

        /// <summary>
        /// 获取正在应用的资源包路径。
        /// </summary>
        public string ApplyingResourcePackPath
        {
            get { throw new NotSupportedException("ApplyingResourcePackPath"); }
        }

        /// <summary>
        /// 获取等待应用资源数量。
        /// </summary>
        public int ApplyWaitingCount
        {
            get { throw new NotSupportedException("ApplyWaitingCount"); }
        }

        /// <summary>
        /// 获取或设置资源更新重试次数。
        /// </summary>
        public int UpdateRetryCount
        {
            get { throw new NotSupportedException("UpdateRetryCount"); }
            set { throw new NotSupportedException("UpdateRetryCount"); }
        }

        /// <summary>
        /// 获取正在更新的资源组。
        /// </summary>
        public IResourceGroup UpdatingResourceGroup
        {
            get { throw new NotSupportedException("UpdatingResourceGroup"); }
        }

        /// <summary>
        /// 获取等待更新资源个数。
        /// </summary>
        public int UpdateWaitingCount
        {
            get { throw new NotSupportedException("UpdateWaitingCount"); }
        }

        /// <summary>
        /// 获取候选更新资源数量。
        /// </summary>
        public int UpdateCandidateCount
        {
            get { throw new NotSupportedException("UpdateCandidateCount"); }
        }

        /// <summary>
        /// 获取正在更新资源个数。
        /// </summary>
        public int UpdatingCount
        {
            get { throw new NotSupportedException("UpdatingCount"); }
        }

        /// <summary>
        /// 获取加载资源代理总个数。
        /// </summary>
        public int LoadTotalAgentCount
        {
            get { throw new NotSupportedException("LoadTotalAgentCount"); }
        }

        /// <summary>
        /// 获取可用加载资源代理个数。
        /// </summary>
        public int LoadFreeAgentCount
        {
            get { throw new NotSupportedException("LoadFreeAgentCount"); }
        }

        /// <summary>
        /// 获取工作中加载资源代理个数。
        /// </summary>
        public int LoadWorkingAgentCount
        {
            get { throw new NotSupportedException("LoadWorkingAgentCount"); }
        }

        /// <summary>
        /// 获取等待加载资源任务个数。
        /// </summary>
        public int LoadWaitingTaskCount
        {
            get { throw new NotSupportedException("LoadWaitingTaskCount"); }
        }

        /// <summary>
        /// 获取或设置资源对象池自动释放可释放对象的间隔秒数。
        /// </summary>
        public float AssetAutoReleaseInterval
        {
            get { throw new NotSupportedException("AssetAutoReleaseInterval"); }
            set { throw new NotSupportedException("AssetAutoReleaseInterval"); }
        }

        /// <summary>
        /// 获取或设置资源对象池的容量。
        /// </summary>
        public int AssetCapacity
        {
            get { throw new NotSupportedException("AssetCapacity"); }
            set { throw new NotSupportedException("AssetCapacity"); }
        }

        /// <summary>
        /// 获取或设置资源对象池对象过期秒数。
        /// </summary>
        public float AssetExpireTime
        {
            get { throw new NotSupportedException("AssetExpireTime"); }
            set { throw new NotSupportedException("AssetExpireTime"); }
        }

        /// <summary>
        /// 获取或设置资源对象池的优先级。
        /// </summary>
        public int AssetPriority
        {
            get { throw new NotSupportedException("AssetPriority"); }
            set { throw new NotSupportedException("AssetPriority"); }
        }

        /// <summary>
        /// 获取或设置资源对象池自动释放可释放对象的间隔秒数。
        /// </summary>
        public float ResourceAutoReleaseInterval
        {
            get { throw new NotSupportedException("ResourceAutoReleaseInterval"); }
            set { throw new NotSupportedException("ResourceAutoReleaseInterval"); }
        }

        /// <summary>
        /// 获取或设置资源对象池的容量。
        /// </summary>
        public int ResourceCapacity
        {
            get { throw new NotSupportedException("ResourceCapacity"); }
            set { throw new NotSupportedException("ResourceCapacity"); }
        }

        /// <summary>
        /// 获取或设置资源对象池对象过期秒数。
        /// </summary>
        public float ResourceExpireTime
        {
            get { throw new NotSupportedException("ResourceExpireTime"); }
            set { throw new NotSupportedException("ResourceExpireTime"); }
        }

        /// <summary>
        /// 获取或设置资源对象池的优先级。
        /// </summary>
        public int ResourcePriority
        {
            get { throw new NotSupportedException("ResourcePriority"); }
            set { throw new NotSupportedException("ResourcePriority"); }
        }

        /// <summary>
        /// 获取等待编辑器加载的资源数量。
        /// </summary>
        public int LoadWaitingAssetCount
        {
            get { return loadAssetInfos.Count; }
        }


        public EditorResourceManager()
        {
            readOnlyPath = null;
            readWritePath = null;
            cachedAssets = new Dictionary<string, Object>(StringComparer.Ordinal);
            loadAssetInfos = new CachedLinkedList<LoadAssetInfo>();
            loadSceneInfos = new CachedLinkedList<LoadSceneInfo>();
            unloadSceneInfos = new CachedLinkedList<UnloadSceneInfo>();
        }

        public override void Update(float elapseSeconds, float realElapseSeconds)
        {
            if (loadAssetInfos.Count > 0)
            {
                int count = 0;
                LinkedListNode<LoadAssetInfo> current = loadAssetInfos.First;
                while (current != null && count < loadAssetCountPerFrame)
                {
                    LoadAssetInfo loadAssetInfo = current.Value;
                    if (elapseSeconds >= loadAssetInfo.DelaySeconds)
                    {
                        Object asset = GetCachedAsset(loadAssetInfo.AssetName);
                        if (asset == null)
                        {
#if UNITY_EDITOR
                            if (loadAssetInfo.AssetType != null)
                            {
                                asset = UnityEditor.AssetDatabase.LoadAssetAtPath(loadAssetInfo.AssetName,
                                    loadAssetInfo.AssetType);
                            }
                            else
                            {
                                asset = UnityEditor.AssetDatabase.LoadMainAssetAtPath(loadAssetInfo.AssetName);
                            }

                            if (asset != null)
                            {
                                cachedAssets.Add(loadAssetInfo.AssetName, asset);
                            }
#endif
                        }

                        if (asset != null)
                        {
                            if (loadAssetInfo.LoadAssetCallbacks.LoadAssetSuccessCallback != null)
                            {
                                loadAssetInfo.LoadAssetCallbacks.LoadAssetSuccessCallback(loadAssetInfo.AssetName,
                                    asset, elapseSeconds, loadAssetInfo.UserData);
                            }
                        }
                        else
                        {
                            if (loadAssetInfo.LoadAssetCallbacks.LoadAssetFailureCallback != null)
                            {
                                loadAssetInfo.LoadAssetCallbacks.LoadAssetFailureCallback(loadAssetInfo.AssetName,
                                    LoadResourceStatus.AssetError, "Can not load this asset from asset database.",
                                    loadAssetInfo.UserData);
                            }
                        }

                        LinkedListNode<LoadAssetInfo> next = current.Next;
                        loadAssetInfos.Remove(loadAssetInfo);
                        current = next;
                        count++;
                    }
                    else
                    {
                        if (loadAssetInfo.LoadAssetCallbacks.LoadAssetUpdateCallback != null)
                        {
                            loadAssetInfo.LoadAssetCallbacks.LoadAssetUpdateCallback(loadAssetInfo.AssetName,
                                elapseSeconds / loadAssetInfo.DelaySeconds, loadAssetInfo.UserData);
                        }

                        current = current.Next;
                    }
                }
            }

            if (loadSceneInfos.Count > 0)
            {
                LinkedListNode<LoadSceneInfo> current = loadSceneInfos.First;
                while (current != null)
                {
                    LoadSceneInfo loadSceneInfo = current.Value;
                    if (loadSceneInfo.AsyncOperation.isDone)
                    {
                        if (loadSceneInfo.AsyncOperation.allowSceneActivation)
                        {
                            if (loadSceneInfo.LoadSceneCallbacks.LoadSceneSuccessCallback != null)
                            {
                                loadSceneInfo.LoadSceneCallbacks.LoadSceneSuccessCallback(loadSceneInfo.SceneAssetName,
                                    (float) (DateTime.UtcNow - loadSceneInfo.StartTime).TotalSeconds,
                                    loadSceneInfo.UserData);
                            }
                        }
                        else
                        {
                            if (loadSceneInfo.LoadSceneCallbacks.LoadSceneFailureCallback != null)
                            {
                                loadSceneInfo.LoadSceneCallbacks.LoadSceneFailureCallback(loadSceneInfo.SceneAssetName,
                                    LoadResourceStatus.AssetError, "Can not load this scene from asset database.",
                                    loadSceneInfo.UserData);
                            }
                        }

                        LinkedListNode<LoadSceneInfo> next = current.Next;
                        loadSceneInfos.Remove(loadSceneInfo);
                        current = next;
                    }
                    else
                    {
                        if (loadSceneInfo.LoadSceneCallbacks.LoadSceneUpdateCallback != null)
                        {
                            loadSceneInfo.LoadSceneCallbacks.LoadSceneUpdateCallback(loadSceneInfo.SceneAssetName,
                                loadSceneInfo.AsyncOperation.progress, loadSceneInfo.UserData);
                        }

                        current = current.Next;
                    }
                }
            }

            if (unloadSceneInfos.Count > 0)
            {
                LinkedListNode<UnloadSceneInfo> current = unloadSceneInfos.First;
                while (current != null)
                {
                    UnloadSceneInfo unloadSceneInfo = current.Value;
                    if (unloadSceneInfo.AsyncOperation.isDone)
                    {
                        if (unloadSceneInfo.AsyncOperation.allowSceneActivation)
                        {
                            if (unloadSceneInfo.UnloadSceneCallbacks.UnloadSceneSuccessCallback != null)
                            {
                                unloadSceneInfo.UnloadSceneCallbacks.UnloadSceneSuccessCallback(
                                    unloadSceneInfo.SceneAssetName, unloadSceneInfo.UserData);
                            }
                        }
                        else
                        {
                            if (unloadSceneInfo.UnloadSceneCallbacks.UnloadSceneFailureCallback != null)
                            {
                                unloadSceneInfo.UnloadSceneCallbacks.UnloadSceneFailureCallback(
                                    unloadSceneInfo.SceneAssetName, unloadSceneInfo.UserData);
                            }
                        }

                        LinkedListNode<UnloadSceneInfo> next = current.Next;
                        unloadSceneInfos.Remove(unloadSceneInfo);
                        current = next;
                    }
                    else
                    {
                        current = current.Next;
                    }
                }
            }
        }

        public override void Shutdown()
        {
            // do nothing
        }


        /// <summary>
        /// 设置资源只读区路径。
        /// </summary>
        /// <param name="readOnlyPath">资源只读区路径。</param>
        public void SetReadOnlyPath(string readOnlyPath)
        {
            if (string.IsNullOrEmpty(readOnlyPath))
            {
                Log.Error("Readonly path is invalid.");
                return;
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
                Log.Error("Read-write path is invalid.");
                return;
            }

            this.readWritePath = readWritePath;
        }

        /// <summary>
        /// 设置资源模式。
        /// </summary>
        /// <param name="resourceMode">资源模式。</param>
        public void SetResourceMode(ResourceMode resourceMode)
        {
            throw new NotSupportedException("SetResourceMode");
        }

        /// <summary>
        /// 检查版本资源列表。
        /// </summary>
        /// <param name="latestInternalResourceVersion">最新的内部资源版本号。</param>
        /// <returns>检查版本资源列表结果。</returns>
        public CheckVersionListResult CheckVersionList(int latestInternalResourceVersion)
        {
            throw new NotSupportedException("CheckVersionList");
        }

        /// <summary>
        /// 使用可更新模式并更新版本资源列表。
        /// </summary>
        /// <param name="versionListLength">版本资源列表大小。</param>
        /// <param name="versionListHashCode">版本资源列表哈希值。</param>
        /// <param name="versionListZipLength">版本资源列表压缩后大小。</param>
        /// <param name="versionListZipHashCode">版本资源列表压缩后哈希值。</param>
        /// <param name="updateVersionListCallbacks">版本资源列表更新回调函数集。</param>
        public void UpdateVersionList(int versionListLength, int versionListHashCode, int versionListZipLength,
            int versionListZipHashCode, UpdateVersionListCallbacks updateVersionListCallbacks)
        {
            throw new NotSupportedException("UpdateVersionList");
        }

        /// <summary>
        /// 使用可更新模式并检查资源。
        /// </summary>
        /// <param name="ignoreOtherVariant">是否忽略处理其它变体的资源，若不忽略，将会移除其它变体的资源。</param>
        /// <param name="checkResourcesCompleteCallback">使用可更新模式并检查资源完成时的回调函数。</param>
        public void CheckResources()
        {
            throw new NotSupportedException("CheckResources");
        }


        /// <summary>
        /// 使用可更新模式并更新全部资源。
        /// </summary>
        /// <param name="updateResourcesCompleteCallback">使用可更新模式并更新默认资源组完成时的回调函数。</param>
        public void UpdateResources(UpdateResourcesCompleteCallback updateResourcesCompleteCallback)
        {
            throw new NotSupportedException("UpdateResources");
        }

        /// <summary>
        /// 使用可更新模式并更新指定资源组的资源。
        /// </summary>
        /// <param name="resourceGroupName">要更新的资源组名称。</param>
        /// <param name="updateResourcesCompleteCallback">使用可更新模式并更新指定资源组完成时的回调函数。</param>
        public void UpdateResources(string resourceGroupName,
            UpdateResourcesCompleteCallback updateResourcesCompleteCallback)
        {
            throw new NotSupportedException("UpdateResources");
        }

        /// <summary>
        /// 检查资源是否存在。
        /// </summary>
        /// <param name="assetName">要检查资源的名称。</param>
        /// <returns>检查资源是否存在的结果。</returns>
        public HasAssetResult HasAsset(string assetName)
        {
#if UNITY_EDITOR
            Object obj = UnityEditor.AssetDatabase.LoadMainAssetAtPath(assetName);
            if (obj == null)
            {
                return HasAssetResult.NotExist;
            }

            return obj.GetType() == typeof(UnityEditor.DefaultAsset) ? HasAssetResult.BinaryOnDisk : HasAssetResult.AssetOnDisk;
#else
            return HasAssetResult.NotExist;
#endif
        }

        /// <summary>
        /// 异步加载资源。
        /// </summary>
        /// <param name="assetName">要加载资源的名称。</param>
        /// <param name="loadAssetCallbacks">加载资源回调函数集。</param>
        public void LoadAsset(string assetName, LoadAssetCallbacks loadAssetCallbacks)
        {
            LoadAsset(assetName, null, DefaultPriority, loadAssetCallbacks, null);
        }

        /// <summary>
        /// 异步加载资源。
        /// </summary>
        /// <param name="assetName">要加载资源的名称。</param>
        /// <param name="assetType">要加载资源的类型。</param>
        /// <param name="loadAssetCallbacks">加载资源回调函数集。</param>
        public void LoadAsset(string assetName, Type assetType, LoadAssetCallbacks loadAssetCallbacks)
        {
            LoadAsset(assetName, assetType, DefaultPriority, loadAssetCallbacks, null);
        }

        /// <summary>
        /// 异步加载资源。
        /// </summary>
        /// <param name="assetName">要加载资源的名称。</param>
        /// <param name="priority">加载资源的优先级。</param>
        /// <param name="loadAssetCallbacks">加载资源回调函数集。</param>
        public void LoadAsset(string assetName, int priority, LoadAssetCallbacks loadAssetCallbacks)
        {
            LoadAsset(assetName, null, priority, loadAssetCallbacks, null);
        }

        /// <summary>
        /// 异步加载资源。
        /// </summary>
        /// <param name="assetName">要加载资源的名称。</param>
        /// <param name="loadAssetCallbacks">加载资源回调函数集。</param>
        /// <param name="userData">用户自定义数据。</param>
        public void LoadAsset(string assetName, LoadAssetCallbacks loadAssetCallbacks, object userData)
        {
            LoadAsset(assetName, null, DefaultPriority, loadAssetCallbacks, userData);
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
            LoadAsset(assetName, assetType, priority, loadAssetCallbacks, null);
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
            LoadAsset(assetName, assetType, DefaultPriority, loadAssetCallbacks, userData);
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
            LoadAsset(assetName, null, priority, loadAssetCallbacks, userData);
        }

        /// <summary>
        /// 异步加载资源。
        /// </summary>
        /// <param name="assetName">要加载资源的名称。</param>
        /// <param name="assetType">要加载资源的类型。</param>
        /// <param name="priority">加载资源的优先级。</param>
        /// <param name="loadAssetCallbacks">加载资源回调函数集。</param>
        /// <param name="userData">用户自定义数据。</param>
        public void LoadAsset(string assetName, Type assetType, int priority, LoadAssetCallbacks loadAssetCallbacks,
            object userData)
        {
            if (loadAssetCallbacks == null)
            {
                Log.Error("Load asset callbacks is invalid.");
                return;
            }

            if (string.IsNullOrEmpty(assetName))
            {
                if (loadAssetCallbacks.LoadAssetFailureCallback != null)
                {
                    loadAssetCallbacks.LoadAssetFailureCallback(assetName, LoadResourceStatus.NotExist,
                        "Asset name is invalid.", userData);
                }

                return;
            }

            if (!assetName.StartsWith("Assets/", StringComparison.Ordinal))
            {
                if (loadAssetCallbacks.LoadAssetFailureCallback != null)
                {
                    loadAssetCallbacks.LoadAssetFailureCallback(assetName, LoadResourceStatus.NotExist,
                        StringUtils.Format("Asset name '{}' is invalid.", assetName), userData);
                }

                return;
            }

            if (!HasFile(assetName))
            {
                if (loadAssetCallbacks.LoadAssetFailureCallback != null)
                {
                    loadAssetCallbacks.LoadAssetFailureCallback(assetName, LoadResourceStatus.NotExist,
                        StringUtils.Format("Asset '{}' is not exist.", assetName), userData);
                }

                return;
            }

            loadAssetInfos.AddLast(new LoadAssetInfo(assetName, assetType, priority, DateTime.UtcNow,
                minLoadAssetRandomDelaySeconds + (float) RandomUtils.GetRandomDouble() *
                (maxLoadAssetRandomDelaySeconds - minLoadAssetRandomDelaySeconds), loadAssetCallbacks, userData));
        }

        /// <summary>
        /// 卸载资源。
        /// </summary>
        /// <param name="asset">要卸载的资源。</param>
        public void UnloadAsset(object asset)
        {
            // Do nothing in editor resource mode.
        }

        /// <summary>
        /// 异步加载场景。
        /// </summary>
        /// <param name="sceneAssetName">要加载场景资源的名称。</param>
        /// <param name="loadSceneCallbacks">加载场景回调函数集。</param>
        public void LoadScene(string sceneAssetName, LoadSceneCallbacks loadSceneCallbacks)
        {
            LoadScene(sceneAssetName, DefaultPriority, loadSceneCallbacks, null);
        }

        /// <summary>
        /// 异步加载场景。
        /// </summary>
        /// <param name="sceneAssetName">要加载场景资源的名称。</param>
        /// <param name="priority">加载场景资源的优先级。</param>
        /// <param name="loadSceneCallbacks">加载场景回调函数集。</param>
        public void LoadScene(string sceneAssetName, int priority, LoadSceneCallbacks loadSceneCallbacks)
        {
            LoadScene(sceneAssetName, priority, loadSceneCallbacks, null);
        }

        /// <summary>
        /// 异步加载场景。
        /// </summary>
        /// <param name="sceneAssetName">要加载场景资源的名称。</param>
        /// <param name="loadSceneCallbacks">加载场景回调函数集。</param>
        /// <param name="userData">用户自定义数据。</param>
        public void LoadScene(string sceneAssetName, LoadSceneCallbacks loadSceneCallbacks, object userData)
        {
            LoadScene(sceneAssetName, DefaultPriority, loadSceneCallbacks, userData);
        }

        /// <summary>
        /// 异步加载场景。
        /// </summary>
        /// <param name="sceneAssetName">要加载场景资源的名称。</param>
        /// <param name="priority">加载场景资源的优先级。</param>
        /// <param name="loadSceneCallbacks">加载场景回调函数集。</param>
        /// <param name="userData">用户自定义数据。</param>
        public void LoadScene(string sceneAssetName, int priority, LoadSceneCallbacks loadSceneCallbacks,
            object userData)
        {
            if (loadSceneCallbacks == null)
            {
                Log.Error("Load scene callbacks is invalid.");
                return;
            }

            if (string.IsNullOrEmpty(sceneAssetName))
            {
                if (loadSceneCallbacks.LoadSceneFailureCallback != null)
                {
                    loadSceneCallbacks.LoadSceneFailureCallback(sceneAssetName, LoadResourceStatus.NotExist,
                        "Scene asset name is invalid.", userData);
                }

                return;
            }

            if (!sceneAssetName.StartsWith("Assets/", StringComparison.Ordinal) ||
                !sceneAssetName.EndsWith(".unity", StringComparison.Ordinal))
            {
                if (loadSceneCallbacks.LoadSceneFailureCallback != null)
                {
                    loadSceneCallbacks.LoadSceneFailureCallback(sceneAssetName, LoadResourceStatus.NotExist,
                        StringUtils.Format("Scene asset name '{}' is invalid.", sceneAssetName), userData);
                }

                return;
            }

            if (!HasFile(sceneAssetName))
            {
                if (loadSceneCallbacks.LoadSceneFailureCallback != null)
                {
                    loadSceneCallbacks.LoadSceneFailureCallback(sceneAssetName, LoadResourceStatus.NotExist,
                        StringUtils.Format("Scene '{}' is not exist.", sceneAssetName), userData);
                }

                return;
            }

            AsyncOperation asyncOperation = SceneManager.LoadSceneAsync(sceneAssetName, LoadSceneMode.Additive);
            if (asyncOperation == null)
            {
                return;
            }

            loadSceneInfos.AddLast(new LoadSceneInfo(asyncOperation, sceneAssetName, priority, DateTime.UtcNow,
                loadSceneCallbacks, userData));
        }

        /// <summary>
        /// 异步卸载场景。
        /// </summary>
        /// <param name="sceneAssetName">要卸载场景资源的名称。</param>
        /// <param name="unloadSceneCallbacks">卸载场景回调函数集。</param>
        public void UnloadScene(string sceneAssetName, UnloadSceneCallbacks unloadSceneCallbacks)
        {
            UnloadScene(sceneAssetName, unloadSceneCallbacks, null);
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
                Log.Error("Scene asset name is invalid.");
                return;
            }

            if (!sceneAssetName.StartsWith("Assets/", StringComparison.Ordinal) ||
                !sceneAssetName.EndsWith(".unity", StringComparison.Ordinal))
            {
                Log.Error("Scene asset name '{}' is invalid.", sceneAssetName);
                return;
            }

            if (unloadSceneCallbacks == null)
            {
                Log.Error("Unload scene callbacks is invalid.");
                return;
            }

            if (!HasFile(sceneAssetName))
            {
                Log.Error("Scene '{}' is not exist.", sceneAssetName);
                return;
            }

            AsyncOperation asyncOperation = SceneManager.UnloadSceneAsync(sceneAssetName);
            if (asyncOperation == null)
            {
                return;
            }

            unloadSceneInfos.AddLast(new UnloadSceneInfo(asyncOperation, sceneAssetName, unloadSceneCallbacks,
                userData));
        }

        /// <summary>
        /// 获取二进制资源的实际路径。
        /// </summary>
        /// <param name="binaryAssetName">要获取实际路径的二进制资源的名称。</param>
        /// <returns>二进制资源的实际路径。</returns>
        /// <remarks>此方法仅适用于二进制资源存储在磁盘（而非文件系统）中的情况。若二进制资源存储在文件系统中时，返回值将始终为空。</remarks>
        public string GetBinaryPath(string binaryAssetName)
        {
            if (!HasFile(binaryAssetName))
            {
                return null;
            }

            return Application.dataPath.Substring(0, Application.dataPath.Length - AssetsStringLength) +
                   binaryAssetName;
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
            throw new NotSupportedException("GetBinaryPath");
        }

        /// <summary>
        /// 获取二进制资源的长度。
        /// </summary>
        /// <param name="binaryAssetName">要获取长度的二进制资源的名称。</param>
        /// <returns>二进制资源的长度。</returns>
        public int GetBinaryLength(string binaryAssetName)
        {
            string binaryPath = GetBinaryPath(binaryAssetName);
            if (string.IsNullOrEmpty(binaryPath))
            {
                return -1;
            }

            return (int) new FileInfo(binaryPath).Length;
        }

        /// <summary>
        /// 异步加载二进制资源。
        /// </summary>
        /// <param name="binaryAssetName">要加载二进制资源的名称。</param>
        /// <param name="loadBinaryCallbacks">加载二进制资源回调函数集。</param>
        public void LoadBinary(string binaryAssetName, LoadBinaryCallbacks loadBinaryCallbacks)
        {
            LoadBinary(binaryAssetName, loadBinaryCallbacks, null);
        }

        /// <summary>
        /// 异步加载二进制资源。
        /// </summary>
        /// <param name="binaryAssetName">要加载二进制资源的名称。</param>
        /// <param name="loadBinaryCallbacks">加载二进制资源回调函数集。</param>
        /// <param name="userData">用户自定义数据。</param>
        public void LoadBinary(string binaryAssetName, LoadBinaryCallbacks loadBinaryCallbacks, object userData)
        {
            if (loadBinaryCallbacks == null)
            {
                Log.Error("Load binary callbacks is invalid.");
                return;
            }

            if (string.IsNullOrEmpty(binaryAssetName))
            {
                if (loadBinaryCallbacks.LoadBinaryFailureCallback != null)
                {
                    loadBinaryCallbacks.LoadBinaryFailureCallback(binaryAssetName, LoadResourceStatus.NotExist,
                        "Binary asset name is invalid.", userData);
                }

                return;
            }

            if (!binaryAssetName.StartsWith("Assets/", StringComparison.Ordinal))
            {
                if (loadBinaryCallbacks.LoadBinaryFailureCallback != null)
                {
                    loadBinaryCallbacks.LoadBinaryFailureCallback(binaryAssetName, LoadResourceStatus.NotExist,
                        StringUtils.Format("Binary asset name '{}' is invalid.", binaryAssetName), userData);
                }

                return;
            }

            string binaryPath = GetBinaryPath(binaryAssetName);
            if (binaryPath == null)
            {
                if (loadBinaryCallbacks.LoadBinaryFailureCallback != null)
                {
                    loadBinaryCallbacks.LoadBinaryFailureCallback(binaryAssetName, LoadResourceStatus.NotExist,
                        StringUtils.Format("Binary asset '{}' is not exist.", binaryAssetName), userData);
                }

                return;
            }

            try
            {
                byte[] binaryBytes = File.ReadAllBytes(binaryPath);
                loadBinaryCallbacks.LoadBinarySuccessCallback(binaryAssetName, binaryBytes, 0f, userData);
            }
            catch (Exception exception)
            {
                if (loadBinaryCallbacks.LoadBinaryFailureCallback != null)
                {
                    loadBinaryCallbacks.LoadBinaryFailureCallback(binaryAssetName, LoadResourceStatus.AssetError,
                        exception.ToString(), userData);
                }
            }
        }

        /// <summary>
        /// 从文件系统中加载二进制资源。
        /// </summary>
        /// <param name="binaryAssetName">要加载二进制资源的名称。</param>
        /// <returns>存储加载二进制资源的二进制流。</returns>
        public byte[] LoadBinaryFromFileSystem(string binaryAssetName)
        {
            throw new NotSupportedException("LoadBinaryFromFileSystem");
        }

        /// <summary>
        /// 从文件系统中加载二进制资源。
        /// </summary>
        /// <param name="binaryAssetName">要加载二进制资源的名称。</param>
        /// <param name="buffer">存储加载二进制资源的二进制流。</param>
        /// <returns>实际加载了多少字节。</returns>
        public int LoadBinaryFromFileSystem(string binaryAssetName, byte[] buffer)
        {
            throw new NotSupportedException("LoadBinaryFromFileSystem");
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
            throw new NotSupportedException("LoadBinaryFromFileSystem");
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
            throw new NotSupportedException("LoadBinaryFromFileSystem");
        }

        /// <summary>
        /// 从文件系统中加载二进制资源的片段。
        /// </summary>
        /// <param name="binaryAssetName">要加载片段的二进制资源的名称。</param>
        /// <param name="length">要加载片段的长度。</param>
        /// <returns>存储加载二进制资源片段内容的二进制流。</returns>
        public byte[] LoadBinarySegmentFromFileSystem(string binaryAssetName, int length)
        {
            throw new NotSupportedException("LoadBinarySegmentFromFileSystem");
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
            throw new NotSupportedException("LoadBinarySegmentFromFileSystem");
        }

        /// <summary>
        /// 从文件系统中加载二进制资源的片段。
        /// </summary>
        /// <param name="binaryAssetName">要加载片段的二进制资源的名称。</param>
        /// <param name="buffer">存储加载二进制资源片段内容的二进制流。</param>
        /// <returns>实际加载了多少字节。</returns>
        public int LoadBinarySegmentFromFileSystem(string binaryAssetName, byte[] buffer)
        {
            throw new NotSupportedException("LoadBinarySegmentFromFileSystem");
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
            throw new NotSupportedException("LoadBinarySegmentFromFileSystem");
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
            throw new NotSupportedException("LoadBinarySegmentFromFileSystem");
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
            throw new NotSupportedException("LoadBinarySegmentFromFileSystem");
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
            throw new NotSupportedException("LoadBinarySegmentFromFileSystem");
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
            throw new NotSupportedException("LoadBinarySegmentFromFileSystem");
        }

        /// <summary>
        /// 检查资源组是否存在。
        /// </summary>
        /// <param name="resourceGroupName">要检查资源组的名称。</param>
        /// <returns>资源组是否存在。</returns>
        public bool HasResourceGroup(string resourceGroupName)
        {
            throw new NotSupportedException("HasResourceGroup");
        }

        /// <summary>
        /// 获取默认资源组。
        /// </summary>
        /// <returns>默认资源组。</returns>
        public IResourceGroup GetResourceGroup()
        {
            throw new NotSupportedException("GetResourceGroup");
        }

        /// <summary>
        /// 获取资源组。
        /// </summary>
        /// <param name="resourceGroupName">要获取的资源组名称。</param>
        /// <returns>要获取的资源组。</returns>
        public IResourceGroup GetResourceGroup(string resourceGroupName)
        {
            throw new NotSupportedException("GetResourceGroup");
        }

        /// <summary>
        /// 获取所有加载资源任务的信息。
        /// </summary>
        /// <returns>所有加载资源任务的信息。</returns>
        public TaskInfo[] GetAllLoadAssetInfos()
        {
            throw new NotSupportedException("GetAllLoadAssetInfos");
        }

        private bool HasFile(string assetName)
        {
            if (string.IsNullOrEmpty(assetName))
            {
                return false;
            }

            if (HasCachedAsset(assetName))
            {
                return true;
            }

            string assetFullName = Application.dataPath.Substring(0, Application.dataPath.Length - AssetsStringLength) +
                                   assetName;
            if (string.IsNullOrEmpty(assetFullName))
            {
                return false;
            }

            string[] splitedAssetFullName = assetFullName.Split('/');
            string currentPath = Path.GetPathRoot(assetFullName);
            for (int i = 1; i < splitedAssetFullName.Length - 1; i++)
            {
                string[] directoryNames = Directory.GetDirectories(currentPath, splitedAssetFullName[i]);
                if (directoryNames.Length != 1)
                {
                    return false;
                }

                currentPath = directoryNames[0];
            }

            string[] fileNames = Directory.GetFiles(currentPath, splitedAssetFullName[splitedAssetFullName.Length - 1]);
            if (fileNames.Length != 1)
            {
                return false;
            }

            string fileFullName = PathUtils.GetRegularPath(fileNames[0]);
            if (fileFullName == null)
            {
                return false;
            }

            if (assetFullName != fileFullName)
            {
                if (assetFullName.ToLower() == fileFullName.ToLower())
                {
                    Log.Warning(
                        "The real path of the specific asset '{}' is '{}'. Check the case of letters in the path.",
                        assetName, "Assets" + fileFullName.Substring(Application.dataPath.Length));
                }

                return false;
            }

            return true;
        }

        private bool HasCachedAsset(string assetName)
        {
            if (string.IsNullOrEmpty(assetName))
            {
                return false;
            }

            return cachedAssets.ContainsKey(assetName);
        }

        private Object GetCachedAsset(string assetName)
        {
            if (string.IsNullOrEmpty(assetName))
            {
                return null;
            }

            Object asset = null;
            if (cachedAssets.TryGetValue(assetName, out asset))
            {
                return asset;
            }

            return null;
        }

        [StructLayout(LayoutKind.Auto)]
        private struct LoadAssetInfo
        {
            private readonly string assetName;
            private readonly Type assetType;
            private readonly int priority;
            private readonly DateTime startTime;
            private readonly float delaySeconds;
            private readonly LoadAssetCallbacks loadAssetCallbacks;
            private readonly object userData;

            public LoadAssetInfo(string assetName, Type assetType, int priority, DateTime startTime, float delaySeconds,
                LoadAssetCallbacks loadAssetCallbacks, object userData)
            {
                this.assetName = assetName;
                this.assetType = assetType;
                this.priority = priority;
                this.startTime = startTime;
                this.delaySeconds = delaySeconds;
                this.loadAssetCallbacks = loadAssetCallbacks;
                this.userData = userData;
            }

            public string AssetName
            {
                get { return assetName; }
            }

            public Type AssetType
            {
                get { return assetType; }
            }

            public int Priority
            {
                get { return priority; }
            }

            public DateTime StartTime
            {
                get { return startTime; }
            }

            public float DelaySeconds
            {
                get { return delaySeconds; }
            }

            public LoadAssetCallbacks LoadAssetCallbacks
            {
                get { return loadAssetCallbacks; }
            }

            public object UserData
            {
                get { return userData; }
            }
        }

        [StructLayout(LayoutKind.Auto)]
        private struct LoadSceneInfo
        {
            private readonly AsyncOperation asyncOperation;
            private readonly string sceneAssetName;
            private readonly int priority;
            private readonly DateTime startTime;
            private readonly LoadSceneCallbacks loadSceneCallbacks;
            private readonly object userData;

            public LoadSceneInfo(AsyncOperation asyncOperation, string sceneAssetName, int priority, DateTime startTime,
                LoadSceneCallbacks loadSceneCallbacks, object userData)
            {
                this.asyncOperation = asyncOperation;
                this.sceneAssetName = sceneAssetName;
                this.priority = priority;
                this.startTime = startTime;
                this.loadSceneCallbacks = loadSceneCallbacks;
                this.userData = userData;
            }

            public AsyncOperation AsyncOperation
            {
                get { return asyncOperation; }
            }

            public string SceneAssetName
            {
                get { return sceneAssetName; }
            }

            public int Priority
            {
                get { return priority; }
            }

            public DateTime StartTime
            {
                get { return startTime; }
            }

            public LoadSceneCallbacks LoadSceneCallbacks
            {
                get { return loadSceneCallbacks; }
            }

            public object UserData
            {
                get { return userData; }
            }
        }

        [StructLayout(LayoutKind.Auto)]
        private struct UnloadSceneInfo
        {
            private readonly AsyncOperation asyncOperation;
            private readonly string sceneAssetName;
            private readonly UnloadSceneCallbacks unloadSceneCallbacks;
            private readonly object userData;

            public UnloadSceneInfo(AsyncOperation asyncOperation, string sceneAssetName,
                UnloadSceneCallbacks unloadSceneCallbacks, object userData)
            {
                this.asyncOperation = asyncOperation;
                this.sceneAssetName = sceneAssetName;
                this.unloadSceneCallbacks = unloadSceneCallbacks;
                this.userData = userData;
            }

            public AsyncOperation AsyncOperation
            {
                get { return asyncOperation; }
            }

            public string SceneAssetName
            {
                get { return sceneAssetName; }
            }

            public UnloadSceneCallbacks UnloadSceneCallbacks
            {
                get { return unloadSceneCallbacks; }
            }

            public object UserData
            {
                get { return userData; }
            }
        }
    }
}