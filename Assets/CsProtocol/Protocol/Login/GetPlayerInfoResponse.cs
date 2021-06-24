using System;
using System.Collections.Generic;
using CsProtocol.Buffer;

namespace CsProtocol
{
    // @author jaysunxiao
    // @version 1.0
    // @since 2019-10-15 17:55
    public class GetPlayerInfoResponse : IPacket
    {
        public CurrencyVo currencyVo;
        public PlayerInfo playerInfo;

        public static GetPlayerInfoResponse ValueOf(CurrencyVo currencyVo, PlayerInfo playerInfo)
        {
            var packet = new GetPlayerInfoResponse();
            packet.currencyVo = currencyVo;
            packet.playerInfo = playerInfo;
            return packet;
        }


        public short ProtocolId()
        {
            return 1005;
        }
    }


    public class GetPlayerInfoResponseRegistration : IProtocolRegistration
    {
        public short ProtocolId()
        {
            return 1005;
        }

        public void Write(ByteBuffer buffer, IPacket packet)
        {
            if (packet == null)
            {
                buffer.WriteBool(false);
                return;
            }
            buffer.WriteBool(true);
            GetPlayerInfoResponse message = (GetPlayerInfoResponse) packet;
            ProtocolManager.GetProtocol(401).Write(buffer, message.currencyVo);
            ProtocolManager.GetProtocol(400).Write(buffer, message.playerInfo);
        }

        public IPacket Read(ByteBuffer buffer)
        {
            if (!buffer.ReadBool())
            {
                return null;
            }
            GetPlayerInfoResponse packet = new GetPlayerInfoResponse();
            CurrencyVo result0 = (CurrencyVo) ProtocolManager.GetProtocol(401).Read(buffer);
            packet.currencyVo = result0;
            PlayerInfo result1 = (PlayerInfo) ProtocolManager.GetProtocol(400).Read(buffer);
            packet.playerInfo = result1;
            return packet;
        }
    }
}
