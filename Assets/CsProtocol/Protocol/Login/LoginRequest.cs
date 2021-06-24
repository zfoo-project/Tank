using System;
using System.Collections.Generic;
using CsProtocol.Buffer;

namespace CsProtocol
{
    // @author jaysunxiao
    // @version 1.0
    // @since 2019-10-15 17:55
    public class LoginRequest : IPacket
    {
        public string account;
        public string password;

        public static LoginRequest ValueOf(string account, string password)
        {
            var packet = new LoginRequest();
            packet.account = account;
            packet.password = password;
            return packet;
        }


        public short ProtocolId()
        {
            return 1000;
        }
    }


    public class LoginRequestRegistration : IProtocolRegistration
    {
        public short ProtocolId()
        {
            return 1000;
        }

        public void Write(ByteBuffer buffer, IPacket packet)
        {
            if (packet == null)
            {
                buffer.WriteBool(false);
                return;
            }
            buffer.WriteBool(true);
            LoginRequest message = (LoginRequest) packet;
            buffer.WriteString(message.account);
            buffer.WriteString(message.password);
        }

        public IPacket Read(ByteBuffer buffer)
        {
            if (!buffer.ReadBool())
            {
                return null;
            }
            LoginRequest packet = new LoginRequest();
            string result0 = buffer.ReadString();
            packet.account = result0;
            string result1 = buffer.ReadString();
            packet.password = result1;
            return packet;
        }
    }
}
