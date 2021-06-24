using System;
using System.Collections.Generic;
using CsProtocol.Buffer;

namespace CsProtocol
{
    // @author jaysunxiao
    // @version 1.0
    // @since 2019-10-15 17:55
    public class LogoutResponse : IPacket
    {
        public long sid;
        public long uid;

        public static LogoutResponse ValueOf(long sid, long uid)
        {
            var packet = new LogoutResponse();
            packet.sid = sid;
            packet.uid = uid;
            return packet;
        }


        public short ProtocolId()
        {
            return 1003;
        }
    }


    public class LogoutResponseRegistration : IProtocolRegistration
    {
        public short ProtocolId()
        {
            return 1003;
        }

        public void Write(ByteBuffer buffer, IPacket packet)
        {
            if (packet == null)
            {
                buffer.WriteBool(false);
                return;
            }
            buffer.WriteBool(true);
            LogoutResponse message = (LogoutResponse) packet;
            buffer.WriteLong(message.sid);
            buffer.WriteLong(message.uid);
        }

        public IPacket Read(ByteBuffer buffer)
        {
            if (!buffer.ReadBool())
            {
                return null;
            }
            LogoutResponse packet = new LogoutResponse();
            long result0 = buffer.ReadLong();
            packet.sid = result0;
            long result1 = buffer.ReadLong();
            packet.uid = result1;
            return packet;
        }
    }
}
