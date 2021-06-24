using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Spring.Collection
{
    /// <summary>
    /// 游戏框架链表范围。
    /// </summary>
    /// <typeparam name="T">指定链表范围的元素类型。</typeparam>
    [StructLayout(LayoutKind.Auto)]
    public struct LinkedListRange<T> : IEnumerable<T>, IEnumerable
    {
        private readonly LinkedListNode<T> first;
        private readonly LinkedListNode<T> terminal;

        /// <summary>
        /// 初始化游戏框架链表范围的新实例。
        /// </summary>
        /// <param name="first">链表范围的开始结点。</param>
        /// <param name="terminal">链表范围的终结标记结点。</param>
        public LinkedListRange(LinkedListNode<T> first, LinkedListNode<T> terminal)
        {
            if (first == null || terminal == null || first == terminal)
            {
                throw new Exception("Range is invalid.");
            }

            this.first = first;
            this.terminal = terminal;
        }

        /// <summary>
        /// 获取链表范围是否有效。
        /// </summary>
        public bool IsValid
        {
            get { return first != null && terminal != null && first != terminal; }
        }

        /// <summary>
        /// 获取链表范围的开始结点。
        /// </summary>
        public LinkedListNode<T> First
        {
            get { return first; }
        }

        /// <summary>
        /// 获取链表范围的终结标记结点。
        /// </summary>
        public LinkedListNode<T> Terminal
        {
            get { return terminal; }
        }

        /// <summary>
        /// 获取链表范围的结点数量。
        /// </summary>
        public int Count
        {
            get
            {
                if (!IsValid)
                {
                    return 0;
                }

                int count = 0;
                for (LinkedListNode<T> current = first; current != null && current != terminal; current = current.Next)
                {
                    count++;
                }

                return count;
            }
        }

        /// <summary>
        /// 检查是否包含指定值。
        /// </summary>
        /// <param name="value">要检查的值。</param>
        /// <returns>是否包含指定值。</returns>
        public bool Contains(T value)
        {
            for (LinkedListNode<T> current = first; current != null && current != terminal; current = current.Next)
            {
                if (current.Value.Equals(value))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// 返回循环访问集合的枚举数。
        /// </summary>
        /// <returns>循环访问集合的枚举数。</returns>
        public Enumerator GetEnumerator()
        {
            return new Enumerator(this);
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
        public struct Enumerator : IEnumerator<T>, IEnumerator
        {
            private readonly LinkedListRange<T> linkedListRange;
            private LinkedListNode<T> current;
            private T currentValue;

            public Enumerator(LinkedListRange<T> range)
            {
                if (!range.IsValid)
                {
                    throw new Exception("Range is invalid.");
                }

                linkedListRange = range;
                current = linkedListRange.first;
                currentValue = default(T);
            }

            /// <summary>
            /// 获取当前结点。
            /// </summary>
            public T Current
            {
                get { return currentValue; }
            }

            /// <summary>
            /// 获取当前的枚举数。
            /// </summary>
            object IEnumerator.Current
            {
                get { return currentValue; }
            }

            /// <summary>
            /// 清理枚举数。
            /// </summary>
            public void Dispose()
            {
            }

            /// <summary>
            /// 获取下一个结点。
            /// </summary>
            /// <returns>返回下一个结点。</returns>
            public bool MoveNext()
            {
                if (current == null || current == linkedListRange.terminal)
                {
                    return false;
                }

                currentValue = current.Value;
                current = current.Next;
                return true;
            }

            /// <summary>
            /// 重置枚举数。
            /// </summary>
            void IEnumerator.Reset()
            {
                current = linkedListRange.first;
                currentValue = default(T);
            }
        }
    }
}