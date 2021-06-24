using System;
using System.Collections.Generic;
using System.Threading;
using Spring.Core;
using Spring.Logger;
using Spring.Util;

namespace Spring.Event
{
    public abstract class EventBus
    {
        private static readonly Dictionary<Type, ICollection<IEventReceiver>> receiverMap = new Dictionary<Type, ICollection<IEventReceiver>>();

        static EventBus()
        {
            ThreadPool.SetMinThreads(2, 2);
            ThreadPool.SetMaxThreads(8, 8);
        }

        private EventBus()
        {
        }


        public static void Scan()
        {
            var allBeans = SpringContext.GetAllBeans();
            foreach (var bean in allBeans)
            {
                RegisterEventReceiver(bean);
            }
        }

        private static void RegisterEventReceiver(object bean)
        {
            var clazz = bean.GetType();
            var methods = AssemblyUtils.GetMethodsByAnnoInPOJOClass(clazz, typeof(EventReceiver));
            foreach (var method in methods)
            {
                var parameters = method.GetParameters();
                if (parameters.Length != 1)
                {
                    throw new Exception(StringUtils.Format("[class:{}] [method:{}] must have one parameter!",
                        bean.GetType().Name, method.Name));
                }

                if (!typeof(IEvent).IsAssignableFrom(parameters[0].ParameterType))
                {
                    throw new Exception(StringUtils.Format("[class:{}] [method:{}] must have one [IEvent] type parameter!",
                        bean.GetType().Name, method.Name));
                }

                var paramType = method.GetParameters()[0].ParameterType;
                var expectedMethodName = StringUtils.Format("On{}", paramType.Name);
                if (!method.Name.Equals(expectedMethodName))
                {
                    throw new Exception(StringUtils.Format("[class:{}] [method:{}] [event:{}] expects '{}' as method name!", bean.GetType().FullName, method.Name, paramType.Name, expectedMethodName));
                }

                var receiverDefinition = EventReceiverDefinition.ValueOf(bean, method);
                if (!receiverMap.ContainsKey(paramType))
                {
                    receiverMap.Add(paramType, new LinkedList<IEventReceiver>());
                }

                receiverMap[paramType].Add(receiverDefinition);
            }
        }

        public static void SyncSubmit(IEvent eve)
        {
            ICollection<IEventReceiver> list;
            receiverMap.TryGetValue(eve.GetType(), out list);

            if (CollectionUtils.IsEmpty(list))
            {
                return;
            }

            DoSubmit(eve, list);
        }

        public static void AsyncSubmit(IEvent eve)
        {
            ICollection<IEventReceiver> list;
            receiverMap.TryGetValue(eve.GetType(), out list);

            if (CollectionUtils.IsEmpty(list))
            {
                return;
            }

            AsyncExecute(() => DoSubmit(eve, list));
        }

        public static void AsyncExecute(Action action)
        {
            if (action == null)
            {
                return;
            }

#if UNITY_WEBGL
            // 如果是webgl则直接执行，因为webgl不支持线程操作
            action();
#else
            ThreadPool.QueueUserWorkItem((param) =>
            {
                try
                {
                    action();
                }
                catch (Exception e)
                {
                    Log.Error(e);
                }
            });
#endif
        }

        private static void DoSubmit(IEvent eve, ICollection<IEventReceiver> listReceiver)
        {
            foreach (var receiver in listReceiver)
            {
                try
                {
                    receiver.Invoke(eve);
                }
                catch (Exception e)
                {
                    Log.Error(e);
                }
            }
        }
    }
}