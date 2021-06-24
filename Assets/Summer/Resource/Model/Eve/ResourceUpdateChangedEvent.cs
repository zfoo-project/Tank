using Spring.Event;

namespace Summer.Resource.Model.Eve
{
    /// <summary>
    /// 资源更新改变事件。
    /// </summary>
    public sealed class ResourceUpdateChangedEvent : IEvent
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

        public static ResourceUpdateChangedEvent ValueOf(string name, string downloadPath, string downloadUri, int currentLength, int zipLength)
        {
            var eve = new ResourceUpdateChangedEvent();
            eve.name = name;
            eve.downloadPath = downloadPath;
            eve.downloadUri = downloadUri;
            eve.currentLength = currentLength;
            eve.zipLength = zipLength;
            return eve;
        }
    }
}