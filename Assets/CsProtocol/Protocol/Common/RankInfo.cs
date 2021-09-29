using System;
using System.Collections.Generic;
using CsProtocol.Buffer;

namespace CsProtocol
{
    // @author jaysunxiao
    // @version 3.0
    public class RankInfo : IPacket
    {
        public PlayerInfo playerInfo;
        public int score;
        public long time;

        public static RankInfo ValueOf(PlayerInfo playerInfo, int score, long time)
        {
            var packet = new RankInfo();
            packet.playerInfo = playerInfo;
            packet.score = score;
            packet.time = time;
            return packet;
        }


        public short ProtocolId()
        {
            return 402;
        }
    }


    public class RankInfoRegistration : IProtocolRegistration
    {
        public short ProtocolId()
        {
            return 402;
        }

        public void Write(ByteBuffer buffer, IPacket packet)
        {
            if (packet == null)
            {
                buffer.WriteBool(false);
                return;
            }
            buffer.WriteBool(true);
            RankInfo message = (RankInfo) packet;
            ProtocolManager.GetProtocol(400).Write(buffer, message.playerInfo);
            buffer.WriteInt(message.score);
            buffer.WriteLong(message.time);
        }

        public IPacket Read(ByteBuffer buffer)
        {
            if (!buffer.ReadBool())
            {
                return null;
            }
            RankInfo packet = new RankInfo();
            PlayerInfo result0 = (PlayerInfo) ProtocolManager.GetProtocol(400).Read(buffer);
            packet.playerInfo = result0;
            int result1 = buffer.ReadInt();
            packet.score = result1;
            long result2 = buffer.ReadLong();
            packet.time = result2;
            return packet;
        }
    }
}
