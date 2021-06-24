using System;
using System.Collections.Generic;
using CsProtocol.Buffer;

namespace CsProtocol
{
    // @author jaysunxiao
    // @version 1.0
    // @since 2019-10-15 17:55
    public class LogoutRequest : IPacket
    {

        public static LogoutRequest ValueOf()
        {
            var packet = new LogoutRequest();
            return packet;
        }


        public short ProtocolId()
        {
            return 1002;
        }
    }


    public class LogoutRequestRegistration : IProtocolRegistration
    {
        public short ProtocolId()
        {
            return 1002;
        }

        public void Write(ByteBuffer buffer, IPacket packet)
        {
            if (packet == null)
            {
                buffer.WriteBool(false);
                return;
            }
            buffer.WriteBool(true);
            LogoutRequest message = (LogoutRequest) packet;
        }

        public IPacket Read(ByteBuffer buffer)
        {
            if (!buffer.ReadBool())
            {
                return null;
            }
            LogoutRequest packet = new LogoutRequest();
            return packet;
        }
    }
}
