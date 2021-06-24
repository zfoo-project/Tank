using Spring.Event;

namespace Summer.Resource.Model.Eve
{
    /// <summary>
    /// 资源更新失败事件。
    /// </summary>
    public sealed class ResourceUpdateFailureEvent : IEvent
    {
        /// <summary>
        /// 获取资源名称。
        /// </summary>
        public string name;

        /// <summary>
        /// 获取下载地址。
        /// </summary>
        public string downloadUri;

        /// <summary>
        /// 获取已重试次数。
        /// </summary>
        public int retryCount;

        /// <summary>
        /// 获取设定的重试次数。
        /// </summary>
        public int totalRetryCount;

        /// <summary>
        /// 获取错误信息。
        /// </summary>
        public string errorMessage;

        public static ResourceUpdateFailureEvent ValueOf(string name, string downloadUri, int retryCount, int totalRetryCount, string errorMessage)
        {
            var eve = new ResourceUpdateFailureEvent();
            eve.name = name;
            eve.downloadUri = downloadUri;
            eve.retryCount = retryCount;
            eve.totalRetryCount = totalRetryCount;
            eve.errorMessage = errorMessage;
            return eve;
        }
    }
}