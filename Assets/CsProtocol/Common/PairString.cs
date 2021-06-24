using System;
using System.Collections.Generic;
using CsProtocol.Buffer;

namespace CsProtocol
{
    public class PairString : IPacket
    {
        public string key;
        public string value;

        public static PairString ValueOf(string key, string value)
        {
            var packet = new PairString();
            packet.key = key;
            packet.value = value;
            return packet;
        }


        public short ProtocolId()
        {
            return 112;
        }
    }


    public class PairStringRegistration : IProtocolRegistration
    {
        public short ProtocolId()
        {
            return 112;
        }

        public void Write(ByteBuffer buffer, IPacket packet)
        {
            if (packet == null)
            {
                buffer.WriteBool(false);
                return;
            }
            buffer.WriteBool(true);
            PairString message = (PairString) packet;
            buffer.WriteString(message.key);
            buffer.WriteString(message.value);
        }

        public IPacket Read(ByteBuffer buffer)
        {
            if (!buffer.ReadBool())
            {
                return null;
            }
            PairString packet = new PairString();
            string result0 = buffer.ReadString();
            packet.key = result0;
            string result1 = buffer.ReadString();
            packet.value = result1;
            return packet;
        }
    }
}
