using System;
using System.Collections.Generic;
using CsProtocol.Buffer;

namespace CsProtocol
{
    // @author jaysunxiao
    // @version 3.0
    public class ScoreRankRequest : IPacket
    {

        public static ScoreRankRequest ValueOf()
        {
            var packet = new ScoreRankRequest();
            return packet;
        }


        public short ProtocolId()
        {
            return 3002;
        }
    }


    public class ScoreRankRequestRegistration : IProtocolRegistration
    {
        public short ProtocolId()
        {
            return 3002;
        }

        public void Write(ByteBuffer buffer, IPacket packet)
        {
            if (packet == null)
            {
                buffer.WriteBool(false);
                return;
            }
            buffer.WriteBool(true);
            ScoreRankRequest message = (ScoreRankRequest) packet;
        }

        public IPacket Read(ByteBuffer buffer)
        {
            if (!buffer.ReadBool())
            {
                return null;
            }
            ScoreRankRequest packet = new ScoreRankRequest();
            return packet;
        }
    }
}
