using System;
using System.Collections.Generic;
using CsProtocol.Buffer;

namespace CsProtocol
{
    public class Ping : IPacket
    {

        public static Ping ValueOf()
        {
            var packet = new Ping();
            return packet;
        }


        public short ProtocolId()
        {
            return 103;
        }
    }


    public class PingRegistration : IProtocolRegistration
    {
        public short ProtocolId()
        {
            return 103;
        }

        public void Write(ByteBuffer buffer, IPacket packet)
        {
            if (packet == null)
            {
                buffer.WriteBool(false);
                return;
            }
            buffer.WriteBool(true);
            Ping message = (Ping) packet;
        }

        public IPacket Read(ByteBuffer buffer)
        {
            if (!buffer.ReadBool())
            {
                return null;
            }
            Ping packet = new Ping();
            return packet;
        }
    }
}
