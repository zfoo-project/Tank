using System;
using Summer.FileSystem.Model;
using Summer.Resource.Model.Constant;
using Spring.Util;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;

namespace Summer.Resource.Model.Loader
{
    /// <summary>
    /// 默认加载资源代理辅助器。
    /// </summary>
    public class LoadResourceAgentMono : MonoBehaviour, IDisposable
    {
        private string fileFullPath;
        private string fileName;
        private string bytesFullPath;
        private string assetName;
        private float lastProgress;
        private bool disposed;

        private UnityWebRequest unityWebRequest;

        private AssetBundleCreateRequest fileAssetBundleCreateRequest;
        private AssetBundleCreateRequest bytesAssetBundleCreateRequest;
        private AssetBundleRequest assetBundleRequest;
        private AsyncOperation asyncOperation;


        public LoadResourceTaskAgent loadResourceTaskAgent;


        /// <summary>
        /// 通过加载资源代理辅助器开始异步读取资源文件。
        /// </summary>
        /// <param name="fullPath">要加载资源的完整路径名。</param>
        public void ReadFile(string fullPath)
        {
            fileFullPath = fullPath;
            fileAssetBundleCreateRequest = AssetBundle.LoadFromFileAsync(fullPath);
        }

        /// <summary>
        /// 通过加载资源代理辅助器开始异步读取资源文件。
        /// </summary>
        /// <param name="fileSystem">要加载资源的文件系统。</param>
        /// <param name="name">要加载资源的名称。</param>
        public void ReadFile(IFileSystem fileSystem, string name)
        {
            FileInfo fileInfo = fileSystem.GetFileInfo(name);
            fileFullPath = fileSystem.FullPath;
            fileName = name;
            fileAssetBundleCreateRequest = AssetBundle.LoadFromFileAsync(fileSystem.FullPath, 0u, (ulong) fileInfo.Offset);
        }

        /// <summary>
        /// 通过加载资源代理辅助器开始异步读取资源二进制流。
        /// </summary>
        /// <param name="fullPath">要加载资源的完整路径名。</param>
        public void ReadBytes(string fullPath)
        {
            bytesFullPath = fullPath;
            unityWebRequest = UnityWebRequest.Get(PathUtils.GetRemotePath(fullPath));
            unityWebRequest.SendWebRequest();
        }

        /// <summary>
        /// 通过加载资源代理辅助器开始异步读取资源二进制流。
        /// </summary>
        /// <param name="fileSystem">要加载资源的文件系统。</param>
        /// <param name="name">要加载资源的名称。</param>
        public void ReadBytes(IFileSystem fileSystem, string name)
        {
            byte[] bytes = fileSystem.ReadFile(name);
            this.loadResourceTaskAgent.OnLoadResourceAgentHelperReadBytesComplete(bytes);
        }

        /// <summary>
        /// 通过加载资源代理辅助器开始异步将资源二进制流转换为加载对象。
        /// </summary>
        /// <param name="bytes">要加载资源的二进制流。</param>
        public void ParseBytes(byte[] bytes)
        {
            bytesAssetBundleCreateRequest = AssetBundle.LoadFromMemoryAsync(bytes);
        }

