using System;
using System.Collections.Generic;
using CsProtocol.Buffer;

namespace CsProtocol
{
    public class TripleString : IPacket
    {
        public string left;
        public string middle;
        public string right;

        public static TripleString ValueOf(string left, string middle, string right)
        {
            var packet = new TripleString();
            packet.left = left;
            packet.middle = middle;
            packet.right = right;
            return packet;
        }


        public short ProtocolId()
        {
            return 115;
        }
    }


    public class TripleStringRegistration : IProtocolRegistration
    {
        public short ProtocolId()
        {
            return 115;
        }

        public void Write(ByteBuffer buffer, IPacket packet)
        {
            if (packet == null)
            {
                buffer.WriteBool(false);
                return;
            }
            buffer.WriteBool(true);
            TripleString message = (TripleString) packet;
            buffer.WriteString(message.left);
            buffer.WriteString(message.middle);
            buffer.WriteString(message.right);
        }

        public IPacket Read(ByteBuffer buffer)
        {
            if (!buffer.ReadBool())
            {
                return null;
            }
            TripleString packet = new TripleString();
            string result0 = buffer.ReadString();
            packet.left = result0;
            string result1 = buffer.ReadString();
            packet.middle = result1;
            string result2 = buffer.ReadString();
            packet.right = result2;
            return packet;
        }
    }
}
