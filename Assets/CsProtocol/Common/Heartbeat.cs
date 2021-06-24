using System;
using System.Collections.Generic;
using CsProtocol.Buffer;

namespace CsProtocol
{
    public class Heartbeat : IPacket
    {

        public static Heartbeat ValueOf()
        {
            var packet = new Heartbeat();
            return packet;
        }


        public short ProtocolId()
        {
            return 102;
        }
    }


    public class HeartbeatRegistration : IProtocolRegistration
    {
        public short ProtocolId()
        {
            return 102;
        }

        public void Write(ByteBuffer buffer, IPacket packet)
        {
            if (packet == null)
            {
                buffer.WriteBool(false);
                return;
            }
            buffer.WriteBool(true);
            Heartbeat message = (Heartbeat) packet;
        }

        public IPacket Read(ByteBuffer buffer)
        {
            if (!buffer.ReadBool())
            {
                return null;
            }
            Heartbeat packet = new Heartbeat();
            return packet;
        }
    }
}
