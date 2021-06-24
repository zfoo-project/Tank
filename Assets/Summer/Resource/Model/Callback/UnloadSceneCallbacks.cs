using System;
using Spring.Util;

namespace Summer.Resource.Model.Callback
{
    public delegate void UnloadSceneSuccessCallback(string sceneAssetName, object userData);

    public delegate void UnloadSceneFailureCallback(string sceneAssetName, object userData);

    /// <summary>
    /// 卸载场景回调函数集。
    /// </summary>
    public sealed class UnloadSceneCallbacks
    {
        private readonly UnloadSceneSuccessCallback unloadSceneSuccessCallback;
        private readonly UnloadSceneFailureCallback unloadSceneFailureCallback;

        /// <summary>
        /// 初始化卸载场景回调函数集的新实例。
        /// </summary>
        /// <param name="unloadSceneSuccessCallback">卸载场景成功回调函数。</param>
        public UnloadSceneCallbacks(UnloadSceneSuccessCallback unloadSceneSuccessCallback)
            : this(unloadSceneSuccessCallback, null)
        {
        }

        /// <summary>
        /// 初始化卸载场景回调函数集的新实例。
        /// </summary>
        /// <param name="unloadSceneSuccessCallback">卸载场景成功回调函数。</param>
        /// <param name="unloadSceneFailureCallback">卸载场景失败回调函数。</param>
        public UnloadSceneCallbacks(UnloadSceneSuccessCallback unloadSceneSuccessCallback, UnloadSceneFailureCallback unloadSceneFailureCallback)
        {
            AssertionUtils.NotNull(unloadSceneSuccessCallback);
            this.unloadSceneSuccessCallback = unloadSceneSuccessCallback;
            this.unloadSceneFailureCallback = unloadSceneFailureCallback;
        }

        /// <summary>
        /// 获取卸载场景成功回调函数。
        /// </summary>
        public UnloadSceneSuccessCallback UnloadSceneSuccessCallback
        {
            get { return unloadSceneSuccessCallback; }
        }

        /// <summary>
        /// 获取卸载场景失败回调函数。
        /// </summary>
        public UnloadSceneFailureCallback UnloadSceneFailureCallback
        {
            get { return unloadSceneFailureCallback; }
        }

        public void DefaultOnUnloadSceneFailureCallback(string sceneAssetName, object userData)
        {
            throw new Exception(StringUtils.Format("DefaultOnUnloadSceneFailureCallback unload scene [sceneAssetName:{}] error, [userData:{}]"
                , sceneAssetName, userData));
        }
    }
}