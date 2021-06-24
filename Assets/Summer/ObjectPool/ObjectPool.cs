using System;
using System.Collections.Generic;
using Summer.Base.Model;
using Spring.Collection;
using Spring.Collection.Reference;
using Spring.Util;

namespace Summer.ObjectPool
{
    /// <summary>
    /// 对象池。
    /// </summary>
    /// <typeparam name="T">对象类型。</typeparam>
    public sealed class ObjectPool<T> : ObjectPoolBase, IObjectPool<T> where T : ObjectBase, new()
    {
        private readonly MultiValueDictionary<string, T> objectMap = new MultiValueDictionary<string, T>();
        private readonly Dictionary<object, T> targetObjectMap = new Dictionary<object, T>();
        private readonly ReleaseObjectFilterCallback<T> defaultReleaseObjectFilterCallback;
        private readonly List<T> cachedCanReleaseObjects = new List<T>();
        private readonly List<T> cachedToReleaseObjects = new List<T>();
        private readonly bool allowMultiSpawn;
        private float autoReleaseInterval;
        private int capacity;
        private float expireTime;
        private int priority;
        private float autoReleaseTime = 0f;

        /// <summary>
        /// 初始化对象池的新实例。
        /// </summary>
        /// <param name="allowMultiSpawn">是否允许对象被多次获取。</param>
        /// <param name="autoReleaseInterval">对象池自动释放可释放对象的间隔秒数。</param>
        /// <param name="capacity">对象池的容量。</param>
        /// <param name="expireTime">对象池对象过期秒数。</param>
        /// <param name="priority">对象池的优先级。</param>
        public ObjectPool(bool allowMultiSpawn, float autoReleaseInterval, int capacity, float expireTime, int priority)
        {
            this.defaultReleaseObjectFilterCallback = DefaultReleaseObjectFilterCallback;
            this.allowMultiSpawn = allowMultiSpawn;
            this.autoReleaseInterval = autoReleaseInterval;
            this.capacity = capacity;
            this.expireTime = expireTime;
            this.priority = priority;
        }

        /// <summary>
        /// 获取对象池对象类型。
        /// </summary>
        public override Type ObjectType
        {
            get { return typeof(T); }
        }

        /// <summary>
        /// 获取对象池中对象的数量。
        /// </summary>
        public override int Count
        {
            get { return targetObjectMap.Count; }
        }

        /// <summary>
        /// 获取对象池中能被释放的对象的数量。
        /// </summary>
        public override int CanReleaseCount
        {
            get
            {
                GetCanReleaseObjects(cachedCanReleaseObjects);
                return cachedCanReleaseObjects.Count;
            }
        }

        /// <summary>
        /// 获取是否允许对象被多次获取。
        /// </summary>
        public override bool AllowMultiSpawn
        {
            get { return allowMultiSpawn; }
        }

        /// <summary>
        /// 获取或设置对象池自动释放可释放对象的间隔秒数。
        /// </summary>
        public override float AutoReleaseInterval
        {
            get { return autoReleaseInterval; }
            set { autoReleaseInterval = value; }
        }

        /// <summary>
        /// 获取或设置对象池的容量。
        /// </summary>
        public override int Capacity
        {
            get { return capacity; }
            set { capacity = value; }
        }

        /// <summary>
        /// 获取或设置对象池对象过期秒数。
        /// </summary>
        public override float ExpireTime
        {
            get { return expireTime; }

            set { expireTime = value; }
        }

        /// <summary>
        /// 获取或设置对象池的优先级。
        /// </summary>
        public override int Priority
        {
            get { return priority; }
            set { priority = value; }
        }

        /// <summary>
        /// 创建对象。
        /// </summary>
        /// <param name="obj">对象。</param>
        /// <param name="spawned">对象是否已被获取。</param>
        public void Register(T obj, bool spawned)
        {
            obj.SpawnCount = spawned ? 1 : 0;
            if (spawned)
            {
                obj.OnSpawn();
            }

            objectMap.Add(obj.Name, obj);
            targetObjectMap.Add(obj.Target, obj);

            if (Count > capacity)
            {
                Release();
            }
        }

        /// <summary>
        /// 检查对象。
        /// </summary>
        /// <returns>要检查的对象是否存在。</returns>
        public bool CanSpawn()
        {
            return CanSpawn(string.Empty);
        }

