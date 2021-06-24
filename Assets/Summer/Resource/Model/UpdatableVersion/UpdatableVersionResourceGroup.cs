using System.Runtime.InteropServices;
using Summer.Base;
using Summer.Base.Model;

namespace Summer.Resource.Model.UpdatableVersion
{
    /// <summary>
    /// 资源组。
    /// </summary>
    [StructLayout(LayoutKind.Auto)]
    public struct UpdatableVersionResourceGroup
    {
        private static readonly int[] EmptyIntArray = new int[] { };

        private readonly string name;
        private readonly int[] resourceIndexes;

        /// <summary>
        /// 初始化资源组的新实例。
        /// </summary>
        /// <param name="name">资源组名称。</param>
        /// <param name="resourceIndexes">资源组包含的资源索引集合。</param>
        public UpdatableVersionResourceGroup(string name, int[] resourceIndexes)
        {
            if (name == null)
            {
                throw new GameFrameworkException("Name is invalid.");
            }

            this.name = name;
            this.resourceIndexes = resourceIndexes ?? EmptyIntArray;
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