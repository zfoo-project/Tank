using System;

namespace Summer.ObjectPool
{
    /// <summary>
    /// 对象池管理器。
    /// </summary>
    public interface IObjectPoolManager
    {
        /// <summary>
        /// 获取对象池数量。
        /// </summary>
        int Count { get; }


        /// <summary>
        /// 获取所有对象池。
        /// </summary>
        /// <param name="sort">是否根据对象池的优先级排序。</param>
        /// <returns>所有对象池。</returns>
        ObjectPoolBase[] GetAllObjectPools(bool sort);

        ObjectPoolBase CreateSingleSpawnObjectPool(Type objectType);
        IObjectPool<T> CreateSingleSpawnObjectPool<T>() where T : ObjectBase;
        IObjectPool<T> CreateMultiSpawnObjectPool<T>() where T : ObjectBase;
        ObjectPoolBase CreateMultiSpawnObjectPool(Type objectType);

        IObjectPool<T> CreateObjectPool<T>(bool allowMultiSpawn, float autoReleaseInterval, int capacity, float expireTime, int priority) where T : ObjectBase;

        ObjectPoolBase CreateObjectPool(Type objectType, bool allowMultiSpawn, float autoReleaseInterval, int capacity, float expireTime, int priority);

        /// <summary>
        /// 释放对象池中的可释放对象。
        /// </summary>
        void Release();

        /// <summary>
        /// 释放对象池中的所有未使用对象。
        /// </summary>
        void ReleaseAllUnused();
    }
}