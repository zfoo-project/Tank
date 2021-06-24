using System.Runtime.InteropServices;
using Summer.Base;
using Summer.Base.Model;

namespace Summer.Resource.Model.UpdatableVersion
{
    /// <summary>
    /// 资源。
    /// </summary>
    [StructLayout(LayoutKind.Auto)]
    public struct UpdatableVersionAsset
    {
        private static readonly int[] EmptyIntArray = new int[] { };

        private readonly string name;
        private readonly int[] dependencyAssetIndexes;

        /// <summary>
        /// 初始化资源的新实例。
        /// </summary>
        /// <param name="name">资源名称。</param>
        /// <param name="dependencyAssetIndexes">资源包含的依赖资源索引集合。</param>
        public UpdatableVersionAsset(string name, int[] dependencyAssetIndexes)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new GameFrameworkException("Name is invalid.");
            }

            this.name = name;
            this.dependencyAssetIndexes = dependencyAssetIndexes ?? EmptyIntArray;
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