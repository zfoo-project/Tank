namespace Summer.Resource.Model.Constant
{
    /// <summary>
    /// 资源检查状态。
    /// </summary>
    public enum CheckStatus : byte
    {
        /// <summary>
        /// 资源状态未知。
        /// </summary>
        Unknown = 0,

        /// <summary>
        /// 资源存在且已存放于只读区中。
        /// </summary>
        StorageInReadOnly,

        /// <summary>
        /// 资源存在且已存放于读写区中。
        /// </summary>
        StorageInReadWrite,

        /// <summary>
        /// 资源不适用于当前变体。
        /// </summary>
        Unavailable,

        /// <summary>
        /// 资源需要更新。
        /// </summary>
        Update,

        /// <summary>
        /// 资源已废弃。
        /// </summary>
        Disuse
    }
}