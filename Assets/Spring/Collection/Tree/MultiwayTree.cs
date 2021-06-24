using System;
using Spring.Util;

namespace Spring.Collection.Tree
{
    /// <summary>
    /// 数据结点管理器。
    /// </summary>
    public sealed class MultiwayTree
    {
        private static readonly string ROOT_NAME = "<Root>";

        private TreeNode rootNode;

        /// <summary>
        /// 初始化数据结点管理器的新实例。
        /// </summary>
        public MultiwayTree()
        {
            rootNode = TreeNode.ValueOf(ROOT_NAME, null);
        }

        /// <summary>
        /// 获取根数据结点。
        /// </summary>
        public TreeNode RootNode
        {
            get { return rootNode; }
        }


        /// <summary>
        /// 根据类型获取数据结点的数据。
        /// </summary>
        /// <typeparam name="T">要获取的数据类型。</typeparam>
        /// <param name="path">相对于 node 的查找路径。</param>
        /// <returns>指定类型的数据。</returns>
        public T GetData<T>(string path)
        {
            return GetData<T>(path, null);
        }

        /// <summary>
        /// 获取数据结点的数据。
        /// </summary>
        /// <param name="path">相对于 node 的查找路径。</param>
        /// <returns>数据结点的数据。</returns>
        public object GetData(string path)
        {
            return GetData(path, null);
        }

        /// <summary>
        /// 根据类型获取数据结点的数据。
        /// </summary>
        /// <typeparam name="T">要获取的数据类型。</typeparam>
        /// <param name="path">相对于 node 的查找路径。</param>
        /// <param name="node">查找起始结点。</param>
        /// <returns>指定类型的数据。</returns>
        public T GetData<T>(string path, TreeNode node)
        {
            TreeNode current = GetNode(path, node);
            if (current == null)
            {
                throw new Exception(StringUtils.Format("Data node is not exist, path '{}', node '{}'.",
                    path, node != null ? node.FullName : string.Empty));
            }

            return current.GetData<T>();
        }

        /// <summary>
        /// 获取数据结点的数据。
        /// </summary>
        /// <param name="path">相对于 node 的查找路径。</param>
        /// <param name="node">查找起始结点。</param>
        /// <returns>数据结点的数据。</returns>
        public object GetData(string path, TreeNode node)
        {
            TreeNode current = GetNode(path, node);
            if (current == null)
            {
                throw new Exception(StringUtils.Format("Data node is not exist, path '{}', node '{}'.",
                    path, node != null ? node.FullName : string.Empty));
            }

            return current.GetData();
        }

        public void SetData(string path, object data)
        {
            SetData(path, data, null);
        }

        public void SetData(string path, object data, TreeNode node)
        {
            TreeNode current = GetOrAddNode(path, node);
            current.SetData(data);
        }

        /// <summary>
        /// 获取数据结点。
        /// </summary>
        /// <param name="path">相对于 node 的查找路径。</param>
        /// <returns>指定位置的数据结点，如果没有找到，则返回空。</returns>
        public TreeNode GetNode(string path)
        {
            return GetNode(path, null);
        }

        /// <summary>
        /// 获取数据结点。
        /// </summary>
        /// <param name="path">相对于 node 的查找路径。</param>
        /// <param name="node">查找起始结点。</param>
        /// <returns>指定位置的数据结点，如果没有找到，则返回空。</returns>
        public TreeNode GetNode(string path, TreeNode node)
        {
            TreeNode current = node ?? rootNode;
            string[] splitedPath = GetSplitedPath(path);
            foreach (string i in splitedPath)
            {
                current = current.GetChild(i);
                if (current == null)
                {
                    return null;
                }
            }

            return current;
        }

        /// <summary>
        /// 获取或增加数据结点。
        /// </summary>
        /// <param name="path">相对于 node 的查找路径。</param>
        /// <returns>指定位置的数据结点，如果没有找到，则创建相应的数据结点。</returns>
        public TreeNode GetOrAddNode(string path)
        {
            return GetOrAddNode(path, null);
        }

        /// <summary>
        /// 获取或增加数据结点。
        /// </summary>
        /// <param name="path">相对于 node 的查找路径。</param>
        /// <param name="node">查找起始结点。</param>
        /// <returns>指定位置的数据结点，如果没有找到，则增加相应的数据结点。</returns>
        public TreeNode GetOrAddNode(string path, TreeNode node)
        {
            TreeNode current = node ?? rootNode;
            string[] splitedPath = GetSplitedPath(path);
            foreach (string i in splitedPath)
            {
                current = current.GetOrAddChild(i);
            }

            return current;
        }

        /// <summary>
        /// 移除数据结点。
        /// </summary>
        /// <param name="path">相对于 node 的查找路径。</param>
        public void RemoveNode(string path)
        {
            RemoveNode(path, null);
        }

        /// <summary>
        /// 移除数据结点。
        /// </summary>
        /// <param name="path">相对于 node 的查找路径。</param>
        /// <param name="node">查找起始结点。</param>
        public void RemoveNode(string path, TreeNode node)
        {
            TreeNode current = node ?? rootNode;
            TreeNode parent = current.Parent;
            string[] splitedPath = GetSplitedPath(path);
            foreach (string i in splitedPath)
            {
                parent = current;
                current = current.GetChild(i);
                if (current == null)
                {
                    return;
                }
            }

            if (parent != null)
            {
                parent.RemoveChild(current.Name);
            }
        }

        /// <summary>
        /// 移除所有数据结点。
        /// </summary>
        public void Clear()
        {
            rootNode.Clear();
        }

        /// <summary>
        /// 数据结点路径切分工具函数。
        /// </summary>
        /// <param name="path">要切分的数据结点路径。</param>
        /// <returns>切分后的字符串数组。</returns>
        private static string[] GetSplitedPath(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                return StringUtils.EMPTY_STRING_ARRAY;
            }

            return path.Split(PathUtils.PATH_SPLIT_SEPARATOR, StringSplitOptions.RemoveEmptyEntries);
        }
    }
}