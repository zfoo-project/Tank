using System;
using System.Collections.Generic;
using CsProtocol.Buffer;

namespace CsProtocol
{
    // @author jaysunxiao
    // @version 3.0
    public class PlayerExpNotice : IPacket
    {
        public long exp;
        public int level;

        public static PlayerExpNotice ValueOf(long exp, int level)
        {
            var packet = new PlayerExpNotice();
            packet.exp = exp;
            packet.level = level;
            return packet;
        }


        public short ProtocolId()
        {
            return 1101;
        }
    }


    public class PlayerExpNoticeRegistration : IProtocolRegistration
    {
        public short ProtocolId()
        {
            return 1101;
        }

        public void Write(ByteBuffer buffer, IPacket packet)
        {
            if (packet == null)
            {
                buffer.WriteBool(false);
                return;
            }
            buffer.WriteBool(true);
            PlayerExpNotice message = (PlayerExpNotice) packet;
            buffer.WriteLong(message.exp);
            buffer.WriteInt(message.level);
        }

        public IPacket Read(ByteBuffer buffer)
        {
            if (!buffer.ReadBool())
            {
                return null;
            }
            PlayerExpNotice packet = new PlayerExpNotice();
            long result0 = buffer.ReadLong();
            packet.exp = result0;
            int result1 = buffer.ReadInt();
            packet.level = result1;
            return packet;
        }
    }
}
