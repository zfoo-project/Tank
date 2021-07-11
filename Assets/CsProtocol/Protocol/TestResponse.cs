using System;
using System.Collections.Generic;
using CsProtocol.Buffer;

namespace CsProtocol
{
    // @author jaysunxiao
    // @version 3.0
    public class TestResponse : IPacket
    {
        public string message;

        public static TestResponse ValueOf(string message)
        {
            var packet = new TestResponse();
            packet.message = message;
            return packet;
        }


        public short ProtocolId()
        {
            return 1301;
        }
    }


    public class TestResponseRegistration : IProtocolRegistration
    {
        public short ProtocolId()
        {
            return 1301;
        }

        public void Write(ByteBuffer buffer, IPacket packet)
        {
            if (packet == null)
            {
                buffer.WriteBool(false);
                return;
            }
            buffer.WriteBool(true);
            TestResponse message = (TestResponse) packet;
            buffer.WriteString(message.message);
        }

        public IPacket Read(ByteBuffer buffer)
        {
            if (!buffer.ReadBool())
            {
                return null;
            }
            TestResponse packet = new TestResponse();
            string result0 = buffer.ReadString();
            packet.message = result0;
            return packet;
        }
    }
}
