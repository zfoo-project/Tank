using Spring.Collection.Reference;
using Spring.Event;

namespace Summer.WebRequest.Model.Eve
{
    /// <summary>
    /// Web 请求成功事件。
    /// </summary>
    public sealed class WebRequestSuccessEvent : IEvent, IReference
    {
        private byte[] webResponseBytes;


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

        public static WebRequestSuccessEvent ValueOf(int serialId, string webRequestUri, byte[] webResponseBytes, object userData)
        {
            var eve = ReferenceCache.Acquire<WebRequestSuccessEvent>();
            eve.serialId = serialId;
            eve.webRequestUri = webRequestUri;
            eve.webResponseBytes = webResponseBytes;
            eve.userData = userData;
            return eve;
        }

        public void Clear()
        {
            serialId = 0;
            webRequestUri = null;
            webResponseBytes = null;
            userData = null;
        }

        public byte[] GetWebResponseBytes()
        {
            return webResponseBytes;
        }
    }
}