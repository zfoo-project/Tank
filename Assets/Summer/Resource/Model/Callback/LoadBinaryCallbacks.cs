using System;
using Summer.Resource.Model.Constant;
using Spring.Util;

namespace Summer.Resource.Model.Callback
{
    public delegate void LoadBinarySuccessCallback(string binaryAssetName, byte[] binaryBytes, float duration, object userData);

    public delegate void LoadBinaryFailureCallback(string binaryAssetName, LoadResourceStatus status, string errorMessage, object userData);

    /// <summary>
    /// 加载二进制资源回调函数集。
    /// </summary>
    public sealed class LoadBinaryCallbacks
    {
        private readonly LoadBinarySuccessCallback loadBinarySuccessCallback;
        private readonly LoadBinaryFailureCallback loadBinaryFailureCallback;

        /// <summary>
        /// 初始化加载二进制资源回调函数集的新实例。
        /// </summary>
        /// <param name="loadBinarySuccessCallback">加载二进制资源成功回调函数。</param>
        public LoadBinaryCallbacks(LoadBinarySuccessCallback loadBinarySuccessCallback)
            : this(loadBinarySuccessCallback, null)
        {
        }

        /// <summary>
        /// 初始化加载二进制资源回调函数集的新实例。
        /// </summary>
        /// <param name="loadBinarySuccessCallback">加载二进制资源成功回调函数。</param>
        /// <param name="loadBinaryFailureCallback">加载二进制资源失败回调函数。</param>
        public LoadBinaryCallbacks(LoadBinarySuccessCallback loadBinarySuccessCallback, LoadBinaryFailureCallback loadBinaryFailureCallback)
        {
            AssertionUtils.NotNull(loadBinarySuccessCallback);
            this.loadBinarySuccessCallback = loadBinarySuccessCallback;
            this.loadBinaryFailureCallback = loadBinaryFailureCallback;
            if (this.loadBinaryFailureCallback == null)
            {
                this.loadBinaryFailureCallback = DefaultOnLoadBinaryFailureCallback;
            }
        }

        /// <summary>
        /// 获取加载二进制资源成功回调函数。
        /// </summary>
        public LoadBinarySuccessCallback LoadBinarySuccessCallback
        {
            get { return loadBinarySuccessCallback; }
        }

        /// <summary>
        /// 获取加载二进制资源失败回调函数。
        /// </summary>
        public LoadBinaryFailureCallback LoadBinaryFailureCallback
        {
            get { return loadBinaryFailureCallback; }
        }

        public void DefaultOnLoadBinaryFailureCallback(string binaryAssetName, LoadResourceStatus status, string errorMessage, object userData)
        {
            throw new Exception(StringUtils.Format("Asset [binaryAssetName:{}] is invalid, [status:{}] [errorMessage:{}] [userData:{}]"
                , binaryAssetName, status, errorMessage, userData));
        }
    }
}