        /// <summary>
        /// 检查对象。
        /// </summary>
        /// <param name="name">对象名称。</param>
        /// <returns>要检查的对象是否存在。</returns>
        public bool CanSpawn(string name)
        {
            var objectRange = default(LinkedListRange<T>);
            if (objectMap.TryGetValue(name, out objectRange))
            {
                foreach (var internalObject in objectRange)
                {
                    if (allowMultiSpawn || !internalObject.IsInUse)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// 获取对象。
        /// </summary>
        /// <returns>要获取的对象。</returns>
        public T Spawn()
        {
            return Spawn(string.Empty);
        }

        /// <summary>
        /// 获取对象。
        /// </summary>
        /// <param name="name">对象名称。</param>
        /// <returns>要获取的对象。</returns>
        public T Spawn(string name)
        {
            var objectRange = default(LinkedListRange<T>);
            if (objectMap.TryGetValue(name, out objectRange))
            {
                foreach (var internalObject in objectRange)
                {
                    if (allowMultiSpawn || !internalObject.IsInUse)
                    {
                        return (T) internalObject.Spawn();
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// 回收对象。
        /// </summary>
        /// <param name="obj">要回收的对象。</param>
        public void Unspawn(T obj)
        {
            if (obj == null)
            {
                throw new GameFrameworkException("Object is invalid.");
            }

            Unspawn(obj.Target);
        }

        /// <summary>
        /// 回收对象。
        /// </summary>
        /// <param name="target">要回收的对象。</param>
        public void Unspawn(object target)
        {
            if (target == null)
            {
                throw new GameFrameworkException("Target is invalid.");
            }

            var internalObjectFactory = GetObject(target);
            if (internalObjectFactory != null)
            {
                internalObjectFactory.Unspawn();
                if (Count > capacity && internalObjectFactory.SpawnCount <= 0)
                {
                    Release();
                }
            }
            else
            {
                throw new GameFrameworkException(StringUtils.Format(
                    "Can not find target in object pool '{}', target type is '{}', target value is '{}'.",
                    Name, target.GetType().FullName, target.ToString()));
            }
        }

        /// <summary>
        /// 设置对象是否被加锁。
        /// </summary>
        /// <param name="obj">要设置被加锁的对象。</param>
        /// <param name="locked">是否被加锁。</param>
        public void SetLocked(T obj, bool locked)
        {
            if (obj == null)
            {
                throw new GameFrameworkException("Object is invalid.");
            }

            SetLocked(obj.Target, locked);
        }

        /// <summary>
        /// 设置对象是否被加锁。
        /// </summary>
        /// <param name="target">要设置被加锁的对象。</param>
        /// <param name="locked">是否被加锁。</param>
        public void SetLocked(object target, bool locked)
        {
            if (target == null)
            {
                throw new GameFrameworkException("Target is invalid.");
            }

            var internalObjectFactory = GetObject(target);
            if (internalObjectFactory != null)
            {
                internalObjectFactory.Locked = locked;
            }
            else
            {
                throw new GameFrameworkException(StringUtils.Format(
                    "Can not find target in object pool '{}', target type is '{}', target value is '{}'.",
                    Name, target.GetType().FullName, target.ToString()));
            }
        }

        /// <summary>
        /// 设置对象的优先级。
        /// </summary>
        /// <param name="obj">要设置优先级的对象。</param>
        /// <param name="priority">优先级。</param>
        public void SetPriority(T obj, int priority)
        {
            if (obj == null)
            {
                throw new GameFrameworkException("Object is invalid.");
            }

            SetPriority(obj.Target, priority);
        }

        /// <summary>
        /// 设置对象的优先级。
        /// </summary>
        /// <param name="target">要设置优先级的对象。</param>
        /// <param name="priority">优先级。</param>
        public void SetPriority(object target, int priority)
        {
            if (target == null)
            {
                throw new GameFrameworkException("Target is invalid.");
            }

            var internalObjectFactory = GetObject(target);
            if (internalObjectFactory != null)
            {
                internalObjectFactory.Priority = priority;
            }
            else
            {
                throw new GameFrameworkException(StringUtils.Format(
                    "Can not find target in object pool '{}', target type is '{}', target value is '{}'.",
                    Name, target.GetType().FullName, target.ToString()));
            }
        }

        /// <summary>
        /// 释放对象池中的可释放对象。
        /// </summary>
        public override void Release()
        {
            Release(Count - capacity, defaultReleaseObjectFilterCallback);
        }

        /// <summary>
        /// 释放对象池中的可释放对象。
        /// </summary>
        /// <param name="toReleaseCount">尝试释放对象数量。</param>
        public override void Release(int toReleaseCount)
        {
            Release(toReleaseCount, defaultReleaseObjectFilterCallback);
        }

        /// <summary>
        /// 释放对象池中的可释放对象。
        /// </summary>
        /// <param name="releaseObjectFilterCallback">释放对象筛选函数。</param>
        public void Release(ReleaseObjectFilterCallback<T> releaseObjectFilterCallback)
        {
            Release(Count - capacity, releaseObjectFilterCallback);
        }

        /// <summary>
        /// 释放对象池中的可释放对象。
        /// </summary>
        /// <param name="toReleaseCount">尝试释放对象数量。</param>
        /// <param name="releaseObjectFilterCallback">释放对象筛选函数。</param>
        public void Release(int toReleaseCount, ReleaseObjectFilterCallback<T> releaseObjectFilterCallback)
        {
            if (releaseObjectFilterCallback == null)
            {
                throw new GameFrameworkException("Release object filter callback is invalid.");
            }

            if (toReleaseCount < 0)
            {
                toReleaseCount = 0;
            }

            DateTime expireTime = DateTime.MinValue;
            if (this.expireTime < float.MaxValue)
            {
                expireTime = DateTime.UtcNow.AddSeconds(-this.expireTime);
            }

            autoReleaseTime = 0f;
            GetCanReleaseObjects(cachedCanReleaseObjects);
            List<T> toReleaseObjects =
                releaseObjectFilterCallback(cachedCanReleaseObjects, toReleaseCount, expireTime);
            if (toReleaseObjects == null || toReleaseObjects.Count <= 0)
            {
                return;
            }

            foreach (T toReleaseObject in toReleaseObjects)
            {
                ReleaseObject(toReleaseObject);
            }
        }

        /// <summary>
        /// 释放对象池中的所有未使用对象。
        /// </summary>
        public override void ReleaseAllUnused()
        {
            autoReleaseTime = 0f;
            GetCanReleaseObjects(cachedCanReleaseObjects);
            foreach (T toReleaseObject in cachedCanReleaseObjects)
            {
                ReleaseObject(toReleaseObject);
            }
        }

        /// <summary>
        /// 获取所有对象信息。
        /// </summary>
        /// <returns>所有对象信息。</returns>
        public override ObjectInfo[] GetAllObjectInfos()
        {
            List<ObjectInfo> results = new List<ObjectInfo>();
            foreach (var objectRanges in objectMap)
            {
                foreach (var internalObject in objectRanges.Value)
                {
                    results.Add(new ObjectInfo(internalObject.Name, internalObject.Locked,
                        internalObject.CustomCanReleaseFlag, internalObject.Priority, internalObject.LastUseTime,
                        internalObject.SpawnCount));
                }
            }

            return results.ToArray();
        }

        public override void Update(float elapseSeconds, float realElapseSeconds)
        {
            autoReleaseTime += realElapseSeconds;
            if (autoReleaseTime < autoReleaseInterval)
            {
                return;
            }

            Release();
        }

        public override void Shutdown()
        {
            foreach (var objectInMap in targetObjectMap)
            {
                objectInMap.Value.Release(true);
                ReferenceCache.Release(objectInMap.Value);
            }

            objectMap.Clear();
            targetObjectMap.Clear();
            cachedCanReleaseObjects.Clear();
            cachedToReleaseObjects.Clear();
        }

        private T GetObject(object target)
        {
            if (target == null)
            {
                throw new GameFrameworkException("Target is invalid.");
            }

            T internalObject = null;
            if (targetObjectMap.TryGetValue(target, out internalObject))
            {
                return internalObject;
            }

            return null;
        }

        public bool ReleaseObject(object obj)
        {
            if (obj == null)
            {
                throw new GameFrameworkException("Object is invalid.");
            }

            var internalObject = GetObject(obj);
            if (internalObject == null)
            {
                return false;
            }
            
            if (internalObject.IsInUse || internalObject.Locked || !internalObject.CustomCanReleaseFlag)
            {
                return false;
            }

            objectMap.Remove(internalObject.Name, internalObject);
            targetObjectMap.Remove(internalObject.Target);

            internalObject.Release(false);
            ReferenceCache.Release(internalObject);
            return true;
        }

        private void GetCanReleaseObjects(List<T> results)
        {
            if (results == null)
            {
                throw new GameFrameworkException("Results is invalid.");
            }

            results.Clear();
            foreach (var objectInMap in targetObjectMap)
            {
                var internalObject = objectInMap.Value;
                if (internalObject.IsInUse || internalObject.Locked || !internalObject.CustomCanReleaseFlag)
                {
                    continue;
                }

                results.Add(internalObject);
            }
        }

        private List<T> DefaultReleaseObjectFilterCallback(List<T> candidateObjects, int toReleaseCount, DateTime expireTime)
        {
            cachedToReleaseObjects.Clear();

            if (expireTime > DateTime.MinValue)
            {
                for (int i = candidateObjects.Count - 1; i >= 0; i--)
                {
                    if (candidateObjects[i].LastUseTime <= expireTime)
                    {
                        cachedToReleaseObjects.Add(candidateObjects[i]);
                        candidateObjects.RemoveAt(i);
                        continue;
                    }
                }

                toReleaseCount -= cachedToReleaseObjects.Count;
            }

            for (int i = 0; toReleaseCount > 0 && i < candidateObjects.Count; i++)
            {
                for (int j = i + 1; j < candidateObjects.Count; j++)
                {
                    if (candidateObjects[i].Priority > candidateObjects[j].Priority
                        || candidateObjects[i].Priority == candidateObjects[j].Priority &&
                        candidateObjects[i].LastUseTime > candidateObjects[j].LastUseTime)
                    {
                        T temp = candidateObjects[i];
                        candidateObjects[i] = candidateObjects[j];
                        candidateObjects[j] = temp;
                    }
                }

                cachedToReleaseObjects.Add(candidateObjects[i]);
                toReleaseCount--;
            }

            return cachedToReleaseObjects;
        }
    }
}