namespace Summer.Resource.Model.Vo
{
    /// <summary>
    /// 资源信息。
    /// </summary>
    public sealed class AssetInfo
    {
        private readonly string assetName;
        private readonly ResourceName resourceName;
        private readonly string[] dependencyAssetNames;

        /// <summary>
        /// 初始化资源信息的新实例。
        /// </summary>
        /// <param name="assetName">资源名称。</param>
        /// <param name="resourceName">所在资源名称。</param>
        /// <param name="dependencyAssetNames">依赖资源名称。</param>
        public AssetInfo(string assetName, ResourceName resourceName, string[] dependencyAssetNames)
        {
            this.assetName = assetName;
            this.resourceName = resourceName;
            this.dependencyAssetNames = dependencyAssetNames;
        }

        /// <summary>
        /// 获取资源名称。
        /// </summary>
        public string AssetName
        {
            get { return assetName; }
        }

        /// <summary>
        /// 获取所在资源名称。
        /// </summary>
        public ResourceName ResourceName
        {
            get { return resourceName; }
        }

        /// <summary>
        /// 获取依赖资源名称。
        /// </summary>
        /// <returns>依赖资源名称。</returns>
        public string[] GetDependencyAssetNames()
        {
            return dependencyAssetNames;
        }
    }
}