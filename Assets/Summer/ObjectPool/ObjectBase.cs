using System;
using Summer.Base;
using Summer.Base.Model;
using Spring.Collection.Reference;
using Spring.Util;

namespace Summer.ObjectPool
{
    /// <summary>
    /// 对象基类。
    /// </summary>
    public abstract class ObjectBase : IReference
    {
        private string name;
        private object target;
        private bool locked;
        private int spawnCount;
        private int priority;
        private DateTime lastUseTime;

        /// <summary>
        /// 初始化对象基类的新实例。
        /// </summary>
        public ObjectBase()
        {
            name = null;
            target = null;
            locked = false;
            spawnCount = 0;
            priority = 0;
            lastUseTime = default(DateTime);
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
        /// 获取对象。
        /// </summary>
        public object Target
        {
            get
            {
                return target;
            }
        }

        /// <summary>
        /// 获取或设置对象是否被加锁。
        /// </summary>
        public bool Locked
        {
            get
            {
                return locked;
            }
            set
            {
                locked = value;
            }
        }
        
        /// <summary>
        /// 获取对象是否正在使用。
        /// </summary>
        public bool IsInUse
        {
            get { return spawnCount > 0; }
        }

        /// <summary>
        /// 获取对象的获取计数。
        /// </summary>
        public int SpawnCount
        {
            get => spawnCount;
            set => spawnCount = value;
        }

        /// <summary>
        /// 获取或设置对象的优先级。
        /// </summary>
        public int Priority
        {
            get
            {
                return priority;
            }
            set
            {
                priority = value;
            }
        }

        /// <summary>
        /// 获取自定义释放检查标记。
        /// </summary>
        public virtual bool CustomCanReleaseFlag
        {
            get
            {
                return true;
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
            set
            {
                lastUseTime = value;
            }
        }

        /// <summary>
        /// 初始化对象基类。
        /// </summary>
        /// <param name="target">对象。</param>
        protected void Initialize(object target)
        {
            Initialize(null, target, false, 0);
        }

        /// <summary>
        /// 初始化对象基类。
        /// </summary>
        /// <param name="name">对象名称。</param>
        /// <param name="target">对象。</param>
        protected void Initialize(string name, object target)
        {
            Initialize(name, target, false, 0);
        }

        /// <summary>
        /// 初始化对象基类。
        /// </summary>
        /// <param name="name">对象名称。</param>
        /// <param name="target">对象。</param>
        /// <param name="locked">对象是否被加锁。</param>
        protected void Initialize(string name, object target, bool locked)
        {
            Initialize(name, target, locked, 0);
        }

        /// <summary>
        /// 初始化对象基类。
        /// </summary>
        /// <param name="name">对象名称。</param>
        /// <param name="target">对象。</param>
        /// <param name="priority">对象的优先级。</param>
        protected void Initialize(string name, object target, int priority)
        {
            Initialize(name, target, false, priority);
        }

        /// <summary>
        /// 初始化对象基类。
        /// </summary>
        /// <param name="name">对象名称。</param>
        /// <param name="target">对象。</param>
        /// <param name="locked">对象是否被加锁。</param>
        /// <param name="priority">对象的优先级。</param>
        protected void Initialize(string name, object target, bool locked, int priority)
        {
            if (target == null)
            {
                throw new GameFrameworkException(StringUtils.Format("Target '{}' is invalid.", name));
            }

            this.name = name ?? string.Empty;
            this.target = target;
            this.locked = locked;
            this.priority = priority;
            this.lastUseTime = DateTime.UtcNow;
        }

        /// <summary>
        /// 清理对象基类。
        /// </summary>
        public virtual void Clear()
        {
            name = null;
            target = null;
            locked = false;
            spawnCount = 0;
            priority = 0;
            lastUseTime = default(DateTime);
        }
        
        /// <summary>
        /// 获取对象。
        /// </summary>
        /// <returns>对象。</returns>
        public ObjectBase Spawn()
        {
            spawnCount++;
            lastUseTime = DateTime.UtcNow;
            OnSpawn();
            return this;
        }
        
        /// <summary>
        /// 回收对象。
        /// </summary>
        public void Unspawn()
        {
            OnUnspawn();
            lastUseTime = DateTime.UtcNow;
            spawnCount--;
            if (spawnCount < 0)
            {
                throw new GameFrameworkException(StringUtils.Format("Object '{}' spawn count is less than 0.", Name));
            }
        }

        /// <summary>
        /// 获取对象时的事件。
        /// </summary>
        public virtual void OnSpawn()
        {
        }

        /// <summary>
        /// 回收对象时的事件。
        /// </summary>
        public virtual void OnUnspawn()
        {
        }

        /// <summary>
        /// 释放对象。
        /// </summary>
        /// <param name="isShutdown">是否是关闭对象池时触发。</param>
        public abstract void Release(bool isShutdown);
    }
}
