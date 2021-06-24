using System.Reflection;
using CsProtocol.Buffer;

namespace Summer.Net.Dispatcher
{
    public class PacketReceiverDefinition : IPacketReceiver
    {
        private object bean;

        // 被PacketReceiver注解的方法
        private MethodInfo method;

        public void Invoke(IPacket packet)
        {
            method.Invoke(bean, new object[] {packet});
        }


        public static PacketReceiverDefinition ValueOf(object bean, MethodInfo method)
        {
            var definition = new PacketReceiverDefinition();
            definition.bean = bean;
            definition.method = method;
            return definition;
        }
    }
}