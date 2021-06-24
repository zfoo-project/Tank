using Spring.Event;

namespace Summer.Resource.Model.Eve
{
    /// <summary>
    /// 资源应用成功事件。
    /// </summary>
    public sealed class ResourceApplySuccessEvent : IEvent
    {
        /// <summary>
        /// 获取资源名称。
        /// </summary>
        public string name;

        /// <summary>
        /// 获取资源应用后存放路径。
        /// </summary>
        public string applyPath;

        /// <summary>
        /// 获取资源包路径。
        /// </summary>
        public string resourcePackPath;

        /// <summary>
        /// 获取资源大小。
        /// </summary>
        public int length;

        /// <summary>
        /// 获取压缩后大小。
        /// </summary>
        public int zipLength;

        public static ResourceApplySuccessEvent ValueOf(string name, string applyPath, string resourcePackPath, int length, int zipLength)
        {
            var eve = new ResourceApplySuccessEvent();
            eve.name = name;
            eve.applyPath = applyPath;
            eve.resourcePackPath = resourcePackPath;
            eve.length = length;
            eve.zipLength = zipLength;
            return eve;
        }
    }
}