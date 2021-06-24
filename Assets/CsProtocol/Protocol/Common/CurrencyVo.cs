using System;
using System.Collections.Generic;
using CsProtocol.Buffer;

namespace CsProtocol
{
    // @author jaysunxiao
    // @version 3.0
    public class CurrencyVo : IPacket
    {
        // 能量
        public int energy;
        // 钻石
        public int gem;
        // 金币
        public int gold;

        public static CurrencyVo ValueOf(int energy, int gem, int gold)
        {
            var packet = new CurrencyVo();
            packet.energy = energy;
            packet.gem = gem;
            packet.gold = gold;
            return packet;
        }


        public short ProtocolId()
        {
            return 401;
        }
    }


    public class CurrencyVoRegistration : IProtocolRegistration
    {
        public short ProtocolId()
        {
            return 401;
        }

        public void Write(ByteBuffer buffer, IPacket packet)
        {
            if (packet == null)
            {
                buffer.WriteBool(false);
                return;
            }
            buffer.WriteBool(true);
            CurrencyVo message = (CurrencyVo) packet;
            buffer.WriteInt(message.energy);
            buffer.WriteInt(message.gem);
            buffer.WriteInt(message.gold);
        }

        public IPacket Read(ByteBuffer buffer)
        {
            if (!buffer.ReadBool())
            {
                return null;
            }
            CurrencyVo packet = new CurrencyVo();
            int result0 = buffer.ReadInt();
            packet.energy = result0;
            int result1 = buffer.ReadInt();
            packet.gem = result1;
            int result2 = buffer.ReadInt();
            packet.gold = result2;
            return packet;
        }
    }
}
