namespace Summer.Resource.Model.UpdatableVersion
{
    /// <summary>
    /// 可更新模式版本资源列表。
    /// </summary>
    public partial struct UpdatableVersionList
    {
        private static readonly UpdatableVersionAsset[] EmptyAssetArray = new UpdatableVersionAsset[] { };
        private static readonly UpdatableVersionResource[] EmptyResourceArray = new UpdatableVersionResource[] { };
        private static readonly UpdatableVersionFileSystem[] EmptyFileSystemArray = new UpdatableVersionFileSystem[] { };
        private static readonly UpdatableVersionResourceGroup[] EmptyResourceGroupArray = new UpdatableVersionResourceGroup[] { };

        private readonly bool m_IsValid;
        private readonly string m_ApplicableGameVersion;
        private readonly int m_InternalResourceVersion;
        private readonly UpdatableVersionAsset[] m_Assets;
        private readonly UpdatableVersionResource[] m_Resources;
        private readonly UpdatableVersionFileSystem[] m_FileSystems;
        private readonly UpdatableVersionResourceGroup[] m_ResourceGroups;

        /// <summary>
        /// 初始化可更新模式版本资源列表的新实例。
        /// </summary>
        /// <param name="applicableGameVersion">适配的游戏版本号。</param>
        /// <param name="internalResourceVersion">内部资源版本号。</param>
        /// <param name="assets">包含的资源集合。</param>
        /// <param name="resources">包含的资源集合。</param>
        /// <param name="fileSystems">包含的文件系统集合。</param>
        /// <param name="resourceGroups">包含的资源组集合。</param>
        public UpdatableVersionList(string applicableGameVersion, int internalResourceVersion, UpdatableVersionAsset[] assets, UpdatableVersionResource[] resources, UpdatableVersionFileSystem[] fileSystems, UpdatableVersionResourceGroup[] resourceGroups)
        {
            m_IsValid = true;
            m_ApplicableGameVersion = applicableGameVersion;
            m_InternalResourceVersion = internalResourceVersion;
            m_Assets = assets ?? EmptyAssetArray;
            m_Resources = resources ?? EmptyResourceArray;
            m_FileSystems = fileSystems ?? EmptyFileSystemArray;
            m_ResourceGroups = resourceGroups ?? EmptyResourceGroupArray;
        }

        /// <summary>
        /// 获取可更新模式版本资源列表是否有效。
        /// </summary>
        public bool IsValid
        {
            get
            {
                return m_IsValid;
            }
        }

        /// <summary>
        /// 获取适配的游戏版本号。
        /// </summary>
        public string ApplicableGameVersion
        {
            get
            {
                return m_ApplicableGameVersion;
            }
        }

        /// <summary>
        /// 获取内部资源版本号。
        /// </summary>
        public int InternalResourceVersion
        {
            get
            {
                return m_InternalResourceVersion;
            }
        }

        /// <summary>
        /// 获取包含的资源集合。
        /// </summary>
        /// <returns>包含的资源集合。</returns>
        public UpdatableVersionAsset[] GetAssets()
        {
            return m_Assets;
        }

        /// <summary>
        /// 获取包含的资源集合。
        /// </summary>
        /// <returns>包含的资源集合。</returns>
        public UpdatableVersionResource[] GetResources()
        {
            return m_Resources;
        }

        /// <summary>
        /// 获取包含的文件系统集合。
        /// </summary>
        /// <returns>包含的文件系统集合。</returns>
        public UpdatableVersionFileSystem[] GetFileSystems()
        {
            return m_FileSystems;
        }

        /// <summary>
        /// 获取包含的资源组集合。
        /// </summary>
        /// <returns>包含的资源组集合。</returns>
        public UpdatableVersionResourceGroup[] GetResourceGroups()
        {
            return m_ResourceGroups;
        }
    }
}
