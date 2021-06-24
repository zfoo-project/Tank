using Spring.Collection.Reference;
using Spring.Event;

namespace Summer.WebRequest.Model.Eve
{
    /// <summary>
    /// Web 请求失败事件。
    /// </summary>
    public sealed class WebRequestFailureEvent : IEvent, IReference
    {
        /// <summary>
        /// 获取 Web 请求任务的序列编号。
        /// </summary>
        public int serialId;

        /// <summary>
        /// 获取 Web 请求地址。
        /// </summary>
        public string webRequestUri;

        /// <summary>
        /// 获取错误信息。
        /// </summary>
        public string errorMessage;

        /// <summary>
        /// 获取用户自定义数据。
        /// </summary>
        public object userData;

        public static WebRequestFailureEvent ValueOf(int serialId, string webRequestUri, string errorMessage, object userData)
        {
            var eve = ReferenceCache.Acquire<WebRequestFailureEvent>();
            eve.serialId = serialId;
            eve.webRequestUri = webRequestUri;
            eve.errorMessage = errorMessage;
            eve.userData = userData;
            return eve;
        }

        /// <summary>
        /// 清理 Web 请求失败事件。
        /// </summary>
        public void Clear()
        {
            serialId = 0;
            webRequestUri = null;
            errorMessage = null;
            userData = null;
        }
    }
}