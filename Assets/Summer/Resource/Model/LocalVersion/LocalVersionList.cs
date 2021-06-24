using System.Runtime.InteropServices;
using Summer.Resource.Model.LocalVersion;

namespace Summer.Resource
{
    /// <summary>
    /// 本地版本资源列表。
    /// </summary>
    [StructLayout(LayoutKind.Auto)]
    public struct LocalVersionList
    {
        private static readonly LocalVersionResource[] EmptyResourceArray = new LocalVersionResource[] { };
        private static readonly LocalVersionFileSystem[] EmptyFileSystemArray = new LocalVersionFileSystem[] { };

        private readonly bool isValid;
        private readonly LocalVersionResource[] resources;
        private readonly LocalVersionFileSystem[] fileSystems;

        /// <summary>
        /// 初始化本地版本资源列表的新实例。
        /// </summary>
        /// <param name="resources">包含的资源集合。</param>
        /// <param name="fileSystems">包含的文件系统集合。</param>
        public LocalVersionList(LocalVersionResource[] resources, LocalVersionFileSystem[] fileSystems)
        {
            isValid = true;
            this.resources = resources ?? EmptyResourceArray;
            this.fileSystems = fileSystems ?? EmptyFileSystemArray;
        }

        /// <summary>
        /// 获取本地版本资源列表是否有效。
        /// </summary>
        public bool IsValid
        {
            get
            {
                return isValid;
            }
        }

        /// <summary>
        /// 获取包含的资源集合。
        /// </summary>
        /// <returns>包含的资源集合。</returns>
        public LocalVersionResource[] GetResources()
        {
            return resources;
        }

        /// <summary>
        /// 获取包含的文件系统集合。
        /// </summary>
        /// <returns>包含的文件系统集合。</returns>
        public LocalVersionFileSystem[] GetFileSystems()
        {
            return fileSystems;
        }
    }
}
