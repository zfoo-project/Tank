using System;
using Summer.Resource.Model.Constant;
using Spring.Logger;
using Spring.Util;

namespace Summer.Resource.Model.Callback
{
    public delegate void LoadSceneSuccessCallback(string sceneAssetName, float duration, object userData);

    public delegate void LoadSceneFailureCallback(string sceneAssetName, LoadResourceStatus status, string errorMessage, object userData);

    public delegate void LoadSceneUpdateCallback(string sceneAssetName, float progress, object userData);

    public delegate void LoadSceneDependencyAssetCallback(string sceneAssetName, string dependencyAssetName, int loadedCount, int totalCount, object userData);

    /// <summary>
    /// 加载场景回调函数集。
    /// </summary>
    public sealed class LoadSceneCallbacks
    {
        private readonly LoadSceneSuccessCallback loadSceneSuccessCallback;
        private readonly LoadSceneFailureCallback loadSceneFailureCallback;
        private readonly LoadSceneUpdateCallback loadSceneUpdateCallback;
        private readonly LoadSceneDependencyAssetCallback loadSceneDependencyAssetCallback;

        /// <summary>
        /// 初始化加载场景回调函数集的新实例。
        /// </summary>
        /// <param name="loadSceneSuccessCallback">加载场景成功回调函数。</param>
        public LoadSceneCallbacks(LoadSceneSuccessCallback loadSceneSuccessCallback)
            : this(loadSceneSuccessCallback, null, null, null)
        {
        }

        /// <summary>
        /// 初始化加载场景回调函数集的新实例。
        /// </summary>
        /// <param name="loadSceneSuccessCallback">加载场景成功回调函数。</param>
        /// <param name="loadSceneFailureCallback">加载场景失败回调函数。</param>
        public LoadSceneCallbacks(LoadSceneSuccessCallback loadSceneSuccessCallback, LoadSceneFailureCallback loadSceneFailureCallback)
            : this(loadSceneSuccessCallback, loadSceneFailureCallback, null, null)
        {
        }

        /// <summary>
        /// 初始化加载场景回调函数集的新实例。
        /// </summary>
        /// <param name="loadSceneSuccessCallback">加载场景成功回调函数。</param>
        /// <param name="loadSceneUpdateCallback">加载场景更新回调函数。</param>
        public LoadSceneCallbacks(LoadSceneSuccessCallback loadSceneSuccessCallback, LoadSceneUpdateCallback loadSceneUpdateCallback)
            : this(loadSceneSuccessCallback, null, loadSceneUpdateCallback, null)
        {
        }

        /// <summary>
        /// 初始化加载场景回调函数集的新实例。
        /// </summary>
        /// <param name="loadSceneSuccessCallback">加载场景成功回调函数。</param>
        /// <param name="loadSceneDependencyAssetCallback">加载场景时加载依赖资源回调函数。</param>
        public LoadSceneCallbacks(LoadSceneSuccessCallback loadSceneSuccessCallback, LoadSceneDependencyAssetCallback loadSceneDependencyAssetCallback)
            : this(loadSceneSuccessCallback, null, null, loadSceneDependencyAssetCallback)
        {
        }

        /// <summary>
        /// 初始化加载场景回调函数集的新实例。
        /// </summary>
        /// <param name="loadSceneSuccessCallback">加载场景成功回调函数。</param>
        /// <param name="loadSceneFailureCallback">加载场景失败回调函数。</param>
        /// <param name="loadSceneUpdateCallback">加载场景更新回调函数。</param>
        public LoadSceneCallbacks(LoadSceneSuccessCallback loadSceneSuccessCallback, LoadSceneFailureCallback loadSceneFailureCallback, LoadSceneUpdateCallback loadSceneUpdateCallback)
            : this(loadSceneSuccessCallback, loadSceneFailureCallback, loadSceneUpdateCallback, null)
        {
        }

