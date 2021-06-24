using System.Runtime.InteropServices;

namespace Summer.Resource.Model.PackageVersion
{
    /// <summary>
    /// 单机模式版本资源列表。
    /// </summary>
    [StructLayout(LayoutKind.Auto)]
    public struct PackageVersionList
    {
        private static readonly PackageVersionAsset[] EmptyAssetArray = new PackageVersionAsset[]{ };
        private static readonly PackageVersionResource[] EmptyResourceArray = new PackageVersionResource[] { };
        private static readonly PackageVersionFileSystem[] EmptyFileSystemArray = new PackageVersionFileSystem[] { };
        private static readonly PackageVersionResourceGroup[] EmptyResourceGroupArray = new PackageVersionResourceGroup[] { };

        private readonly bool isValid;
        private readonly string applicableGameVersion;
        private readonly int internalResourceVersion;
        private readonly PackageVersionAsset[] assets;
        private readonly PackageVersionResource[] resources;
        private readonly PackageVersionFileSystem[] fileSystems;
        private readonly PackageVersionResourceGroup[] resourceGroups;

        /// <summary>
        /// 初始化单机模式版本资源列表的新实例。
        /// </summary>
        /// <param name="applicableGameVersion">适配的游戏版本号。</param>
        /// <param name="internalResourceVersion">内部资源版本号。</param>
        /// <param name="assets">包含的资源集合。</param>
        /// <param name="resources">包含的资源集合。</param>
        /// <param name="fileSystems">包含的文件系统集合。</param>
        /// <param name="resourceGroups">包含的资源组集合。</param>
        public PackageVersionList(string applicableGameVersion, int internalResourceVersion, PackageVersionAsset[] assets, PackageVersionResource[] resources, PackageVersionFileSystem[] fileSystems,
            PackageVersionResourceGroup[] resourceGroups)
        {
            isValid = true;
            this.applicableGameVersion = applicableGameVersion;
            this.internalResourceVersion = internalResourceVersion;
            this.assets = assets ?? EmptyAssetArray;
            this.resources = resources ?? EmptyResourceArray;
            this.fileSystems = fileSystems ?? EmptyFileSystemArray;
            this.resourceGroups = resourceGroups ?? EmptyResourceGroupArray;
        }

        /// <summary>
        /// 获取单机模式版本资源列表是否有效。
        /// </summary>
        public bool IsValid
        {
            get { return isValid; }
        }

        /// <summary>
        /// 获取适配的游戏版本号。
        /// </summary>
        public string ApplicableGameVersion
        {
            get { return applicableGameVersion; }
        }

        /// <summary>
        /// 获取内部资源版本号。
        /// </summary>
        public int InternalResourceVersion
        {
            get { return internalResourceVersion; }
        }

        /// <summary>
        /// 获取包含的资源集合。
        /// </summary>
        /// <returns>包含的资源集合。</returns>
        public PackageVersionAsset[] GetAssets()
        {
            return assets;
        }

        /// <summary>
        /// 获取包含的资源集合。
        /// </summary>
        /// <returns>包含的资源集合。</returns>
        public PackageVersionResource[] GetResources()
        {
            return resources;
        }

        /// <summary>
        /// 获取包含的文件系统集合。
        /// </summary>
        /// <returns>包含的文件系统集合。</returns>
        public PackageVersionFileSystem[] GetFileSystems()
        {
            return fileSystems;
        }

        /// <summary>
        /// 获取包含的资源组集合。
        /// </summary>
        /// <returns>包含的资源组集合。</returns>
        public PackageVersionResourceGroup[] GetResourceGroups()
        {
            return resourceGroups;
        }
    }
}