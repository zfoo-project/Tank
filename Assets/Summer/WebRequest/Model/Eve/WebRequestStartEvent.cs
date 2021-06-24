using Spring.Collection.Reference;
using Spring.Event;

namespace Summer.WebRequest.Model.Eve
{
    /// <summary>
    /// Web 请求开始事件。
    /// </summary>
    public sealed class WebRequestStartEvent : IEvent, IReference
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
        /// 获取用户自定义数据。
        /// </summary>
        public object userData;

        public static WebRequestStartEvent ValueOf(int serialId, string webRequestUri, object userData)
        {
            var eve = ReferenceCache.Acquire<WebRequestStartEvent>();
            eve.serialId = serialId;
            eve.webRequestUri = webRequestUri;
            eve.userData = userData;
            return eve;
        }

        public void Clear()
        {
            serialId = 0;
            webRequestUri = null;
            userData = null;
        }
    }
}