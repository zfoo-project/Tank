using System;
using Spring.Util;

namespace Summer.Resource.Model.Callback
{
    public delegate void LoadBytesSuccessCallback(string fileUri, byte[] bytes, float duration, object userData);

    public delegate void LoadBytesFailureCallback(string fileUri, string errorMessage, object userData);

    /// <summary>
    /// 加载数据流回调函数集。
    /// </summary>
    public sealed class LoadBytesCallbacks
    {
        private readonly LoadBytesSuccessCallback loadBytesSuccessCallback;
        private readonly LoadBytesFailureCallback loadBytesFailureCallback;

        /// <summary>
        /// 初始化加载数据流回调函数集的新实例。
        /// </summary>
        /// <param name="loadBinarySuccessCallback">加载数据流成功回调函数。</param>
        public LoadBytesCallbacks(LoadBytesSuccessCallback loadBinarySuccessCallback)
            : this(loadBinarySuccessCallback, null)
        {
        }

        /// <summary>
        /// 初始化加载数据流回调函数集的新实例。
        /// </summary>
        /// <param name="loadBytesSuccessCallback">加载数据流成功回调函数。</param>
        /// <param name="loadBytesFailureCallback">加载数据流失败回调函数。</param>
        public LoadBytesCallbacks(LoadBytesSuccessCallback loadBytesSuccessCallback, LoadBytesFailureCallback loadBytesFailureCallback)
        {
            AssertionUtils.NotNull(loadBytesSuccessCallback);
            this.loadBytesSuccessCallback = loadBytesSuccessCallback;
            this.loadBytesFailureCallback = loadBytesFailureCallback;
            if (this.loadBytesFailureCallback == null)
            {
                this.loadBytesFailureCallback = DefaultOnLoadBytesFailureCallback;
            }
        }

        /// <summary>
        /// 获取加载数据流成功回调函数。
        /// </summary>
        public LoadBytesSuccessCallback LoadBytesSuccessCallback
        {
            get { return loadBytesSuccessCallback; }
        }

        /// <summary>
        /// 获取加载数据流失败回调函数。
        /// </summary>
        public LoadBytesFailureCallback LoadBytesFailureCallback
        {
            get { return loadBytesFailureCallback; }
        }

        public void DefaultOnLoadBytesFailureCallback(string fileUri, string errorMessage, object userData)
        {
            throw new Exception(StringUtils.Format("Asset [fileUri:{}] is invalid, [errorMessage:{}] [userData:{}]"
                , fileUri, errorMessage, userData));
        }
    }
}