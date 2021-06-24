#if UNITY_WEBGL && !UNITY_EDITOR
using System;
using Spring.Event;

using CsProtocol;
using CsProtocol.Buffer;
using Summer.Net.Core.Model;

namespace Summer.Net.Core.Websocket
{
    public class WebsocketClient : AbstractClient
    {
        private string url;

        public WebsocketClient(string url)
        {
            this.url = url;
        }

        internal void HandleOnOpen()
        {
            EventBus.AsyncSubmit(NetOpenEvent.ValueOf());
        }

        internal void HandleOnMessage(byte[] content)
        {
            var byteBuffer = ByteBuffer.ValueOf();
            byteBuffer.WriteBytes(content);
            byteBuffer.ReadRawInt();
            var packet = ProtocolManager.Read(byteBuffer);
            // queue it
            receiveQueue.Enqueue(new Message(MessageType.Data, packet));
        }

        internal void HandleOnClose()
        {
            EventBus.AsyncSubmit(NetCloseEvent.ValueOf());
        }

        internal void HandleOnError()
        {
            EventBus.AsyncSubmit(NetErrorEvent.ValueOf());
        }


        public override void Start()
        {
            if (!WebSocketBridge.initialized)
            {
                WebSocketBridge.Initialize();
            }
            WebSocketBridge.WebSocketClose();
            WebSocketBridge.WebSocketConnect(url);
            WebSocketBridge.websocketClient = this;
        }

        public override bool Connected()
        {
            throw new NotImplementedException();
        }

        public override void Close()
        {
            WebSocketBridge.WebSocketClose();
        }

        public override string ToConnectUrl()
        {
            return url;
        }

        public override bool Send(byte[] data)
        {
            WebSocketBridge.WebSocketSend(data, data.Length);
            return true;
        }

        protected override void SendMessagesBlocking(byte[] messages, int offset, int size)
        {
            throw new NotImplementedException();
        }

        protected override bool ReadMessageBlocking(out byte[] content)
        {
            throw new NotImplementedException();
        }
    }
}
#endif