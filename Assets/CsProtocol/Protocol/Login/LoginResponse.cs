using System;
using System.Collections.Generic;
using CsProtocol.Buffer;

namespace CsProtocol
{
    // @author jaysunxiao
    // @version 1.0
    // @since 2019-10-15 17:55
    public class LoginResponse : IPacket
    {
        public string token;

        public static LoginResponse ValueOf(string token)
        {
            var packet = new LoginResponse();
            packet.token = token;
            return packet;
        }


        public short ProtocolId()
        {
            return 1001;
        }
    }


    public class LoginResponseRegistration : IProtocolRegistration
    {
        public short ProtocolId()
        {
            return 1001;
        }

        public void Write(ByteBuffer buffer, IPacket packet)
        {
            if (packet == null)
            {
                buffer.WriteBool(false);
                return;
            }
            buffer.WriteBool(true);
            LoginResponse message = (LoginResponse) packet;
            buffer.WriteString(message.token);
        }

        public IPacket Read(ByteBuffer buffer)
        {
            if (!buffer.ReadBool())
            {
                return null;
            }
            LoginResponse packet = new LoginResponse();
            string result0 = buffer.ReadString();
            packet.token = result0;
            return packet;
        }
    }
}
