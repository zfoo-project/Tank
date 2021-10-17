using System;
using System.Collections.Generic;
using CsProtocol.Buffer;

namespace CsProtocol
{
    // @author jaysunxiao
    // @version 3.0
    public class NormalObject : IPacket
    {
        public byte a;
        public byte[] aaa;
        public short b;
        public int c;
        public long d;
        public float e;
        public double f;
        public bool g;
        public string jj;
        public ObjectA kk;
        public List<int> l;
        public List<long> ll;
        public List<ObjectA> lll;
        public List<string> llll;
        public Dictionary<int, string> m;
        public Dictionary<int, ObjectA> mm;
        public HashSet<int> s;
        public HashSet<string> ssss;

        public static NormalObject ValueOf(byte a, byte[] aaa, short b, int c, long d, float e, double f, bool g, string jj, ObjectA kk, List<int> l, List<long> ll, List<ObjectA> lll, List<string> llll, Dictionary<int, string> m, Dictionary<int, ObjectA> mm, HashSet<int> s, HashSet<string> ssss)
        {
            var packet = new NormalObject();
            packet.a = a;
            packet.aaa = aaa;
            packet.b = b;
            packet.c = c;
            packet.d = d;
            packet.e = e;
            packet.f = f;
            packet.g = g;
            packet.jj = jj;
            packet.kk = kk;
            packet.l = l;
            packet.ll = ll;
            packet.lll = lll;
            packet.llll = llll;
            packet.m = m;
            packet.mm = mm;
            packet.s = s;
            packet.ssss = ssss;
            return packet;
        }


        public short ProtocolId()
        {
            return 101;
        }
    }


    public class NormalObjectRegistration : IProtocolRegistration
    {
        public short ProtocolId()
        {
            return 101;
        }

        public void Write(ByteBuffer buffer, IPacket packet)
        {
            if (buffer.WritePacketFlag(packet))
            {
                return;
            }
            NormalObject message = (NormalObject) packet;
            buffer.WriteByte(message.a);
            buffer.WriteByteArray(message.aaa);
            buffer.WriteShort(message.b);
            buffer.WriteInt(message.c);
            buffer.WriteLong(message.d);
            buffer.WriteFloat(message.e);
            buffer.WriteDouble(message.f);
            buffer.WriteBool(message.g);
            buffer.WriteString(message.jj);
            ProtocolManager.GetProtocol(102).Write(buffer, message.kk);
            buffer.WriteIntList(message.l);
            buffer.WriteLongList(message.ll);
            buffer.WritePacketList<ObjectA>(message.lll, 102);
            buffer.WriteStringList(message.llll);
            if ((message.m == null) || (message.m.Count == 0))
            {
                buffer.WriteInt(0);
            }
            else
            {
                buffer.WriteInt(message.m.Count);
                foreach (var i0 in message.m)
                {
                    var keyElement1 = i0.Key;
                    var valueElement2 = i0.Value;
                    buffer.WriteInt(keyElement1);
                    buffer.WriteString(valueElement2);
                }
            }
            if ((message.mm == null) || (message.mm.Count == 0))
            {
                buffer.WriteInt(0);
            }
            else
            {
                buffer.WriteInt(message.mm.Count);
                foreach (var i3 in message.mm)
                {
                    var keyElement4 = i3.Key;
                    var valueElement5 = i3.Value;
                    buffer.WriteInt(keyElement4);
                    ProtocolManager.GetProtocol(102).Write(buffer, valueElement5);
                }
            }
            if (message.s == null)
            {
                buffer.WriteInt(0);
            }
            else
            {
                buffer.WriteInt(message.s.Count);
                foreach (var i6 in message.s)
                {
                    buffer.WriteInt(i6);
                }
            }
            if (message.ssss == null)
            {
                buffer.WriteInt(0);
            }
            else
            {
                buffer.WriteInt(message.ssss.Count);
                foreach (var i7 in message.ssss)
                {
                    buffer.WriteString(i7);
                }
            }
        }

        public IPacket Read(ByteBuffer buffer)
        {
            if (!buffer.ReadBool())
            {
                return null;
            }
            NormalObject packet = new NormalObject();
            byte result8 = buffer.ReadByte();
            packet.a = result8;
            int size11 = buffer.ReadInt();
            byte[] result9 = new byte[size11];
            if (size11 > 0)
            {
                for (int index10 = 0; index10 < size11; index10++)
                {
                    byte result12 = buffer.ReadByte();
                    result9[index10] = result12;
                }
            }
            packet.aaa = result9;
            short result13 = buffer.ReadShort();
            packet.b = result13;
            int result14 = buffer.ReadInt();
            packet.c = result14;
            long result15 = buffer.ReadLong();
            packet.d = result15;
            float result16 = buffer.ReadFloat();
            packet.e = result16;
            double result17 = buffer.ReadDouble();
            packet.f = result17;
            bool result18 = buffer.ReadBool();
            packet.g = result18;
            string result19 = buffer.ReadString();
            packet.jj = result19;
            ObjectA result20 = (ObjectA) ProtocolManager.GetProtocol(102).Read(buffer);
            packet.kk = result20;
            var list21 = buffer.ReadIntList();
            packet.l = list21;
            var list22 = buffer.ReadLongList();
            packet.ll = list22;
            var list23 = buffer.ReadPacketList<ObjectA>(102);
            packet.lll = list23;
            var list24 = buffer.ReadStringList();
            packet.llll = list24;
            int size26 = buffer.ReadInt();
            var result25 = new Dictionary<int, string>(size26);
            if (size26 > 0)
            {
                for (var index27 = 0; index27 < size26; index27++)
                {
                    int result28 = buffer.ReadInt();
                    string result29 = buffer.ReadString();
                    result25[result28] = result29;
                }
            }
            packet.m = result25;
            int size31 = buffer.ReadInt();
            var result30 = new Dictionary<int, ObjectA>(size31);
            if (size31 > 0)
            {
                for (var index32 = 0; index32 < size31; index32++)
                {
                    int result33 = buffer.ReadInt();
                    ObjectA result34 = (ObjectA) ProtocolManager.GetProtocol(102).Read(buffer);
                    result30[result33] = result34;
                }
            }
            packet.mm = result30;
            int size37 = buffer.ReadInt();
            var result35 = new HashSet<int>();
            if (size37 > 0)
            {
                for (int index36 = 0; index36 < size37; index36++)
                {
                    int result38 = buffer.ReadInt();
                    result35.Add(result38);
                }
            }
            packet.s = result35;
            int size41 = buffer.ReadInt();
            var result39 = new HashSet<string>();
            if (size41 > 0)
            {
                for (int index40 = 0; index40 < size41; index40++)
                {
                    string result42 = buffer.ReadString();
                    result39.Add(result42);
                }
            }
            packet.ssss = result39;
            return packet;
        }
    }
}
