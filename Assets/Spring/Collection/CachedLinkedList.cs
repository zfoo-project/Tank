using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Spring.Collection
{
    /// <summary>
    /// 游戏框架链表类。
    /// </summary>
    /// <typeparam name="T">指定链表的元素类型。</typeparam>
    public sealed class CachedLinkedList<T> : ICollection<T>,  ICollection
    {
        private readonly LinkedList<T> linkedList;
        private readonly Queue<LinkedListNode<T>> cachedNodes;

        /// <summary>
        /// 初始化游戏框架链表类的新实例。
        /// </summary>
        public CachedLinkedList()
        {
            linkedList = new LinkedList<T>();
            cachedNodes = new Queue<LinkedListNode<T>>();
        }

        /// <summary>
        /// 获取链表中实际包含的结点数量。
        /// </summary>
        public int Count
        {
            get { return linkedList.Count; }
        }


        /// <summary>
        /// 获取链表的第一个结点。
        /// </summary>
        public LinkedListNode<T> First
        {
            get { return linkedList.First; }
        }

        /// <summary>
        /// 获取链表的最后一个结点。
        /// </summary>
        public LinkedListNode<T> Last
        {
            get { return linkedList.Last; }
        }

        /// <summary>
        /// 获取一个值，该值指示 ICollection`1 是否为只读。
        /// </summary>
        public bool IsReadOnly
        {
            get { return ((ICollection<T>) linkedList).IsReadOnly; }
        }

        /// <summary>
        /// 获取可用于同步对 ICollection 的访问的对象。
        /// </summary>
        public object SyncRoot
        {
            get { return ((ICollection) linkedList).SyncRoot; }
        }

        /// <summary>
        /// 获取一个值，该值指示是否同步对 ICollection 的访问（线程安全）。
        /// </summary>
        public bool IsSynchronized
        {
            get { return ((ICollection) linkedList).IsSynchronized; }
        }

        /// <summary>
        /// 将值添加到 ICollection`1 的结尾处。
        /// </summary>
        /// <param name="value">要添加的值。</param>
        public void Add(T value)
        {
            AddLast(value);
        }

        /// <summary>
        /// 在链表中指定的现有结点后添加包含指定值的新结点。
        /// </summary>
        /// <param name="node">指定的现有结点。</param>
        /// <param name="value">指定值。</param>
        /// <returns>包含指定值的新结点。</returns>
        public LinkedListNode<T> AddAfter(LinkedListNode<T> node, T value)
        {
            LinkedListNode<T> newNode = AcquireNode(value);
            linkedList.AddAfter(node, newNode);
            return newNode;
        }


        /// <summary>
        /// 在链表中指定的现有结点前添加包含指定值的新结点。
        /// </summary>
        /// <param name="node">指定的现有结点。</param>
        /// <param name="value">指定值。</param>
        /// <returns>包含指定值的新结点。</returns>
        public LinkedListNode<T> AddBefore(LinkedListNode<T> node, T value)
        {
            LinkedListNode<T> newNode = AcquireNode(value);
            linkedList.AddBefore(node, newNode);
            return newNode;
        }


        /// <summary>
        /// 在链表的开头处添加包含指定值的新结点。
        /// </summary>
        /// <param name="value">指定值。</param>
        /// <returns>包含指定值的新结点。</returns>
        public LinkedListNode<T> AddFirst(T value)
        {
            LinkedListNode<T> node = AcquireNode(value);
            linkedList.AddFirst(node);
            return node;
        }


        /// <summary>
        /// 在链表的结尾处添加包含指定值的新结点。
        /// </summary>
        /// <param name="value">指定值。</param>
        /// <returns>包含指定值的新结点。</returns>
        public LinkedListNode<T> AddLast(T value)
        {
            LinkedListNode<T> node = AcquireNode(value);
            linkedList.AddLast(node);
            return node;
        }


        /// <summary>
        /// 从链表中移除所有结点。
        /// </summary>
        public void Clear()
        {
            LinkedListNode<T> current = linkedList.First;
            while (current != null)
            {
                ReleaseNode(current);
                current = current.Next;
            }

            linkedList.Clear();
        }


        /// <summary>
        /// 确定某值是否在链表中。
        /// </summary>
        /// <param name="value">指定值。</param>
        /// <returns>某值是否在链表中。</returns>
        public bool Contains(T value)
        {
            return linkedList.Contains(value);
        }

        /// <summary>
        /// 从目标数组的指定索引处开始将整个链表复制到兼容的一维数组。
        /// </summary>
        /// <param name="array">一维数组，它是从链表复制的元素的目标。数组必须具有从零开始的索引。</param>
        /// <param name="index">array 中从零开始的索引，从此处开始复制。</param>
        public void CopyTo(T[] array, int index)
        {
            linkedList.CopyTo(array, index);
        }

        /// <summary>
        /// 从特定的 ICollection 索引开始，将数组的元素复制到一个数组中。
        /// </summary>
        /// <param name="array">一维数组，它是从 ICollection 复制的元素的目标。数组必须具有从零开始的索引。</param>
        /// <param name="index">array 中从零开始的索引，从此处开始复制。</param>
        public void CopyTo(Array array, int index)
        {
            ((ICollection) linkedList).CopyTo(array, index);
        }


        /// <summary>
        /// 从链表中移除指定值的第一个匹配项。
        /// </summary>
        /// <param name="value">指定值。</param>
        /// <returns>是否移除成功。</returns>
        public bool Remove(T value)
        {
            LinkedListNode<T> node = linkedList.Find(value);
            if (node != null)
            {
                linkedList.Remove(node);
                ReleaseNode(node);
                return true;
            }

            return false;
        }

        /// <summary>
        /// 从链表中移除指定的结点。
        /// </summary>
        /// <param name="node">指定的结点。</param>
        public void Remove(LinkedListNode<T> node)
        {
            linkedList.Remove(node);
            ReleaseNode(node);
        }

        /// <summary>
        /// 移除位于链表开头处的结点。
        /// </summary>
        public void RemoveFirst()
        {
            LinkedListNode<T> first = linkedList.First;
            if (first == null)
            {
                throw new Exception("First is invalid.");
            }

            linkedList.RemoveFirst();
            ReleaseNode(first);
        }


        /// <summary>
        /// 返回循环访问集合的枚举数。
        /// </summary>
        /// <returns>循环访问集合的枚举数。</returns>
        public Enumerator GetEnumerator()
        {
            return new Enumerator(linkedList);
        }

        private LinkedListNode<T> AcquireNode(T value)
        {
            LinkedListNode<T> node = null;
            if (cachedNodes.Count > 0)
            {
                node = cachedNodes.Dequeue();
                node.Value = value;
            }
            else
            {
                node = new LinkedListNode<T>(value);
            }

            return node;
        }

        private void ReleaseNode(LinkedListNode<T> node)
        {
            node.Value = default(T);
            cachedNodes.Enqueue(node);
        }

        /// <summary>
        /// 返回循环访问集合的枚举数。
        /// </summary>
        /// <returns>循环访问集合的枚举数。</returns>
        IEnumerator<T> IEnumerable<T>.GetEnumerator()
        {
            return GetEnumerator();
        }

        /// <summary>
        /// 返回循环访问集合的枚举数。
        /// </summary>
        /// <returns>循环访问集合的枚举数。</returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        /// <summary>
        /// 循环访问集合的枚举数。
        /// </summary>
        [StructLayout(LayoutKind.Auto)]
        public struct Enumerator : IEnumerator<T>
        {
            private LinkedList<T>.Enumerator enumerator;

            public Enumerator(LinkedList<T> linkedList)
            {
                if (linkedList == null)
                {
                    throw new Exception("Linked list is invalid.");
                }

                enumerator = linkedList.GetEnumerator();
            }

            /// <summary>
            /// 获取当前结点。
            /// </summary>
            public T Current
            {
                get { return enumerator.Current; }
            }

            /// <summary>
            /// 获取当前的枚举数。
            /// </summary>
            object IEnumerator.Current
            {
                get { return enumerator.Current; }
            }

            /// <summary>
            /// 清理枚举数。
            /// </summary>
            public void Dispose()
            {
                enumerator.Dispose();
            }

            /// <summary>
            /// 获取下一个结点。
            /// </summary>
            /// <returns>返回下一个结点。</returns>
            public bool MoveNext()
            {
                return enumerator.MoveNext();
            }

            /// <summary>
            /// 重置枚举数。
            /// </summary>
            void IEnumerator.Reset()
            {
                ((IEnumerator<T>) enumerator).Reset();
            }
        }
    }
}