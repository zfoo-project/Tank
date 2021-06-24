using Summer.Resource.Manager;
using Summer.Resource.Model.Constant;

namespace Summer.Resource.Model.Vo
{
    /// <summary>
    /// 应用信息。
    /// </summary>
    public sealed class ApplyInfo
    {
        private readonly ResourceName resourceName;
        private readonly string fileSystemName;
        private readonly LoadType loadType;
        private readonly long offset;
        private readonly int length;
        private readonly int hashCode;
        private readonly int zipLength;
        private readonly int zipHashCode;
        private readonly string resourcePath;

        /// <summary>
        /// 初始化应用信息的新实例。
        /// </summary>
        /// <param name="resourceName">资源名称。</param>
        /// <param name="fileSystemName">资源所在的文件系统名称。</param>
        /// <param name="loadType">资源加载方式。</param>
        /// <param name="offset">资源偏移。</param>
        /// <param name="length">资源大小。</param>
        /// <param name="hashCode">资源哈希值。</param>
        /// <param name="zipLength">压缩后大小。</param>
        /// <param name="zipHashCode">压缩后哈希值。</param>
        /// <param name="resourcePath">资源路径。</param>
        public ApplyInfo(ResourceName resourceName, string fileSystemName, LoadType loadType, long offset, int length,
            int hashCode, int zipLength, int zipHashCode, string resourcePath)
        {
            this.resourceName = resourceName;
            this.fileSystemName = fileSystemName;
            this.loadType = loadType;
            this.offset = offset;
            this.length = length;
            this.hashCode = hashCode;
            this.zipLength = zipLength;
            this.zipHashCode = zipHashCode;
            this.resourcePath = resourcePath;
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
        /// 获取资源所在的文件系统名称。
        /// </summary>
        public string FileSystemName
        {
            get { return fileSystemName; }
        }

        /// <summary>
        /// 获取资源加载方式。
        /// </summary>
        public LoadType LoadType
        {
            get { return loadType; }
        }

        /// <summary>
        /// 获取资源偏移。
        /// </summary>
        public long Offset
        {
            get { return offset; }
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
        public int HashCode { get; set; }

        /// <summary>
        /// 获取压缩后大小。
        /// </summary>
        public int ZipLength
        {
            get { return zipLength; }
        }

        /// <summary>
        /// 获取压缩后哈希值。
        /// </summary>
        public int ZipHashCode
        {
            get { return zipHashCode; }
        }

        /// <summary>
        /// 获取资源路径。
        /// </summary>
        public string ResourcePath
        {
            get { return resourcePath; }
        }
    }
}