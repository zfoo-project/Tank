using Spring.Event;

namespace Summer.Resource.Model.Eve
{
    /// <summary>
    /// 资源应用失败事件。
    /// </summary>
    public sealed class ResourceApplyFailureEvent : IEvent
    {
        /// <summary>
        /// 获取资源名称。
        /// </summary>
        public string name;

        /// <summary>
        /// 获取资源包路径。
        /// </summary>
        public string resourcePackPath;

        /// <summary>
        /// 获取错误信息。
        /// </summary>
        public string errorMessage;

        public static ResourceApplyFailureEvent ValueOf(string name, string resourcePackPath, string errorMessage)
        {
            var eve = new ResourceApplyFailureEvent();
            eve.name = name;
            eve.resourcePackPath = resourcePackPath;
            eve.errorMessage = errorMessage;
            return eve;
        }
    }
}