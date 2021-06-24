using System;
using System.Collections.Generic;
using CsProtocol.Buffer;

namespace CsProtocol
{
    // @author jaysunxiao
    // @version 1.0
    // @since 2019-10-28 09:58
    public class PlayerInfo : IPacket
    {
        public string avatar;
        public long exp;
        public long id;
        public int level;
        public string name;

        public static PlayerInfo ValueOf(string avatar, long exp, long id, int level, string name)
        {
            var packet = new PlayerInfo();
            packet.avatar = avatar;
            packet.exp = exp;
            packet.id = id;
            packet.level = level;
            packet.name = name;
            return packet;
        }


        public short ProtocolId()
        {
            return 400;
        }
    }


    public class PlayerInfoRegistration : IProtocolRegistration
    {
        public short ProtocolId()
        {
            return 400;
        }

        public void Write(ByteBuffer buffer, IPacket packet)
        {
            if (packet == null)
            {
                buffer.WriteBool(false);
                return;
            }
            buffer.WriteBool(true);
            PlayerInfo message = (PlayerInfo) packet;
            buffer.WriteString(message.avatar);
            buffer.WriteLong(message.exp);
            buffer.WriteLong(message.id);
            buffer.WriteInt(message.level);
            buffer.WriteString(message.name);
        }

        public IPacket Read(ByteBuffer buffer)
        {
            if (!buffer.ReadBool())
            {
                return null;
            }
            PlayerInfo packet = new PlayerInfo();
            string result0 = buffer.ReadString();
            packet.avatar = result0;
            long result1 = buffer.ReadLong();
            packet.exp = result1;
            long result2 = buffer.ReadLong();
            packet.id = result2;
            int result3 = buffer.ReadInt();
            packet.level = result3;
            string result4 = buffer.ReadString();
            packet.name = result4;
            return packet;
        }
    }
}
