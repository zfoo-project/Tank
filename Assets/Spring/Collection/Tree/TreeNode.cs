using System;
using System.Collections.Generic;
using Spring.Util;

namespace Spring.Collection.Tree
{
    /// <summary>
    /// 数据结点。
    /// </summary>
    public sealed class TreeNode
    {
        private static readonly TreeNode[] EmptyDataNodeArray = new TreeNode[] { };

        private string name;
        private object data;
        private TreeNode parent;
        private List<TreeNode> children;


        /// <summary>
        /// 创建数据结点。
        /// </summary>
        /// <param name="name">数据结点名称。</param>
        /// <param name="parent">父数据结点。</param>
        /// <returns>创建的数据结点。</returns>
        public static TreeNode ValueOf(string name, TreeNode parent)
        {
            if (!IsValidName(name))
            {
                throw new Exception("Name of data node is invalid.");
            }

            TreeNode node = new TreeNode();
            node.name = name;
            node.parent = parent;
            return node;
        }

        /// <summary>
        /// 获取数据结点的名称。
        /// </summary>
        public string Name
        {
            get { return name; }
        }

        /// <summary>
        /// 获取数据结点的完整名称。
        /// </summary>
        public string FullName
        {
            get
            {
                return parent == null
                    ? name
                    : StringUtils.Format("{}{}{}", parent.FullName
                        , PathUtils.PATH_SPLIT_SEPARATOR[0], name);
            }
        }

        /// <summary>
        /// 获取父数据结点。
        /// </summary>
        public TreeNode Parent
        {
            get { return parent; }
        }

        /// <summary>
        /// 获取子数据结点的数量。
        /// </summary>
        public int ChildCount
        {
            get { return children != null ? children.Count : 0; }
        }

        /// <summary>
        /// 根据类型获取数据结点的数据。
        /// </summary>
        /// <typeparam name="T">要获取的数据类型。</typeparam>
        /// <returns>指定类型的数据。</returns>
        public T GetData<T>()
        {
            return (T) data;
        }

        /// <summary>
        /// 获取数据结点的数据。
        /// </summary>
        /// <returns>数据结点数据。</returns>
        public object GetData()
        {
            return data;
        }

        public void SetData(object data)
        {
            this.data = data;
        }

        /// <summary>
        /// 根据索引检查是否存在子数据结点。
        /// </summary>
        /// <param name="index">子数据结点的索引。</param>
        /// <returns>是否存在子数据结点。</returns>
        public bool HasChild(int index)
        {
            return index >= 0 && index < ChildCount;
        }

        /// <summary>
        /// 根据名称检查是否存在子数据结点。
        /// </summary>
        /// <param name="name">子数据结点名称。</param>
        /// <returns>是否存在子数据结点。</returns>
        public bool HasChild(string name)
        {
            if (!IsValidName(name))
            {
                throw new Exception("Name is invalid.");
            }

            if (children == null)
            {
                return false;
            }

            foreach (TreeNode child in children)
            {
                if (child.Name == name)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// 根据索引获取子数据结点。
        /// </summary>
        /// <param name="index">子数据结点的索引。</param>
        /// <returns>指定索引的子数据结点，如果索引越界，则返回空。</returns>
        public TreeNode GetChild(int index)
        {
            return index >= 0 && index < ChildCount ? children[index] : null;
        }

        /// <summary>
        /// 根据名称获取子数据结点。
        /// </summary>
        /// <param name="name">子数据结点名称。</param>
        /// <returns>指定名称的子数据结点，如果没有找到，则返回空。</returns>
        public TreeNode GetChild(string name)
        {
            if (!IsValidName(name))
            {
                throw new Exception("Name is invalid.");
            }

            if (children == null)
            {
                return null;
            }

            foreach (TreeNode child in children)
            {
                if (child.Name == name)
                {
                    return child;
                }
            }

            return null;
        }

        /// <summary>
        /// 根据名称获取或增加子数据结点。
        /// </summary>
        /// <param name="name">子数据结点名称。</param>
        /// <returns>指定名称的子数据结点，如果对应名称的子数据结点已存在，则返回已存在的子数据结点，否则增加子数据结点。</returns>
        public TreeNode GetOrAddChild(string name)
        {
            TreeNode node = (TreeNode) GetChild(name);
            if (node != null)
            {
                return node;
            }

            node = ValueOf(name, this);

            if (children == null)
            {
                children = new List<TreeNode>();
            }

            children.Add(node);

            return node;
        }

        /// <summary>
        /// 获取所有子数据结点。
        /// </summary>
        /// <returns>所有子数据结点。</returns>
        public TreeNode[] GetAllChild()
        {
            if (children == null)
            {
                return EmptyDataNodeArray;
            }

            return children.ToArray();
        }

        /// <summary>
        /// 获取所有子数据结点。
        /// </summary>
        /// <param name="results">所有子数据结点。</param>
        public void GetAllChild(List<TreeNode> results)
        {
            if (results == null)
            {
                throw new Exception("Results is invalid.");
            }

            results.Clear();
            if (children == null)
            {
                return;
            }

            foreach (TreeNode child in children)
            {
                results.Add(child);
            }
        }

        /// <summary>
        /// 根据索引移除子数据结点。
        /// </summary>
        /// <param name="index">子数据结点的索引位置。</param>
        public void RemoveChild(int index)
        {
            TreeNode node = (TreeNode) GetChild(index);
            if (node == null)
            {
                return;
            }

            children.Remove(node);
        }

        /// <summary>
        /// 根据名称移除子数据结点。
        /// </summary>
        /// <param name="name">子数据结点名称。</param>
        public void RemoveChild(string name)
        {
            TreeNode node = (TreeNode) GetChild(name);
            if (node == null)
            {
                return;
            }

            children.Remove(node);
        }

        public void Clear()
        {
            name = null;
            data = null;
            parent = null;
            children = null;
        }

        /// <summary>
        /// 获取数据结点字符串。
        /// </summary>
        /// <returns>数据结点字符串。</returns>
        public override string ToString()
        {
            return StringUtils.Format("{}: {}", FullName, ToDataString());
        }

        /// <summary>
        /// 获取数据字符串。
        /// </summary>
        /// <returns>数据字符串。</returns>
        public string ToDataString()
        {
            if (data == null)
            {
                return "<Null>";
            }

            return StringUtils.Format("[{}] {}", data.GetType().Name, data.ToString());
        }

        /// <summary>
        /// 检测数据结点名称是否合法。
        /// </summary>
        /// <param name="name">要检测的数据结点名称。</param>
        /// <returns>是否是合法的数据结点名称。</returns>
        private static bool IsValidName(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                return false;
            }

            foreach (string pathSplitSeparator in PathUtils.PATH_SPLIT_SEPARATOR)
            {
                if (name.Contains(pathSplitSeparator))
                {
                    return false;
                }
            }

            return true;
        }
    }
}