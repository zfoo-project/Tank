using System;
using System.Collections.Generic;
using Summer.Base.Model;
using Spring.Core;
using Spring.Logger;
using Spring.Util;

namespace Summer.ObjectPool
{
    /// <summary>
    /// 对象池管理器。
    /// </summary>
    [Bean]
    public sealed class ObjectPoolManager : AbstractManager, IObjectPoolManager
    {
        public const int DefaultCapacity = int.MaxValue;
        public const float DefaultAutoReleaseInterval = float.MaxValue;
        public const float DefaultExpireTime = float.MaxValue;
        public const int DefaultPriority = 0;

        private readonly Dictionary<Type, ObjectPoolBase> objectPools = new Dictionary<Type, ObjectPoolBase>();
        private readonly List<ObjectPoolBase> cachedAllObjectPools= new List<ObjectPoolBase>();


        /// <summary>
        /// 获取游戏框架模块优先级。
        /// </summary>
        /// <remarks>优先级较高的模块会优先轮询，并且关闭操作会后进行。</remarks>
        public override int Priority
        {
            get { return 90; }
        }

        /// <summary>
        /// 获取对象池数量。
        /// </summary>
        public int Count
        {
            get { return objectPools.Count; }
        }

        /// <summary>
        /// 对象池管理器轮询。
        /// </summary>
        /// <param name="elapseSeconds">逻辑流逝时间，以秒为单位。</param>
        /// <param name="realElapseSeconds">真实流逝时间，以秒为单位。</param>
        public override void Update(float elapseSeconds, float realElapseSeconds)
        {
            foreach (var objectPool in objectPools)
            {
                objectPool.Value.Update(elapseSeconds, realElapseSeconds);
            }
        }

        /// <summary>
        /// 关闭并清理对象池管理器。
        /// </summary>
        public override void Shutdown()
        {
            foreach (var objectPool in objectPools)
            {
                objectPool.Value.Shutdown();
            }

            objectPools.Clear();
            cachedAllObjectPools.Clear();
        }

        /// <summary>
        /// 检查是否存在对象池。
        /// </summary>
        /// <typeparam name="T">对象类型。</typeparam>
        /// <returns>是否存在对象池。</returns>
        public bool HasObjectPool<T>() where T : ObjectBase
        {
            return objectPools.ContainsKey(typeof(T));
        }

        /// <summary>
        /// 检查是否存在对象池。
        /// </summary>
        /// <param name="objectType">对象类型。</param>
        /// <returns>是否存在对象池。</returns>
        public bool HasObjectPool(Type objectType)
        {
            if (objectType == null)
            {
                throw new GameFrameworkException("Object type is invalid.");
            }

            if (!typeof(ObjectBase).IsAssignableFrom(objectType))
            {
                throw new GameFrameworkException(StringUtils.Format("Object type '{}' is invalid.",
                    objectType.FullName));
            }

            return objectPools.ContainsKey(objectType);
        }

