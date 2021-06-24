using System;
using System.Runtime.InteropServices;

namespace Spring.Collection.Reference
{
    /// <summary>
    /// 引用池信息。
    /// </summary>
    [StructLayout(LayoutKind.Auto)]
    public struct ReferenceCacheInfo
    {
        private readonly Type type;
        private readonly int unusedReferenceCount;
        private readonly int usingReferenceCount;
        private readonly int acquireReferenceCount;
        private readonly int releaseReferenceCount;
        private readonly int addReferenceCount;
        private readonly int removeReferenceCount;

        /// <summary>
        /// 初始化引用池信息的新实例。
        /// </summary>
        /// <param name="type">引用池类型。</param>
        /// <param name="unusedReferenceCount">未使用引用数量。</param>
        /// <param name="usingReferenceCount">正在使用引用数量。</param>
        /// <param name="acquireReferenceCount">获取引用数量。</param>
        /// <param name="releaseReferenceCount">归还引用数量。</param>
        /// <param name="addReferenceCount">增加引用数量。</param>
        /// <param name="removeReferenceCount">移除引用数量。</param>
        public ReferenceCacheInfo(Type type, int unusedReferenceCount, int usingReferenceCount, int acquireReferenceCount, int releaseReferenceCount, int addReferenceCount, int removeReferenceCount)
        {
            this.type = type;
            this.unusedReferenceCount = unusedReferenceCount;
            this.usingReferenceCount = usingReferenceCount;
            this.acquireReferenceCount = acquireReferenceCount;
            this.releaseReferenceCount = releaseReferenceCount;
            this.addReferenceCount = addReferenceCount;
            this.removeReferenceCount = removeReferenceCount;
        }

        /// <summary>
        /// 获取引用池类型。
        /// </summary>
        public Type Type
        {
            get
            {
                return type;
            }
        }

        /// <summary>
        /// 获取未使用引用数量。
        /// </summary>
        public int UnusedReferenceCount
        {
            get
            {
                return unusedReferenceCount;
            }
        }

        /// <summary>
        /// 获取正在使用引用数量。
        /// </summary>
        public int UsingReferenceCount
        {
            get
            {
                return usingReferenceCount;
            }
        }

        /// <summary>
        /// 获取获取引用数量。
        /// </summary>
        public int AcquireReferenceCount
        {
            get
            {
                return acquireReferenceCount;
            }
        }

        /// <summary>
        /// 获取归还引用数量。
        /// </summary>
        public int ReleaseReferenceCount
        {
            get
            {
                return releaseReferenceCount;
            }
        }

        /// <summary>
        /// 获取增加引用数量。
        /// </summary>
        public int AddReferenceCount
        {
            get
            {
                return addReferenceCount;
            }
        }

        /// <summary>
        /// 获取移除引用数量。
        /// </summary>
        public int RemoveReferenceCount
        {
            get
            {
                return removeReferenceCount;
            }
        }
    }
}
