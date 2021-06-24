using System;
using System.Collections.Generic;
using Spring.Core;
using Spring.Storage.Model.Anno;
using Spring.Storage.Model.Vo;
using Spring.Util;

namespace Spring.Storage.Manager
{
    [Bean]
    public class StorageManager : IStorageManager
    {
        private Dictionary<Type, IStorage> storageMap = new Dictionary<Type, IStorage>();


        public void InitBefore(Dictionary<Type, List<object>> resourceListMap)
        {
            if (resourceListMap == null || resourceListMap.Count <= 0)
            {
                throw new Exception("静态资源的信息定义不存在，可能是配置缺少");
            }

            foreach (var definition in resourceListMap)
            {
                try
                {
                    var resourceType = definition.Key;
                    var resourceList = definition.Value;
                    var fieldInfos = AssemblyUtils.GetFieldsByAnnoInPOJOClass(resourceType, typeof(Id));
                    if (fieldInfos == null || fieldInfos.Length <= 0)
                    {
                        throw new Exception(StringUtils.Format("资源类型[{}]需要包含被[Id]注解标注的主键", resourceType.Name));
                    }

                    if (fieldInfos.Length > 1)
                    {
                        throw new Exception(StringUtils.Format("资源类型[{}]的[Id]注解标注的主键只能包含一个", resourceType.Name));
                    }

                    Type storageType = typeof(Storage<,>).MakeGenericType(fieldInfos[0].FieldType, resourceType);
                    var storage = (IStorage) Activator.CreateInstance(storageType);
                    storage.Init(resourceType, resourceList);
                    storageMap[resourceType] = storage;
                }
                catch (Exception e)
                {
                    throw new Exception(StringUtils.Format("配置表[{}]读取错误", definition.Key.Name), e);
                }
            }
        }

        public void InitAfter()
        {
            // 通过ResInjection注入
            var allComponents = SpringContext.GetAllBeans();
            foreach (var component in allComponents)
            {
                // @ResInjection
                // Storage<Integer, ActivityResource> resources;
                var fieldInfos = AssemblyUtils.GetFieldsByAnnoInPOJOClass(component.GetType(), typeof(ResInjection));
                if (fieldInfos.Length <= 0)
                {
                    continue;
                }

                foreach (var fieldInfo in fieldInfos)
                {
                    var genericTypes = fieldInfo.FieldType.GenericTypeArguments;
                    var keyType = genericTypes[0];
                    var resourceType = genericTypes[1];

                    var storage = storageMap[resourceType];

                    if (storage == null)
                    {
                        throw new Exception(StringUtils.Format("静态类资源[resource:{}]不存在", resourceType.Name));
                    }

                    var idFields = AssemblyUtils.GetFieldsByAnnoInPOJOClass(resourceType, typeof(Id));
                    if (idFields.Length != 1)
                    {
                        throw new Exception(StringUtils.Format("静态类资源[resource:{}]配置没有注解id", resourceType.Name));
                    }

                    if (keyType != idFields[0].FieldType)
                    {
                        throw new Exception(StringUtils.Format("静态类资源[resource:{}]配置注解[id:{}]类型和泛型类型[type:{}]不匹配"
                            , resourceType.Name, idFields[0].FieldType.Name, keyType.Name));
                    }

                    AssemblyUtils.SetField(fieldInfo, component, storage);
                }
            }
        }


        public IStorage GetStorage(Type type)
        {
            return storageMap[type];
        }

        public Storage<K, V> GetStorage<K, V>()
        {
            return (Storage<K, V>) storageMap[typeof(V)];
        }
    }
}