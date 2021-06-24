using System;
using System.Collections.Generic;
using CsProtocol.Buffer;

namespace CsProtocol
{
    // @author jaysunxiao
    // @version 3.0
    public class BattleResultRequest : IPacket
    {
        public int score;

        public static BattleResultRequest ValueOf(int score)
        {
            var packet = new BattleResultRequest();
            packet.score = score;
            return packet;
        }


        public short ProtocolId()
        {
            return 1006;
        }
    }


    public class BattleResultRequestRegistration : IProtocolRegistration
    {
        public short ProtocolId()
        {
            return 1006;
        }

        public void Write(ByteBuffer buffer, IPacket packet)
        {
            if (packet == null)
            {
                buffer.WriteBool(false);
                return;
            }
            buffer.WriteBool(true);
            BattleResultRequest message = (BattleResultRequest) packet;
            buffer.WriteInt(message.score);
        }

        public IPacket Read(ByteBuffer buffer)
        {
            if (!buffer.ReadBool())
            {
                return null;
            }
            BattleResultRequest packet = new BattleResultRequest();
            int result0 = buffer.ReadInt();
            packet.score = result0;
            return packet;
        }
    }
}
