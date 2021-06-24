using System;
using System.Collections.Generic;
using System.Linq;
using Spring.Logger;
using Spring.Util;

namespace Spring.Core
{
    public sealed class SpringContext
    {
        private static readonly Dictionary<Type, object> beanMap = new Dictionary<Type, object>();

        private static readonly Dictionary<Type, object> cachedBeanMap = new Dictionary<Type, object>();

        public static readonly List<string> scanPaths = new List<string>();
        
        /**
         * 扫描状态，true标识已经扫描过，false标识没有扫描过
         */
        private static bool scanFlag;

        private SpringContext()
        {
        }

        public static bool GetScanFlag()
        {
            return scanFlag;
        }
        
        public static void AddScanPath(List<string> includePathList)
        {
            scanPaths.AddRange(includePathList);
        }

        public static T RegisterBean<T>()
        {
            return (T) RegisterBean(typeof(T));
        }

        public static void RegisterBean(object bean)
        {
            checkBean(bean);
            var type = bean.GetType();
            beanMap.Add(type, bean);
        }


        public static object RegisterBean(Type type)
        {
            var bean = Activator.CreateInstance(type);
            if (bean == null)
            {
                throw new Exception(StringUtils.Format("无法通过类型[{}]创建实例", type));
            }

            RegisterBean(bean);
            return bean;
        }

        public static void Scan()
        {
            if (scanFlag)
            {
                throw new Exception("SpringContext已经扫描，不需要重复扫描");
            }

            // 实例化所有被Bean注解标注的类
            var types = AssemblyUtils.GetAllClassTypes();
            foreach (var type in types)
            {
                if (type.IsDefined(typeof(Bean), false))
                {
                    if (scanPaths.Any(it => type.FullName.StartsWith(it)))
                    {
                        RegisterBean(type);
                    }
                }
            }

            // 注入被Autowired注解标注的属性
            foreach (var pair in beanMap)
            {
                var type = pair.Key;
                var bean = pair.Value;
                var fields = AssemblyUtils.GetFieldTypes(type);
                foreach (var fieldInfo in fields)
                {
                    if (fieldInfo.IsDefined(typeof(Autowired), false))
                    {
                        var fieldType = fieldInfo.FieldType;
                        try
                        {
                            var autowiredFieldBean = GetBean(fieldType);
                            fieldInfo.SetValue(bean, autowiredFieldBean);
                        }
                        catch (Exception e)
                        {
                            throw new Exception(StringUtils.Format("在类[{}]中注入[{}]类型异常"
                                , bean.GetType().Name, fieldType.Name), e);
                        }
                    }
                }
            }

            // 调用被BeforePostConstruct注解标注的方法
            foreach (var pair in beanMap)
            {
                var type = pair.Key;
                var bean = pair.Value;
                var methods = AssemblyUtils.GetMethodsByAnnoInPOJOClass(type, typeof(BeforePostConstruct));
                foreach (var method in methods)
                {
                    method.Invoke(bean, null);
                }
            }

            // 调用被PostConstruct注解标注的方法
            foreach (var pair in beanMap)
            {
                var type = pair.Key;
                var bean = pair.Value;
                var methods = AssemblyUtils.GetMethodsByAnnoInPOJOClass(type, typeof(PostConstruct));
                foreach (var method in methods)
                {
                    method.Invoke(bean, null);
                }
            }

            // 调用被AfterPostConstruct注解标注的方法
            foreach (var pair in beanMap)
            {
                var type = pair.Key;
                var bean = pair.Value;
                var methods = AssemblyUtils.GetMethodsByAnnoInPOJOClass(type, typeof(AfterPostConstruct));
                foreach (var method in methods)
                {
                    method.Invoke(bean, null);
                }
            }

            scanFlag = true;
        }


        public static T GetBean<T>()
        {
            return (T) GetBean(typeof(T));
        }

        public static List<object> GetBeans(Type type)
        {
            return beanMap
                .Where(it => type.IsAssignableFrom(it.Key))
                .Select(it => it.Value)
                .ToList();
        }

        public static object GetBean(Type type)
        {
            object bean = null;

            // 先从缓存获取
            cachedBeanMap.TryGetValue(type, out bean);
            if (bean != null)
            {
                return bean;
            }

            // 再从全部的组件获取
            beanMap.TryGetValue(type, out bean);
            if (bean != null)
            {
                return bean;
            }

            // 如果是接口类型，没有办法直接获取bean，这里遍历全部的bean
            var findList = beanMap.Keys.Where(it => type.IsAssignableFrom(it)).ToList();
            if (CollectionUtils.IsEmpty(findList))
            {
                throw new Exception(StringUtils.Format("无法找到类型[{}]的实例", type.Name));
            }

            if (findList.Count > 1)
            {
                throw new Exception(StringUtils.Format("类型[{}]存在多个[{}][{}]实例"
                    , type.Name, findList.First().GetType().Name, findList.Skip(1).First().GetType().Name));
            }

            bean = beanMap[findList.First()];

            // 缓存结果
            cachedBeanMap[type] = bean;

            return bean;
        }

        public static List<object> GetAllBeans()
        {
            var list = new List<object>();
            foreach (var pair in beanMap)
            {
                list.Add(pair.Value);
            }

            return list;
        }

        public static void Shutdown()
        {
            scanFlag = false;
            scanPaths.Clear();
            beanMap.Clear();
            cachedBeanMap.Clear();
        }

        public static void checkBean(object bean)
        {
            if (bean == null)
            {
                throw new Exception(StringUtils.Format("实例[{}]为空，无法创建", null));
            }

            var type = bean.GetType();
            if (beanMap.ContainsKey(type))
            {
                throw new Exception(StringUtils.Format("类型[{}]重复创建实例", type.Name));
            }
        }
    }
}