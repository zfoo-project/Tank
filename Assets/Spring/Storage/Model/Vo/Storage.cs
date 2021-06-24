using System;
using System.Collections.Generic;
using NPOI.Util;
using Spring.Util;

namespace Spring.Storage.Model.Vo
{
    public class Storage<K, V> : IStorage
    {
        private Type type;

        private Dictionary<K, V> dataMap = new Dictionary<K, V>();
        private Dictionary<string, Dictionary<object, List<V>>> indexMap = new Dictionary<string, Dictionary<object, List<V>>>();
        private Dictionary<string, Dictionary<object, V>> uniqueIndexMap = new Dictionary<string, Dictionary<object, V>>();

        private IdDef idDef;
        private Dictionary<string, IndexDef> indexDefMap;


        public void Init(Type resourceType, List<object> list)
        {
            type = resourceType;
            idDef = IdDef.ValueOf(resourceType);
            indexDefMap = IndexDef.CreateResourceIndexes(resourceType);

            dataMap.Clear();
            indexMap.Clear();
            uniqueIndexMap.Clear();

            list.ForEach(it => Put((V) it));
        }

        public List<object> GetAll()
        {
            var list = new List<object>();
            foreach (var dataMapValue in dataMap.Values)
            {
                list.Add(dataMapValue);
            }

            return list;
        }

        public Dictionary<K, V> GetDataMap()
        {
            return dataMap;
        }

        public bool Contain(K key)
        {
            return dataMap.ContainsKey(key);
        }

        public V Get(K key)
        {
            return dataMap[key];
        }

        public List<V> GetIndex(string indexName, object key)
        {
            var indexValues = indexMap[indexName];
            var values = indexValues[key];
            if (CollectionUtils.IsEmpty(values))
            {
                return new List<V>(0);
            }

            return values;
        }

        public V GetUniqueIndex(string indexName, Object key)
        {
            var indexValueMap = uniqueIndexMap[indexName];
            var value = indexValueMap[key];
            return value;
        }


        private void Put(V value)
        {
            K key = (K) AssemblyUtils.GetField(idDef.field, value);

            if (key == null)
            {
                throw new Exception("静态资源存在id未配置的项");
            }

            if (dataMap.ContainsKey(key))
            {
                throw new RuntimeException(StringUtils.Format("静态资源[resource:{}]的[id:{}]重复", type.Name, key));
            }

            // 添加资源
            dataMap[key] = value;

            // 添加索引
            foreach (var def in indexDefMap.Values)
            {
                var indexKey = def.key;
                var indexValue = AssemblyUtils.GetField(def.field, value);
                if (def.unique)
                {
                    // 唯一索引
                    Dictionary<object, V> index = null;
                    uniqueIndexMap.TryGetValue(indexKey, out index);
                    if (index == null)
                    {
                        index = new Dictionary<object, V>();
                        uniqueIndexMap[indexKey] = index;
                    }

                    index[indexValue] = value;
                }
                else
                {
                    // 不是唯一索引
                    Dictionary<object, List<V>> index = null;
                    indexMap.TryGetValue(indexKey, out index);
                    if (index == null)
                    {
                        index = new Dictionary<object, List<V>>();
                        indexMap[indexKey] = index;
                    }

                    List<V> list = null;
                    index.TryGetValue(indexValue, out list);
                    if (list == null)
                    {
                        list = new List<V>();
                        index[indexValue] = list;
                    }

                    list.Add(value);
                }
            }
        }

        public int Size()
        {
            return dataMap.Count;
        }
    }
}