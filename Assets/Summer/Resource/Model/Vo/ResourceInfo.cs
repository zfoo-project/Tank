using Summer.Resource.Model.Constant;

namespace Summer.Resource.Model.Vo
{
    /// <summary>
    /// 资源信息。
    /// </summary>
    public sealed class ResourceInfo
    {
        private readonly ResourceName resourceName;
        private readonly string fileSystemName;
        private readonly LoadType loadType;
        private readonly int length;
        private readonly int hashCode;
        private readonly bool storageInReadOnly;
        private bool ready;

        /// <summary>
        /// 初始化资源信息的新实例。
        /// </summary>
        /// <param name="resourceName">资源名称。</param>
        /// <param name="fileSystemName">文件系统名称。</param>
        /// <param name="loadType">资源加载方式。</param>
        /// <param name="length">资源大小。</param>
        /// <param name="hashCode">资源哈希值。</param>
        /// <param name="storageInReadOnly">资源是否在只读区。</param>
        /// <param name="ready">资源是否准备完毕。</param>
        public ResourceInfo(ResourceName resourceName, string fileSystemName, LoadType loadType, int length,
            int hashCode, bool storageInReadOnly, bool ready)
        {
            this.resourceName = resourceName;
            this.fileSystemName = fileSystemName;
            this.loadType = loadType;
            this.length = length;
            this.hashCode = hashCode;
            this.storageInReadOnly = storageInReadOnly;
            this.ready = ready;
        }

        /// <summary>
        /// 获取资源名称。
        /// </summary>
        public ResourceName ResourceName
        {
            get { return resourceName; }
        }

        /// <summary>
        /// 获取资源是否使用文件系统。
        /// </summary>
        public bool UseFileSystem
        {
            get { return !string.IsNullOrEmpty(fileSystemName); }
        }

        /// <summary>
        /// 获取文件系统名称。
        /// </summary>
        public string FileSystemName
        {
            get { return fileSystemName; }
        }

        /// <summary>
        /// 获取资源是否通过二进制方式加载。
        /// </summary>
        public bool IsLoadFromBinary
        {
            get
            {
                return loadType == LoadType.LoadFromBinary || loadType == LoadType.LoadFromBinaryAndQuickDecrypt ||
                       loadType == LoadType.LoadFromBinaryAndDecrypt;
            }
        }

        /// <summary>
        /// 获取资源加载方式。
        /// </summary>
        public LoadType LoadType
        {
            get { return loadType; }
        }

        /// <summary>
        /// 获取资源大小。
        /// </summary>
        public int Length
        {
            get { return length; }
        }

        /// <summary>
        /// 获取资源哈希值。
        /// </summary>
        public int HashCode
        {
            get { return hashCode; }
        }

        /// <summary>
        /// 获取资源是否在只读区。
        /// </summary>
        public bool StorageInReadOnly
        {
            get { return storageInReadOnly; }
        }

        /// <summary>
        /// 获取资源是否准备完毕。
        /// </summary>
        public bool Ready
        {
            get { return ready; }
        }

        /// <summary>
        /// 标记资源准备完毕。
        /// </summary>
        public void MarkReady()
        {
            ready = true;
        }
    }
}