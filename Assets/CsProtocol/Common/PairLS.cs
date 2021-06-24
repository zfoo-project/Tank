using System;
using System.Collections.Generic;
using CsProtocol.Buffer;

namespace CsProtocol
{
    public class PairLS : IPacket
    {
        public long key;
        public string value;

        public static PairLS ValueOf(long key, string value)
        {
            var packet = new PairLS();
            packet.key = key;
            packet.value = value;
            return packet;
        }


        public short ProtocolId()
        {
            return 113;
        }
    }


    public class PairLSRegistration : IProtocolRegistration
    {
        public short ProtocolId()
        {
            return 113;
        }

        public void Write(ByteBuffer buffer, IPacket packet)
        {
            if (packet == null)
            {
                buffer.WriteBool(false);
                return;
            }
            buffer.WriteBool(true);
            PairLS message = (PairLS) packet;
            buffer.WriteLong(message.key);
            buffer.WriteString(message.value);
        }

        public IPacket Read(ByteBuffer buffer)
        {
            if (!buffer.ReadBool())
            {
                return null;
            }
            PairLS packet = new PairLS();
            long result0 = buffer.ReadLong();
            packet.key = result0;
            string result1 = buffer.ReadString();
            packet.value = result1;
            return packet;
        }
    }
}