        /// <summary>
        /// 通过加载资源代理辅助器开始异步加载资源。
        /// </summary>
        /// <param name="resource">资源。</param>
        /// <param name="assetName">要加载的资源名称。</param>
        /// <param name="assetType">要加载资源的类型。</param>
        /// <param name="isScene">要加载的资源是否是场景。</param>
        public void LoadAsset(object resource, string assetName, Type assetType, bool isScene)
        {
            AssetBundle assetBundle = resource as AssetBundle;
            if (assetBundle == null)
            {
                this.loadResourceTaskAgent.OnError(LoadResourceStatus.TypeError, "Can not load asset bundle from loaded resource which is not an asset bundle.");
                return;
            }

            if (string.IsNullOrEmpty(assetName))
            {
                this.loadResourceTaskAgent.OnError(LoadResourceStatus.AssetError, "Can not load asset from asset bundle which child name is invalid.");
                return;
            }

            this.assetName = assetName;
            if (isScene)
            {
                int sceneNamePositionStart = assetName.LastIndexOf('/');
                int sceneNamePositionEnd = assetName.LastIndexOf('.');
                if (sceneNamePositionStart <= 0 || sceneNamePositionEnd <= 0 || sceneNamePositionStart > sceneNamePositionEnd)
                {
                    this.loadResourceTaskAgent.OnError(LoadResourceStatus.AssetError, StringUtils.Format("Scene name '{}' is invalid.", assetName));
                    return;
                }

                string sceneName = assetName.Substring(sceneNamePositionStart + 1, sceneNamePositionEnd - sceneNamePositionStart - 1);
                asyncOperation = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
            }
            else
            {
                if (assetType != null)
                {
                    assetBundleRequest = assetBundle.LoadAssetAsync(assetName, assetType);
                }
                else
                {
                    assetBundleRequest = assetBundle.LoadAssetAsync(assetName);
                }
            }
        }

        /// <summary>
        /// 重置加载资源代理辅助器。
        /// </summary>
        public void Reset()
        {
            fileFullPath = null;
            fileName = null;
            bytesFullPath = null;
            assetName = null;
            lastProgress = 0f;

            if (unityWebRequest != null)
            {
                unityWebRequest.Dispose();
                unityWebRequest = null;
            }

            fileAssetBundleCreateRequest = null;
            bytesAssetBundleCreateRequest = null;
            assetBundleRequest = null;
            asyncOperation = null;
        }

        /// <summary>
        /// 释放资源。
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// 释放资源。
        /// </summary>
        /// <param name="disposing">释放资源标记。</param>
        protected virtual void Dispose(bool disposing)
        {
            if (disposed)
            {
                return;
            }

            if (disposing)
            {
                if (unityWebRequest != null)
                {
                    unityWebRequest.Dispose();
                    unityWebRequest = null;
                }
            }

            disposed = true;
        }

        private void Update()
        {
            UpdateUnityWebRequest();
            UpdateFileAssetBundleCreateRequest();
            UpdateBytesAssetBundleCreateRequest();
            UpdateAssetBundleRequest();
            UpdateAsyncOperation();
        }

        private void UpdateUnityWebRequest()
        {
            if (unityWebRequest != null)
            {
                if (unityWebRequest.isDone)
                {
                    if (string.IsNullOrEmpty(unityWebRequest.error))
                    {
                        this.loadResourceTaskAgent.OnLoadResourceAgentHelperReadBytesComplete(unityWebRequest.downloadHandler.data);
                        unityWebRequest.Dispose();
                        unityWebRequest = null;
                        bytesFullPath = null;
                        lastProgress = 0f;
                    }
                    else
                    {
                        bool isError = false;
                        isError = unityWebRequest.isNetworkError || unityWebRequest.isHttpError;
                        this.loadResourceTaskAgent.OnError(LoadResourceStatus.NotExist, StringUtils.Format("Can not load asset bundle '{}' with error message '{}'.", bytesFullPath, isError ? unityWebRequest.error : null));
                    }
                }
                else if (unityWebRequest.downloadProgress != lastProgress)
                {
                    lastProgress = unityWebRequest.downloadProgress;
                    this.loadResourceTaskAgent.OnLoadResourceAgentHelperUpdate(LoadResourceProgress.ReadResource, unityWebRequest.downloadProgress);
                }
            }
        }