        /// <summary>
        /// 检查是否存在对象池。
        /// </summary>
        /// <param name="condition">要检查的条件。</param>
        /// <returns>是否存在对象池。</returns>
        public bool HasObjectPool(Predicate<ObjectPoolBase> condition)
        {
            if (condition == null)
            {
                throw new GameFrameworkException("Condition is invalid.");
            }

            foreach (var objectPool in objectPools)
            {
                if (condition(objectPool.Value))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// 获取对象池。
        /// </summary>
        /// <typeparam name="T">对象类型。</typeparam>
        /// <returns>要获取的对象池。</returns>
        public IObjectPool<T> GetObjectPool<T>() where T : ObjectBase
        {
            return (IObjectPool<T>) InternalGetObjectPool(typeof(T));
        }

        /// <summary>
        /// 获取对象池。
        /// </summary>
        /// <param name="objectType">对象类型。</param>
        /// <returns>要获取的对象池。</returns>
        public ObjectPoolBase GetObjectPool(Type objectType)
        {
            if (objectType == null)
            {
                throw new GameFrameworkException("Object type is invalid.");
            }

            if (!typeof(ObjectBase).IsAssignableFrom(objectType))
            {
                throw new GameFrameworkException(StringUtils.Format("Object type '{}' is invalid.",
                    objectType.FullName));
            }

            return InternalGetObjectPool(objectType);
        }



        /// <summary>
        /// 获取所有对象池。
        /// </summary>
        /// <returns>所有对象池。</returns>
        public ObjectPoolBase[] GetAllObjectPools()
        {
            return GetAllObjectPools(false);
        }

        /// <summary>
        /// 获取所有对象池。
        /// </summary>
        /// <param name="results">所有对象池。</param>
        public void GetAllObjectPools(List<ObjectPoolBase> results)
        {
            GetAllObjectPools(false, results);
        }

        /// <summary>
        /// 获取所有对象池。
        /// </summary>
        /// <param name="sort">是否根据对象池的优先级排序。</param>
        /// <returns>所有对象池。</returns>
        public ObjectPoolBase[] GetAllObjectPools(bool sort)
        {
            if (sort)
            {
                List<ObjectPoolBase> results = new List<ObjectPoolBase>();
                foreach (var objectPool in objectPools)
                {
                    results.Add(objectPool.Value);
                }

                results.Sort(ObjectPoolComparer);
                return results.ToArray();
            }
            else
            {
                int index = 0;
                ObjectPoolBase[] results = new ObjectPoolBase[objectPools.Count];
                foreach (var objectPool in objectPools)
                {
                    results[index++] = objectPool.Value;
                }

                return results;
            }
        }

        /// <summary>
        /// 获取所有对象池。
        /// </summary>
        /// <param name="sort">是否根据对象池的优先级排序。</param>
        /// <param name="results">所有对象池。</param>
        public void GetAllObjectPools(bool sort, List<ObjectPoolBase> results)
        {
            if (results == null)
            {
                throw new GameFrameworkException("Results is invalid.");
            }

            results.Clear();
            foreach (var objectPool in objectPools)
            {
                results.Add(objectPool.Value);
            }

            if (sort)
            {
                results.Sort(ObjectPoolComparer);
            }
        }


        /// <summary>
        /// 创建允许单次获取的对象池。
        /// </summary>
        /// <param name="objectType">对象类型。</param>
        /// <returns>要创建的允许单次获取的对象池。</returns>
        public ObjectPoolBase CreateSingleSpawnObjectPool(Type objectType)
        {
            return CreateObjectPool(objectType, false, DefaultAutoReleaseInterval, DefaultCapacity, DefaultExpireTime,
                DefaultPriority);
        }

        public IObjectPool<T> CreateSingleSpawnObjectPool<T>() where T : ObjectBase
        {
            return (IObjectPool<T>) CreateObjectPool(typeof(T), false, DefaultAutoReleaseInterval, DefaultCapacity,
                DefaultExpireTime, DefaultPriority);
        }

        /// <summary>
        /// 创建允许多次获取的对象池。
        /// </summary>
        /// <typeparam name="T">对象类型。</typeparam>
        /// <returns>要创建的允许多次获取的对象池。</returns>
        public IObjectPool<T> CreateMultiSpawnObjectPool<T>() where T : ObjectBase
        {
            return (IObjectPool<T>) CreateObjectPool(typeof(T), true, DefaultAutoReleaseInterval, DefaultCapacity,
                DefaultExpireTime, DefaultPriority);
        }

        /// <summary>
        /// 创建允许多次获取的对象池。
        /// </summary>
        /// <param name="objectType">对象类型。</param>
        /// <returns>要创建的允许多次获取的对象池。</returns>
        public ObjectPoolBase CreateMultiSpawnObjectPool(Type objectType)
        {
            return CreateObjectPool(objectType, true, DefaultAutoReleaseInterval, DefaultCapacity, DefaultExpireTime,
                DefaultPriority);
        }

        public IObjectPool<T> CreateObjectPool<T>(bool allowMultiSpawn, float autoReleaseInterval, int capacity,
            float expireTime,
            int priority) where T : ObjectBase
        {
            return (IObjectPool<T>) CreateObjectPool(typeof(T), allowMultiSpawn, autoReleaseInterval, capacity,
                expireTime, priority);
        }

        public ObjectPoolBase CreateObjectPool(Type objectType, bool allowMultiSpawn, float autoReleaseInterval,
            int capacity, float expireTime, int priority)
        {
            if (objectType == null)
            {
                throw new GameFrameworkException("Object type is invalid.");
            }

            if (!typeof(ObjectBase).IsAssignableFrom(objectType))
            {
                throw new GameFrameworkException(StringUtils.Format("Object type '{}' is invalid.",
                    objectType.FullName));
            }

            ObjectPoolBase objectPool = InternalGetObjectPool(objectType);
            if (objectPool != null)
            {
                return objectPool;
            }

            Type objectPoolType = typeof(ObjectPool<>).MakeGenericType(objectType);
            objectPool = (ObjectPoolBase) Activator.CreateInstance(objectPoolType, allowMultiSpawn, autoReleaseInterval,
                capacity, expireTime, priority);
            objectPools.Add(objectType, objectPool);
            return objectPool;
        }

        /// <summary>
        /// 释放对象池中的可释放对象。
        /// </summary>
        public void Release()
        {
            Log.Info("Object pool release...");
            GetAllObjectPools(true, cachedAllObjectPools);
            foreach (ObjectPoolBase objectPool in cachedAllObjectPools)
            {
                objectPool.Release();
            }
        }

        /// <summary>
        /// 释放对象池中的所有未使用对象。
        /// </summary>
        public void ReleaseAllUnused()
        {
            Log.Info("Object pool release all unused...");
            GetAllObjectPools(true, cachedAllObjectPools);
            foreach (ObjectPoolBase objectPool in cachedAllObjectPools)
            {
                objectPool.ReleaseAllUnused();
            }
        }


        private ObjectPoolBase InternalGetObjectPool(Type type)
        {
            ObjectPoolBase objectPool = null;
            if (objectPools.TryGetValue(type, out objectPool))
            {
                return objectPool;
            }

            return null;
        }

        private int ObjectPoolComparer(ObjectPoolBase a, ObjectPoolBase b)
        {
            return a.Priority.CompareTo(b.Priority);
        }
    }
}