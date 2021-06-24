using System.Runtime.InteropServices;
using Spring.Util;

namespace Summer.Resource.Model.PackageVersion
{
    /// <summary>
    /// 资源，用于描述 Unity 中的一个具体资产，如一个预制体、一个材质、一张图片等。
    /// </summary>
    [StructLayout(LayoutKind.Auto)]
    public struct PackageVersionAsset
    {
        private readonly string name;
        private readonly int[] dependencyAssetIndexes;

        /// <summary>
        /// 初始化资源的新实例。
        /// </summary>
        /// <param name="name">资源名称。</param>
        /// <param name="dependencyAssetIndexes">资源包含的依赖资源索引集合。</param>
        public PackageVersionAsset(string name, int[] dependencyAssetIndexes)
        {
            AssertionUtils.NotNull(name);
            this.name = name;
            this.dependencyAssetIndexes = dependencyAssetIndexes ?? CollectionUtils.EMPTY_INT_ARRAY;
        }

        /// <summary>
        /// 获取资源名称。
        /// </summary>
        public string Name
        {
            get { return name; }
        }

        /// <summary>
        /// 获取资源包含的依赖资源索引集合。
        /// </summary>
        /// <returns>资源包含的依赖资源索引集合。</returns>
        public int[] GetDependencyAssetIndexes()
        {
            return dependencyAssetIndexes;
        }
    }
}