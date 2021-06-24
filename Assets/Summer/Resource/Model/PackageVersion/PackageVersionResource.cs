using System.Runtime.InteropServices;
using Spring.Util;

namespace Summer.Resource.Model.PackageVersion
{
    /// <summary>
    /// 资源，用于描述 Unity 中的一个 AssetBundle（一些 Asset 的集合），或者一个 Game Framework 定义下的二进制文件（一个 Asset 的二进制形式，能够用于脱离 Unity 直接加载）。
    /// </summary>
    [StructLayout(LayoutKind.Auto)]
    public struct PackageVersionResource
    {
        private readonly string name;
        private readonly string variant;
        private readonly string extension;
        private readonly byte loadType;
        private readonly int length;
        private readonly int hashCode;
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
        /// <param name="assetIndexes">资源包含的资源索引集合。</param>
        public PackageVersionResource(string name, string variant, string extension, byte loadType, int length, int hashCode, int[] assetIndexes)
        {
            AssertionUtils.NotNull(name);
            this.name = name;
            this.variant = variant;
            this.extension = extension;
            this.loadType = loadType;
            this.length = length;
            this.hashCode = hashCode;
            this.assetIndexes = assetIndexes ?? CollectionUtils.EMPTY_INT_ARRAY;
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
        /// 获取资源包含的资源索引集合。
        /// </summary>
        /// <returns>资源包含的资源索引集合。</returns>
        public int[] GetAssetIndexes()
        {
            return assetIndexes;
        }
    }
}