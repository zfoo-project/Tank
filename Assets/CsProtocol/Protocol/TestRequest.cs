using System;
using System.Collections.Generic;
using CsProtocol.Buffer;

namespace CsProtocol
{
    // @author jaysunxiao
    // @version 3.0
    public class TestRequest : IPacket
    {
        public string message;

        public static TestRequest ValueOf(string message)
        {
            var packet = new TestRequest();
            packet.message = message;
            return packet;
        }


        public short ProtocolId()
        {
            return 1300;
        }
    }


    public class TestRequestRegistration : IProtocolRegistration
    {
        public short ProtocolId()
        {
            return 1300;
        }

        public void Write(ByteBuffer buffer, IPacket packet)
        {
            if (packet == null)
            {
                buffer.WriteBool(false);
                return;
            }
            buffer.WriteBool(true);
            TestRequest message = (TestRequest) packet;
            buffer.WriteString(message.message);
        }

        public IPacket Read(ByteBuffer buffer)
        {
            if (!buffer.ReadBool())
            {
                return null;
            }
            TestRequest packet = new TestRequest();
            string result0 = buffer.ReadString();
            packet.message = result0;
            return packet;
        }
    }
}
