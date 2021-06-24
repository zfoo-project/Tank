using System.Runtime.InteropServices;
using Summer.Base;
using Summer.Base.Model;

namespace Summer.Resource.Model.LocalVersion
{
    /// <summary>
    /// 文件系统。
    /// </summary>
    [StructLayout(LayoutKind.Auto)]
    public struct LocalVersionFileSystem
    {
        private static readonly int[] EmptyIntArray = new int[] { };

        private readonly string name;
        private readonly int[] resourceIndexes;

        /// <summary>
        /// 初始化文件系统的新实例。
        /// </summary>
        /// <param name="name">文件系统名称。</param>
        /// <param name="resourceIndexes">文件系统包含的资源索引集合。</param>
        public LocalVersionFileSystem(string name, int[] resourceIndexes)
        {
            if (name == null)
            {
                throw new GameFrameworkException("Name is invalid.");
            }

            this.name = name;
            this.resourceIndexes = resourceIndexes ?? EmptyIntArray;
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