using System;
using System.Collections.Generic;
using Spring.Storage.Model.Vo;

namespace Spring.Storage.Manager
{
    public interface IStorageManager
    {
        /**
         * 配置表初始化之前，先读取所有的excel
         */
        void InitBefore(Dictionary<Type, List<object>> resourceListMap);

        void InitAfter();

        IStorage GetStorage(Type type);

        Storage<K, V> GetStorage<K, V>();
    }
}