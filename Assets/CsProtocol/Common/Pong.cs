using System;
using System.Collections.Generic;
using CsProtocol.Buffer;

namespace CsProtocol
{
    public class Pong : IPacket
    {
        public long time;

        public static Pong ValueOf(long time)
        {
            var packet = new Pong();
            packet.time = time;
            return packet;
        }


        public short ProtocolId()
        {
            return 104;
        }
    }


    public class PongRegistration : IProtocolRegistration
    {
        public short ProtocolId()
        {
            return 104;
        }

        public void Write(ByteBuffer buffer, IPacket packet)
        {
            if (packet == null)
            {
                buffer.WriteBool(false);
                return;
            }
            buffer.WriteBool(true);
            Pong message = (Pong) packet;
            buffer.WriteLong(message.time);
        }

        public IPacket Read(ByteBuffer buffer)
        {
            if (!buffer.ReadBool())
            {
                return null;
            }
            Pong packet = new Pong();
            long result0 = buffer.ReadLong();
            packet.time = result0;
            return packet;
        }
    }
}