        /// <summary>
        /// 初始化加载场景回调函数集的新实例。
        /// </summary>
        /// <param name="loadSceneSuccessCallback">加载场景成功回调函数。</param>
        /// <param name="loadSceneFailureCallback">加载场景失败回调函数。</param>
        /// <param name="loadSceneDependencyAssetCallback">加载场景时加载依赖资源回调函数。</param>
        public LoadSceneCallbacks(LoadSceneSuccessCallback loadSceneSuccessCallback, LoadSceneFailureCallback loadSceneFailureCallback, LoadSceneDependencyAssetCallback loadSceneDependencyAssetCallback)
            : this(loadSceneSuccessCallback, loadSceneFailureCallback, null, loadSceneDependencyAssetCallback)
        {
        }

        /// <summary>
        /// 初始化加载场景回调函数集的新实例。
        /// </summary>
        /// <param name="loadSceneSuccessCallback">加载场景成功回调函数。</param>
        /// <param name="loadSceneFailureCallback">加载场景失败回调函数。</param>
        /// <param name="loadSceneUpdateCallback">加载场景更新回调函数。</param>
        /// <param name="loadSceneDependencyAssetCallback">加载场景时加载依赖资源回调函数。</param>
        public LoadSceneCallbacks(LoadSceneSuccessCallback loadSceneSuccessCallback, LoadSceneFailureCallback loadSceneFailureCallback, LoadSceneUpdateCallback loadSceneUpdateCallback,
            LoadSceneDependencyAssetCallback loadSceneDependencyAssetCallback)
        {
            AssertionUtils.NotNull(loadSceneSuccessCallback);
            this.loadSceneSuccessCallback = loadSceneSuccessCallback;
            this.loadSceneFailureCallback = loadSceneFailureCallback;
            this.loadSceneUpdateCallback = loadSceneUpdateCallback;
            this.loadSceneDependencyAssetCallback = loadSceneDependencyAssetCallback;

            if (this.loadSceneUpdateCallback == null)
            {
                this.loadSceneUpdateCallback = DefaultOnLoadSceneUpdateCallback;
            }

            if (this.loadSceneFailureCallback == null)
            {
                this.loadSceneFailureCallback = DefaultOnLoadSceneFailureCallback;
            }

            if (this.loadSceneDependencyAssetCallback == null)
            {
                this.loadSceneDependencyAssetCallback = DefaultOnLoadSceneDependencyAssetCallback;
            }
        }

        /// <summary>
        /// 获取加载场景成功回调函数。
        /// </summary>
        public LoadSceneSuccessCallback LoadSceneSuccessCallback
        {
            get { return loadSceneSuccessCallback; }
        }

        /// <summary>
        /// 获取加载场景失败回调函数。
        /// </summary>
        public LoadSceneFailureCallback LoadSceneFailureCallback
        {
            get { return loadSceneFailureCallback; }
        }

        /// <summary>
        /// 获取加载场景更新回调函数。
        /// </summary>
        public LoadSceneUpdateCallback LoadSceneUpdateCallback
        {
            get { return loadSceneUpdateCallback; }
        }

        /// <summary>
        /// 获取加载场景时加载依赖资源回调函数。
        /// </summary>
        public LoadSceneDependencyAssetCallback LoadSceneDependencyAssetCallback
        {
            get { return loadSceneDependencyAssetCallback; }
        }

        public void DefaultOnLoadSceneFailureCallback(string sceneAssetName, LoadResourceStatus status, string errorMessage, object userData)
        {
            throw new Exception(StringUtils.Format("Asset [sceneAssetName:{}] is invalid, [status:{}] [errorMessage:{}] [userData:{}]"
                , sceneAssetName, status, errorMessage, userData));
        }

        public void DefaultOnLoadSceneUpdateCallback(string sceneAssetName, float progress, object userData)
        {
        }

        public void DefaultOnLoadSceneDependencyAssetCallback(string sceneAssetName, string dependencyAssetName, int loadedCount, int totalCount, object userData)
        {
            Log.Info("DefaultOnLoadSceneDependencyAssetCallback [sceneAssetName:{}], [dependencyAssetName:{}] [loadedCount:{}] [totalCount:{}] [userData:{}]",
                sceneAssetName, dependencyAssetName, loadedCount, totalCount, userData);
        }
    }
}