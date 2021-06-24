using Summer.Base.Model;
using Summer.Resource.Manager;
using Summer.Resource.Model.Constant;
using Spring.Util;

namespace Summer.Resource.Model.Vo
{
    /// <summary>
    /// 资源检查信息。
    /// </summary>
    public sealed class CheckInfo
    {
        private readonly ResourceName resourceName;
        private CheckStatus status;
        private bool needRemove;
        private bool needMoveToDisk;
        private bool needMoveToFileSystem;
        private RemoteVersionInfo versionInfo;
        private LocalVersionInfo readOnlyInfo;
        private LocalVersionInfo readWriteInfo;
        private string cachedFileSystemName;

        /// <summary>
        /// 初始化资源检查信息的新实例。
        /// </summary>
        /// <param name="resourceName">资源名称。</param>
        public CheckInfo(ResourceName resourceName)
        {
            this.resourceName = resourceName;
            this.status = CheckStatus.Unknown;
            this.needRemove = false;
            this.needMoveToDisk = false;
            this.needMoveToFileSystem = false;
            this.versionInfo = default(RemoteVersionInfo);
            this.readOnlyInfo = default(LocalVersionInfo);
            this.readWriteInfo = default(LocalVersionInfo);
            this.cachedFileSystemName = null;
        }

        /// <summary>
        /// 获取资源名称。
        /// </summary>
        public ResourceName ResourceName
        {
            get { return resourceName; }
        }

        /// <summary>
        /// 获取资源检查状态。
        /// </summary>
        public CheckStatus Status
        {
            get { return status; }
        }

        /// <summary>
        /// 获取是否需要移除读写区的资源。
        /// </summary>
        public bool NeedRemove
        {
            get { return needRemove; }
        }

        /// <summary>
        /// 获取是否需要将读写区的资源移动到磁盘。
        /// </summary>
        public bool NeedMoveToDisk
        {
            get { return needMoveToDisk; }
        }

        /// <summary>
        /// 获取是否需要将读写区的资源移动到文件系统。
        /// </summary>
        public bool NeedMoveToFileSystem
        {
            get { return needMoveToFileSystem; }
        }

        /// <summary>
        /// 获取资源所在的文件系统名称。
        /// </summary>
        public string FileSystemName
        {
            get { return versionInfo.FileSystemName; }
        }

        /// <summary>
        /// 获取资源是否使用文件系统。
        /// </summary>
        public bool ReadWriteUseFileSystem
        {
            get { return readWriteInfo.UseFileSystem; }
        }

        /// <summary>
        /// 获取读写资源所在的文件系统名称。
        /// </summary>
        public string ReadWriteFileSystemName
        {
            get { return readWriteInfo.FileSystemName; }
        }

        /// <summary>
        /// 获取资源加载方式。
        /// </summary>
        public LoadType LoadType
        {
            get { return versionInfo.LoadType; }
        }

        /// <summary>
        /// 获取资源大小。
        /// </summary>
        public int Length
        {
            get { return versionInfo.Length; }
        }

        /// <summary>
        /// 获取资源哈希值。
        /// </summary>
        public int HashCode
        {
            get { return versionInfo.HashCode; }
        }

        /// <summary>
        /// 获取压缩后大小。
        /// </summary>
        public int ZipLength
        {
            get { return versionInfo.ZipLength; }
        }

        /// <summary>
        /// 获取压缩后哈希值。
        /// </summary>
        public int ZipHashCode
        {
            get { return versionInfo.ZipHashCode; }
        }

        /// <summary>
        /// 临时缓存资源所在的文件系统名称。
        /// </summary>
        /// <param name="fileSystemName">资源所在的文件系统名称。</param>
        public void SetCachedFileSystemName(string fileSystemName)
        {
            cachedFileSystemName = fileSystemName;
        }

        /// <summary>
        /// 设置资源在版本中的信息。
        /// </summary>
        /// <param name="loadType">资源加载方式。</param>
        /// <param name="length">资源大小。</param>
        /// <param name="hashCode">资源哈希值。</param>
        /// <param name="zipLength">压缩后大小。</param>
        /// <param name="zipHashCode">压缩后哈希值。</param>
        public void SetVersionInfo(LoadType loadType, int length, int hashCode, int zipLength, int zipHashCode)
        {
            if (versionInfo.Exist)
            {
                throw new GameFrameworkException(StringUtils.Format("You must set version info of '{}' only once.",
                    resourceName.FullName));
            }

            versionInfo = new RemoteVersionInfo(cachedFileSystemName, loadType, length, hashCode, zipLength,
                zipHashCode);
            cachedFileSystemName = null;
        }

        /// <summary>
        /// 设置资源在只读区中的信息。
        /// </summary>
        /// <param name="loadType">资源加载方式。</param>
        /// <param name="length">资源大小。</param>
        /// <param name="hashCode">资源哈希值。</param>
        public void SetReadOnlyInfo(LoadType loadType, int length, int hashCode)
        {
            if (readOnlyInfo.Exist)
            {
                throw new GameFrameworkException(StringUtils.Format("You must set readonly info of '{}' only once.",
                    resourceName.FullName));
            }

            readOnlyInfo = new LocalVersionInfo(cachedFileSystemName, loadType, length, hashCode);
            cachedFileSystemName = null;
        }

        /// <summary>
        /// 设置资源在读写区中的信息。
        /// </summary>
        /// <param name="loadType">资源加载方式。</param>
        /// <param name="length">资源大小。</param>
        /// <param name="hashCode">资源哈希值。</param>
        public void SetReadWriteInfo(LoadType loadType, int length, int hashCode)
        {
            if (readWriteInfo.Exist)
            {
                throw new GameFrameworkException(StringUtils.Format("You must set read-write info of '{}' only once.",
                    resourceName.FullName));
            }

            readWriteInfo = new LocalVersionInfo(cachedFileSystemName, loadType, length, hashCode);
            cachedFileSystemName = null;
        }

        /// <summary>
        /// 刷新资源信息状态。
        /// </summary>
        public void RefreshStatus()
        {
            if (!versionInfo.Exist)
            {
                status = CheckStatus.Disuse;
                needRemove = readWriteInfo.Exist;
                return;
            }

            if (resourceName.Variant == null)
            {
                if (readOnlyInfo.Exist && readOnlyInfo.FileSystemName == versionInfo.FileSystemName &&
                    readOnlyInfo.LoadType == versionInfo.LoadType &&
                    readOnlyInfo.Length == versionInfo.Length && readOnlyInfo.HashCode == versionInfo.HashCode)
                {
                    status = CheckStatus.StorageInReadOnly;
                    needRemove = readWriteInfo.Exist;
                }
                else if (readWriteInfo.Exist && readWriteInfo.LoadType == versionInfo.LoadType &&
                         readWriteInfo.Length == versionInfo.Length &&
                         readWriteInfo.HashCode == versionInfo.HashCode)
                {
                    bool differentFileSystem = readWriteInfo.FileSystemName != versionInfo.FileSystemName;
                    status = CheckStatus.StorageInReadWrite;
                    needMoveToDisk = readWriteInfo.UseFileSystem && differentFileSystem;
                    needMoveToFileSystem = versionInfo.UseFileSystem && differentFileSystem;
                }
                else
                {
                    status = CheckStatus.Update;
                    needRemove = readWriteInfo.Exist;
                }
            }
            else
            {
                status = CheckStatus.Unavailable;
                needRemove = readWriteInfo.Exist;
            }
        }
    }
}