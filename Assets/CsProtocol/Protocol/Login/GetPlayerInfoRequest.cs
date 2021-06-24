using System;
using System.Collections.Generic;
using CsProtocol.Buffer;

namespace CsProtocol
{
    // @author jaysunxiao
    // @version 1.0
    // @since 2019-10-15 17:55
    public class GetPlayerInfoRequest : IPacket
    {
        public string token;

        public static GetPlayerInfoRequest ValueOf(string token)
        {
            var packet = new GetPlayerInfoRequest();
            packet.token = token;
            return packet;
        }


        public short ProtocolId()
        {
            return 1004;
        }
    }


    public class GetPlayerInfoRequestRegistration : IProtocolRegistration
    {
        public short ProtocolId()
        {
            return 1004;
        }

        public void Write(ByteBuffer buffer, IPacket packet)
        {
            if (packet == null)
            {
                buffer.WriteBool(false);
                return;
            }
            buffer.WriteBool(true);
            GetPlayerInfoRequest message = (GetPlayerInfoRequest) packet;
            buffer.WriteString(message.token);
        }

        public IPacket Read(ByteBuffer buffer)
        {
            if (!buffer.ReadBool())
            {
                return null;
            }
            GetPlayerInfoRequest packet = new GetPlayerInfoRequest();
            string result0 = buffer.ReadString();
            packet.token = result0;
            return packet;
        }
    }
}
