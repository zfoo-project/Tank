using System.Runtime.InteropServices;

namespace Summer.Resource.Model.ResourcePackVersion
{
    /// <summary>
    /// 资源包版本资源列表。
    /// </summary>
    [StructLayout(LayoutKind.Auto)]
    public struct ResourcePackVersionList
    {
        private static readonly ResourcePackVersionResource[] EmptyResourceArray = new ResourcePackVersionResource[] { };

        private readonly bool isValid;
        private readonly int offset;
        private readonly long length;
        private readonly int hashCode;
        private readonly ResourcePackVersionResource[] resources;

        /// <summary>
        /// 初始化资源包版本资源列表的新实例。
        /// </summary>
        /// <param name="offset">资源数据偏移。</param>
        /// <param name="length">资源数据长度。</param>
        /// <param name="hashCode">资源数据哈希值。</param>
        /// <param name="resources">包含的资源集合。</param>
        public ResourcePackVersionList(int offset, long length, int hashCode, ResourcePackVersionResource[] resources)
        {
            isValid = true;
            this.offset = offset;
            this.length = length;
            this.hashCode = hashCode;
            this.resources = resources ?? EmptyResourceArray;
        }

        /// <summary>
        /// 获取资源包版本资源列表是否有效。
        /// </summary>
        public bool IsValid
        {
            get
            {
                return isValid;
            }
        }

        /// <summary>
        /// 获取资源数据偏移。
        /// </summary>
        public int Offset
        {
            get
            {
                return offset;
            }
        }

        /// <summary>
        /// 获取资源数据长度。
        /// </summary>
        public long Length
        {
            get
            {
                return length;
            }
        }

        /// <summary>
        /// 获取资源数据哈希值。
        /// </summary>
        public int HashCode
        {
            get
            {
                return hashCode;
            }
        }

        /// <summary>
        /// 获取包含的资源集合。
        /// </summary>
        /// <returns>包含的资源集合。</returns>
        public ResourcePackVersionResource[] GetResources()
        {
            return resources;
        }
    }
}
