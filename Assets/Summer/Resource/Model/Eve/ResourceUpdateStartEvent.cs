using Spring.Event;

namespace Summer.Resource.Model.Eve
{
    /// <summary>
    /// 资源更新开始事件。
    /// </summary>
    public sealed class ResourceUpdateStartEvent : IEvent
    {
        /// <summary>
        /// 获取资源名称。
        /// </summary>
        public string name;

        /// <summary>
        /// 获取资源下载后存放路径。
        /// </summary>
        public string downloadPath;

        /// <summary>
        /// 获取下载地址。
        /// </summary>
        public string downloadUri;

        /// <summary>
        /// 获取当前下载大小。
        /// </summary>
        public int currentLength;

        /// <summary>
        /// 获取压缩后大小。
        /// </summary>
        public int zipLength;

        /// <summary>
        /// 获取已重试下载次数。
        /// </summary>
        public int retryCount;

        public static ResourceUpdateStartEvent ValueOf(string name, string downloadPath, string downloadUri, int currentLength, int zipLength, int retryCount)
        {
            var eve = new ResourceUpdateStartEvent();
            eve.name = name;
            eve.downloadPath = downloadPath;
            eve.downloadUri = downloadUri;
            eve.currentLength = currentLength;
            eve.zipLength = zipLength;
            eve.retryCount = retryCount;
            return eve;
        }
    }
}