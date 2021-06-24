using System;
using System.Runtime.InteropServices;

namespace Summer.ObjectPool
{
    /// <summary>
    /// 对象信息。
    /// </summary>
    [StructLayout(LayoutKind.Auto)]
    public struct ObjectInfo
    {
        private readonly string name;
        private readonly bool locked;
        private readonly bool customCanReleaseFlag;
        private readonly int priority;
        private readonly DateTime lastUseTime;
        private readonly int spawnCount;

        /// <summary>
        /// 初始化对象信息的新实例。
        /// </summary>
        /// <param name="name">对象名称。</param>
        /// <param name="locked">对象是否被加锁。</param>
        /// <param name="customCanReleaseFlag">对象自定义释放检查标记。</param>
        /// <param name="priority">对象的优先级。</param>
        /// <param name="lastUseTime">对象上次使用时间。</param>
        /// <param name="spawnCount">对象的获取计数。</param>
        public ObjectInfo(string name, bool locked, bool customCanReleaseFlag, int priority, DateTime lastUseTime, int spawnCount)
        {
            this.name = name;
            this.locked = locked;
            this.customCanReleaseFlag = customCanReleaseFlag;
            this.priority = priority;
            this.lastUseTime = lastUseTime;
            this.spawnCount = spawnCount;
        }

        /// <summary>
        /// 获取对象名称。
        /// </summary>
        public string Name
        {
            get
            {
                return name;
            }
        }

        /// <summary>
        /// 获取对象是否被加锁。
        /// </summary>
        public bool Locked
        {
            get
            {
                return locked;
            }
        }

        /// <summary>
        /// 获取对象自定义释放检查标记。
        /// </summary>
        public bool CustomCanReleaseFlag
        {
            get
            {
                return customCanReleaseFlag;
            }
        }

        /// <summary>
        /// 获取对象的优先级。
        /// </summary>
        public int Priority
        {
            get
            {
                return priority;
            }
        }

        /// <summary>
        /// 获取对象上次使用时间。
        /// </summary>
        public DateTime LastUseTime
        {
            get
            {
                return lastUseTime;
            }
        }

        /// <summary>
        /// 获取对象是否正在使用。
        /// </summary>
        public bool IsInUse
        {
            get
            {
                return spawnCount > 0;
            }
        }

        /// <summary>
        /// 获取对象的获取计数。
        /// </summary>
        public int SpawnCount
        {
            get
            {
                return spawnCount;
            }
        }
    }
}
