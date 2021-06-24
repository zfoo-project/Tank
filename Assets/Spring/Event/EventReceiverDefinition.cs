using System.Reflection;

namespace Spring.Event
{
    public class EventReceiverDefinition : IEventReceiver
    {
        private object bean;

        // 被ReceiveEvent注解的方法
        private MethodInfo method;

        public void Invoke(IEvent eve)
        {
            method.Invoke(bean, new object[] {eve});
        }

        public static EventReceiverDefinition ValueOf(object bean, MethodInfo method)
        {
            var definition = new EventReceiverDefinition();
            definition.bean = bean;
            definition.method = method;
            return definition;
        }
    }
}