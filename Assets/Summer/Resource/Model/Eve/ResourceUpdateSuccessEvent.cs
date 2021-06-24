using Spring.Event;

namespace Summer.Resource.Model.Eve
{
    /// <summary>
    /// 资源更新成功事件。
    /// </summary>
    public sealed class ResourceUpdateSuccessEvent : IEvent
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
        /// 获取资源大小。
        /// </summary>
        public int length;

        /// <summary>
        /// 获取压缩后大小。
        /// </summary>
        public int zipLength;

        public static ResourceUpdateSuccessEvent ValueOf(string name, string downloadPath, string downloadUri, int length, int zipLength)
        {
            var eve = new ResourceUpdateSuccessEvent();
            eve.name = name;
            eve.downloadPath = downloadPath;
            eve.downloadUri = downloadUri;
            eve.length = length;
            eve.zipLength = zipLength;
            return eve;
        }
    }
}