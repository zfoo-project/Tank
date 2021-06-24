using CsProtocol;
using CsProtocol.Buffer;
using Spring.Core;
using Spring.Event;
using Spring.Logger;
using Summer.Base.Model;
using Summer.Net.Core;
using Summer.Net.Core.Model;
using Summer.Net.Core.Tcp;
using Summer.Net.Dispatcher;
using UnityEngine;
using Message = Summer.Net.Core.Message;

namespace Summer.Net
{
    [Bean]
    public class NetManager : AbstractManager, INetManager
    {
        public static readonly int PROTOCOL_HEAD_LENGTH = 4;

        private AbstractClient netClient;

        public override void Update(float elapseSeconds, float realElapseSeconds)
        {
            if (netClient == null)
            {
                return;
            }

            Message message;
            while (netClient.GetNextMessage(out message))
            {
                switch (message.messageType)
                {
                    case MessageType.Connected:
                        Log.Info("Connected server [{}]", netClient.ToConnectUrl());
                        EventBus.SyncSubmit(NetOpenEvent.ValueOf());
                        break;
                    case MessageType.Data:
                        // Log.Info("Data: " + JsonUtils.object2String(message.packet));
                        PacketDispatcher.Receive(message.packet);
                        break;
                    case MessageType.Disconnected:
                        Log.Info("Disconnected");
                        EventBus.AsyncSubmit(NetErrorEvent.ValueOf());
                        break;
                }
            }
        }

        public override void Shutdown()
        {
            Close();
        }

        public void Connect(string url)
        {
            Close();

            Log.Info("开始连接服务器[url:{}][platform:{}]", url, Application.platform);
#if UNITY_WEBGL && !UNITY_EDITOR
            netClient = new Summer.Net.Core.Websocket.WebsocketClient(url);
#else
            var hostAndPort = HostAndPort.ValueOf(url);
            netClient = new NetTcpClient(hostAndPort.host, hostAndPort.port);
#endif

            netClient.Start();
        }

        public void Close()
        {
            if (netClient != null)
            {
                netClient.Close();
                netClient = null;
            }

            EventBus.SyncSubmit(NetCloseEvent.ValueOf());
        }

        public void Send(IPacket packet)
        {
            ByteBuffer byteBuffer = null;
            try
            {
                byteBuffer = ByteBuffer.ValueOf();

                byteBuffer.WriteRawInt(PROTOCOL_HEAD_LENGTH);

                ProtocolManager.Write(byteBuffer, packet);
                
                // 包的附加包为空
                byteBuffer.WriteBool(false);

                // 包的长度
                int length = byteBuffer.WriteOffset();

                int packetLength = length - PROTOCOL_HEAD_LENGTH;

                byteBuffer.SetWriteOffset(0);
                byteBuffer.WriteRawInt(packetLength);
                byteBuffer.SetWriteOffset(length);

                var sendSuccess = netClient.Send(byteBuffer.ToBytes());
                if (!sendSuccess)
                {
                    Close();
                    EventBus.AsyncSubmit(NetErrorEvent.ValueOf());
                }
            }
            finally
            {
                if (byteBuffer != null)
                {
                    byteBuffer.Clear();
                }
            }
        }
    }
}