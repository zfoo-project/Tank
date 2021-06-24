using System.Runtime.InteropServices;
using Summer.Base;
using Summer.Base.Model;

namespace Summer.Resource.Model.UpdatableVersion
{
    /// <summary>
    /// 资源。
    /// </summary>
    [StructLayout(LayoutKind.Auto)]
    public struct UpdatableVersionResource
    {
        private static readonly int[] EmptyIntArray = new int[] { };

        private readonly string name;
        private readonly string variant;
        private readonly string extension;
        private readonly byte loadType;
        private readonly int length;
        private readonly int hashCode;
        private readonly int zipLength;
        private readonly int zipHashCode;
        private readonly int[] assetIndexes;

        /// <summary>
        /// 初始化资源的新实例。
        /// </summary>
        /// <param name="name">资源名称。</param>
        /// <param name="variant">资源变体名称。</param>
        /// <param name="extension">资源扩展名称。</param>
        /// <param name="loadType">资源加载方式。</param>
        /// <param name="length">资源长度。</param>
        /// <param name="hashCode">资源哈希值。</param>
        /// <param name="zipLength">资源压缩后长度。</param>
        /// <param name="zipHashCode">资源压缩后哈希值。</param>
        /// <param name="assetIndexes">资源包含的资源索引集合。</param>
        public UpdatableVersionResource(string name, string variant, string extension, byte loadType, int length, int hashCode, int zipLength, int zipHashCode, int[] assetIndexes)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new GameFrameworkException("Name is invalid.");
            }

            this.name = name;
            this.variant = variant;
            this.extension = extension;
            this.loadType = loadType;
            this.length = length;
            this.hashCode = hashCode;
            this.zipLength = zipLength;
            this.zipHashCode = zipHashCode;
            this.assetIndexes = assetIndexes ?? EmptyIntArray;
        }

        /// <summary>
        /// 获取资源名称。
        /// </summary>
        public string Name
        {
            get { return name; }
        }

        /// <summary>
        /// 获取资源变体名称。
        /// </summary>
        public string Variant
        {
            get { return variant; }
        }

        /// <summary>
        /// 获取资源扩展名称。
        /// </summary>
        public string Extension
        {
            get { return extension; }
        }

        /// <summary>
        /// 获取资源加载方式。
        /// </summary>
        public byte LoadType
        {
            get { return loadType; }
        }

        /// <summary>
        /// 获取资源长度。
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
        /// 获取资源压缩后长度。
        /// </summary>
        public int ZipLength
        {
            get { return zipLength; }
        }

        /// <summary>
        /// 获取资源压缩后哈希值。
        /// </summary>
        public int ZipHashCode
        {
            get { return zipHashCode; }
        }

        /// <summary>
        /// 获取资源包含的资源索引集合。
        /// </summary>
        /// <returns>资源包含的资源索引集合。</returns>
        public int[] GetAssetIndexes()
        {
            return assetIndexes;
        }
    }
}