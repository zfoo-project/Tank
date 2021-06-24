namespace Summer.Resource.Model.Constant
{
    /// <summary>
    /// 资源模式。
    /// </summary>
    public enum ResourceMode : byte
    {
        /// <summary>
        /// 未指定。
        /// </summary>
        Unspecified = 0,

        /// <summary>
        /// 单机模式。
        /// </summary>
        Package,

        /// <summary>
        /// 预下载的可更新模式。
        /// </summary>
        Updatable,
    }
}
