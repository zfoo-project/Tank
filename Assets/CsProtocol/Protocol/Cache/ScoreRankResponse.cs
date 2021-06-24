using System;
using System.Collections.Generic;
using CsProtocol.Buffer;

namespace CsProtocol
{
    // @author jaysunxiao
    // @version 3.0
    public class ScoreRankResponse : IPacket
    {
        // 排名，按list的先后顺序
        public List<RankInfo> ranks;

        public static ScoreRankResponse ValueOf(List<RankInfo> ranks)
        {
            var packet = new ScoreRankResponse();
            packet.ranks = ranks;
            return packet;
        }


        public short ProtocolId()
        {
            return 3003;
        }
    }


    public class ScoreRankResponseRegistration : IProtocolRegistration
    {
        public short ProtocolId()
        {
            return 3003;
        }

        public void Write(ByteBuffer buffer, IPacket packet)
        {
            if (packet == null)
            {
                buffer.WriteBool(false);
                return;
            }
            buffer.WriteBool(true);
            ScoreRankResponse message = (ScoreRankResponse) packet;
            if (message.ranks == null)
            {
                buffer.WriteInt(0);
            }
            else
            {
                buffer.WriteInt(message.ranks.Count);
                int length0 = message.ranks.Count;
                for (int i1 = 0; i1 < length0; i1++)
                {
                    var element2 = message.ranks[i1];
                    ProtocolManager.GetProtocol(402).Write(buffer, element2);
                }
            }
        }

        public IPacket Read(ByteBuffer buffer)
        {
            if (!buffer.ReadBool())
            {
                return null;
            }
            ScoreRankResponse packet = new ScoreRankResponse();
            int size5 = buffer.ReadInt();
            var result3 = new List<RankInfo>(size5);
            if (size5 > 0)
            {
                for (int index4 = 0; index4 < size5; index4++)
                {
                    RankInfo result6 = (RankInfo) ProtocolManager.GetProtocol(402).Read(buffer);
                    result3.Add(result6);
                }
            }
            packet.ranks = result3;
            return packet;
        }
    }
}