        private void UpdateFileAssetBundleCreateRequest()
        {
            if (fileAssetBundleCreateRequest != null)
            {
                if (fileAssetBundleCreateRequest.isDone)
                {
                    AssetBundle assetBundle = fileAssetBundleCreateRequest.assetBundle;
                    if (assetBundle != null)
                    {
                        AssetBundleCreateRequest oldFileAssetBundleCreateRequest = fileAssetBundleCreateRequest;
                        this.loadResourceTaskAgent.OnLoadResourceAgentHelperReadFileComplete(assetBundle);
                        if (fileAssetBundleCreateRequest == oldFileAssetBundleCreateRequest)
                        {
                            fileAssetBundleCreateRequest = null;
                            lastProgress = 0f;
                        }
                    }
                    else
                    {
                        this.loadResourceTaskAgent.OnError(LoadResourceStatus.NotExist,
                            StringUtils.Format("Can not load asset bundle from file '{}' which is not a valid asset bundle.", fileName == null ? fileFullPath : StringUtils.Format("{} | {}", fileFullPath, fileName)));
                    }
                }
                else if (fileAssetBundleCreateRequest.progress != lastProgress)
                {
                    lastProgress = fileAssetBundleCreateRequest.progress;
                    this.loadResourceTaskAgent.OnLoadResourceAgentHelperUpdate(LoadResourceProgress.LoadResource, fileAssetBundleCreateRequest.progress);
                }
            }
        }

        private void UpdateBytesAssetBundleCreateRequest()
        {
            if (bytesAssetBundleCreateRequest != null)
            {
                if (bytesAssetBundleCreateRequest.isDone)
                {
                    AssetBundle assetBundle = bytesAssetBundleCreateRequest.assetBundle;
                    if (assetBundle != null)
                    {
                        AssetBundleCreateRequest oldBytesAssetBundleCreateRequest = bytesAssetBundleCreateRequest;
                        this.loadResourceTaskAgent.OnLoadResourceAgentHelperParseBytesComplete(assetBundle);
                        if (bytesAssetBundleCreateRequest == oldBytesAssetBundleCreateRequest)
                        {
                            bytesAssetBundleCreateRequest = null;
                            lastProgress = 0f;
                        }
                    }
                    else
                    {
                        this.loadResourceTaskAgent.OnError(LoadResourceStatus.NotExist, "Can not load asset bundle from memory which is not a valid asset bundle.");
                    }
                }
                else if (bytesAssetBundleCreateRequest.progress != lastProgress)
                {
                    lastProgress = bytesAssetBundleCreateRequest.progress;
                    this.loadResourceTaskAgent.OnLoadResourceAgentHelperUpdate(LoadResourceProgress.LoadResource, bytesAssetBundleCreateRequest.progress);
                }
            }
        }

        private void UpdateAssetBundleRequest()
        {
            if (assetBundleRequest != null)
            {
                if (assetBundleRequest.isDone)
                {
                    if (assetBundleRequest.asset != null)
                    {
                        this.loadResourceTaskAgent.OnLoadResourceAgentHelperLoadComplete(assetBundleRequest.asset);
                        assetName = null;
                        lastProgress = 0f;
                        assetBundleRequest = null;
                    }
                    else
                    {
                        this.loadResourceTaskAgent.OnError(LoadResourceStatus.AssetError, StringUtils.Format("Can not load asset '{}' from asset bundle which is not exist.", assetName));
                    }
                }
                else if (assetBundleRequest.progress != lastProgress)
                {
                    lastProgress = assetBundleRequest.progress;
                    this.loadResourceTaskAgent.OnLoadResourceAgentHelperUpdate(LoadResourceProgress.LoadAsset, assetBundleRequest.progress);
                }
            }
        }

        private void UpdateAsyncOperation()
        {
            if (asyncOperation != null)
            {
                if (asyncOperation.isDone)
                {
                    if (asyncOperation.allowSceneActivation)
                    {
                        this.loadResourceTaskAgent.OnLoadResourceAgentHelperLoadComplete(new object());
                        assetName = null;
                        lastProgress = 0f;
                        asyncOperation = null;
                    }
                    else
                    {
                        this.loadResourceTaskAgent.OnError(LoadResourceStatus.AssetError, StringUtils.Format("Can not load scene asset '{}' from asset bundle.", assetName));
                    }
                }
                else if (asyncOperation.progress != lastProgress)
                {
                    this.loadResourceTaskAgent.OnLoadResourceAgentHelperUpdate(LoadResourceProgress.LoadScene, asyncOperation.progress);
                    lastProgress = asyncOperation.progress;
                }
            }
        }
    }
}