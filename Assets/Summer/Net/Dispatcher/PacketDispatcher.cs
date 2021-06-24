using System;
using System.Collections.Generic;
using CsProtocol;
using CsProtocol.Buffer;
using Spring.Core;
using Spring.Logger;
using Spring.Util;
using Spring.Util.Json;

namespace Summer.Net.Dispatcher
{
    public abstract class PacketDispatcher
    {
        private static readonly Dictionary<Type, IPacketReceiver> receiverMap = new Dictionary<Type, IPacketReceiver>();

        public static void Scan()
        {
            ProtocolManager.InitProtocol();
            
            var allBeans = SpringContext.GetAllBeans();
            foreach (var bean in allBeans)
            {
                RegisterPacketReceiver(bean);
            }
        }

        private static void RegisterPacketReceiver(object bean)
        {
            var clazz = bean.GetType();
            var methods = AssemblyUtils.GetMethodsByAnnoInPOJOClass(clazz, typeof(PacketReceiver));
            foreach (var method in methods)
            {
                var parameters = method.GetParameters();
                if (parameters.Length != 1)
                {
                    throw new Exception(StringUtils.Format("[class:{}] [method:{}] must have one parameter!",
                        bean.GetType().Name, method.Name));
                }

                if (!typeof(IPacket).IsAssignableFrom(parameters[0].ParameterType))
                {
                    throw new Exception(StringUtils.Format("[class:{}] [method:{}] must have one [IPacket] type parameter!",
                        bean.GetType().Name, method.Name));
                }

                var paramType = method.GetParameters()[0].ParameterType;
                var expectedMethodName = StringUtils.Format("At{}", paramType.Name);
                if (!method.Name.Equals(expectedMethodName))
                {
                    throw new Exception(StringUtils.Format("[class:{}] [method:{}] [event:{}] expects '{}' as method name!"
                        , bean.GetType().FullName, method.Name, paramType.Name, expectedMethodName));
                }
                
                var receiverDefinition = PacketReceiverDefinition.ValueOf(bean, method);
                if (receiverMap.ContainsKey(paramType))
                {
                    throw new Exception(StringUtils.Format("消息接收器重复[{}][{}]", bean.GetType().Name, paramType.Name));
                }
                receiverMap.Add(paramType, receiverDefinition);
            }
        }

        public static void Send(IPacket packet)
        {
            SpringContext.GetBean<INetManager>().Send(packet);
        }

        public static void Receive(IPacket packet)
        {
            if (packet == null)
            {
                return;
            }

            IPacketReceiver packetReceiver;
            receiverMap.TryGetValue(packet.GetType(), out packetReceiver);

            if (packetReceiver == null)
            {
                Log.Error("没有可以接收消息[{}]的接收器[{}]", packet.GetType().FullName, JsonUtils.object2String(packet));
                return;
            }

            packetReceiver.Invoke(packet);
        }
    }
}