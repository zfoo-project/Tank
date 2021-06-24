using System;
using System.Collections.Generic;
using CsProtocol.Buffer;

namespace CsProtocol
{
    public class TripleLSS : IPacket
    {
        public long left;
        public string middle;
        public string right;

        public static TripleLSS ValueOf(long left, string middle, string right)
        {
            var packet = new TripleLSS();
            packet.left = left;
            packet.middle = middle;
            packet.right = right;
            return packet;
        }


        public short ProtocolId()
        {
            return 116;
        }
    }


    public class TripleLSSRegistration : IProtocolRegistration
    {
        public short ProtocolId()
        {
            return 116;
        }

        public void Write(ByteBuffer buffer, IPacket packet)
        {
            if (packet == null)
            {
                buffer.WriteBool(false);
                return;
            }
            buffer.WriteBool(true);
            TripleLSS message = (TripleLSS) packet;
            buffer.WriteLong(message.left);
            buffer.WriteString(message.middle);
            buffer.WriteString(message.right);
        }

        public IPacket Read(ByteBuffer buffer)
        {
            if (!buffer.ReadBool())
            {
                return null;
            }
            TripleLSS packet = new TripleLSS();
            long result0 = buffer.ReadLong();
            packet.left = result0;
            string result1 = buffer.ReadString();
            packet.middle = result1;
            string result2 = buffer.ReadString();
            packet.right = result2;
            return packet;
        }
    }
}
