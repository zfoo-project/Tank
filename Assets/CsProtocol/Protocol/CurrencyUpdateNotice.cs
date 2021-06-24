using System;
using System.Collections.Generic;
using CsProtocol.Buffer;

namespace CsProtocol
{
    // @author jaysunxiao
    // @version 3.0
    public class CurrencyUpdateNotice : IPacket
    {
        public CurrencyVo currencyVo;

        public static CurrencyUpdateNotice ValueOf(CurrencyVo currencyVo)
        {
            var packet = new CurrencyUpdateNotice();
            packet.currencyVo = currencyVo;
            return packet;
        }


        public short ProtocolId()
        {
            return 1100;
        }
    }


    public class CurrencyUpdateNoticeRegistration : IProtocolRegistration
    {
        public short ProtocolId()
        {
            return 1100;
        }

        public void Write(ByteBuffer buffer, IPacket packet)
        {
            if (packet == null)
            {
                buffer.WriteBool(false);
                return;
            }
            buffer.WriteBool(true);
            CurrencyUpdateNotice message = (CurrencyUpdateNotice) packet;
            ProtocolManager.GetProtocol(401).Write(buffer, message.currencyVo);
        }

        public IPacket Read(ByteBuffer buffer)
        {
            if (!buffer.ReadBool())
            {
                return null;
            }
            CurrencyUpdateNotice packet = new CurrencyUpdateNotice();
            CurrencyVo result0 = (CurrencyVo) ProtocolManager.GetProtocol(401).Read(buffer);
            packet.currencyVo = result0;
            return packet;
        }
    }
}
