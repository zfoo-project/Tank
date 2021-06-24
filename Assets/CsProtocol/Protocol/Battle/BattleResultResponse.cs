using System;
using System.Collections.Generic;
using CsProtocol.Buffer;

namespace CsProtocol
{
    // @author jaysunxiao
    // @version 3.0
    public class BattleResultResponse : IPacket
    {
        public int score;

        public static BattleResultResponse ValueOf(int score)
        {
            var packet = new BattleResultResponse();
            packet.score = score;
            return packet;
        }


        public short ProtocolId()
        {
            return 1007;
        }
    }


    public class BattleResultResponseRegistration : IProtocolRegistration
    {
        public short ProtocolId()
        {
            return 1007;
        }

        public void Write(ByteBuffer buffer, IPacket packet)
        {
            if (packet == null)
            {
                buffer.WriteBool(false);
                return;
            }
            buffer.WriteBool(true);
            BattleResultResponse message = (BattleResultResponse) packet;
            buffer.WriteInt(message.score);
        }

        public IPacket Read(ByteBuffer buffer)
        {
            if (!buffer.ReadBool())
            {
                return null;
            }
            BattleResultResponse packet = new BattleResultResponse();
            int result0 = buffer.ReadInt();
            packet.score = result0;
            return packet;
        }
    }
}
