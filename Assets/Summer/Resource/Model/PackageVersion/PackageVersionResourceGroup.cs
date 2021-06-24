using System.Runtime.InteropServices;
using Spring.Util;

namespace Summer.Resource.Model.PackageVersion
{
    /// <summary>
    /// 资源组。
    /// </summary>
    [StructLayout(LayoutKind.Auto)]
    public struct PackageVersionResourceGroup
    {
        private readonly string name;
        private readonly int[] resourceIndexes;

        /// <summary>
        /// 初始化资源组的新实例。
        /// </summary>
        /// <param name="name">资源组名称。</param>
        /// <param name="resourceIndexes">资源组包含的资源索引集合。</param>
        public PackageVersionResourceGroup(string name, int[] resourceIndexes)
        {
            AssertionUtils.NotNull(name);
            this.name = name;
            this.resourceIndexes = resourceIndexes ?? CollectionUtils.EMPTY_INT_ARRAY;
        }

        /// <summary>
        /// 获取资源组名称。
        /// </summary>
        public string Name
        {
            get { return name; }
        }

        /// <summary>
        /// 获取资源组包含的资源索引集合。
        /// </summary>
        /// <returns>资源组包含的资源索引集合。</returns>
        public int[] GetResourceIndexes()
        {
            return resourceIndexes;
        }
    }
}