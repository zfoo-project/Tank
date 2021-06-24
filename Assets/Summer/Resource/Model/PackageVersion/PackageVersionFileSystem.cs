using System.Runtime.InteropServices;
using Spring.Util;

namespace Summer.Resource.Model.PackageVersion
{
    /// <summary>
    /// 文件系统。
    /// </summary>
    [StructLayout(LayoutKind.Auto)]
    public struct PackageVersionFileSystem
    {

        private readonly string name;
        private readonly int[] resourceIndexes;

        /// <summary>
        /// 初始化文件系统的新实例。
        /// </summary>
        /// <param name="name">文件系统名称。</param>
        /// <param name="resourceIndexes">文件系统包含的资源索引集合。</param>
        public PackageVersionFileSystem(string name, int[] resourceIndexes)
        {
            AssertionUtils.NotNull(name);
            this.name = name;
            this.resourceIndexes = resourceIndexes ?? CollectionUtils.EMPTY_INT_ARRAY;
        }

        /// <summary>
        /// 获取文件系统名称。
        /// </summary>
        public string Name
        {
            get { return name; }
        }

        /// <summary>
        /// 获取文件系统包含的资源索引集合。
        /// </summary>
        /// <returns>文件系统包含的资源索引集合。</returns>
        public int[] GetResourceIndexes()
        {
            return resourceIndexes;
        }
    }
}