using System;
using System.Collections.Generic;
using CsProtocol.Buffer;

namespace CsProtocol
{
    // 复杂的对象
    // 包括了各种复杂的结构，数组，List，Set，Map
    //
    // @author jaysunxiao
    // @version 3.0
    public class ComplexObject : IPacket
    {
        // byte类型，最简单的整形
        public byte a;
        // byte的包装类型
        // 优先使用基础类型，包装类型会有装箱拆箱
        public byte aa;
        // 数组类型
        public byte[] aaa;
        public byte[] aaaa;
        public short b;
        public short bb;
        public short[] bbb;
        public short[] bbbb;
        public int c;
        public int cc;
        public int[] ccc;
        public int[] cccc;
        public long d;
        public long dd;
        public long[] ddd;
        public long[] dddd;
        public float e;
        public float ee;
        public float[] eee;
        public float[] eeee;
        public double f;
        public double ff;
        public double[] fff;
        public double[] ffff;
        public bool g;
        public bool gg;
        public bool[] ggg;
        public bool[] gggg;
        public char h;
        public char hh;
        public char[] hhh;
        public char[] hhhh;
        public string jj;
        public string[] jjj;
        public ObjectA kk;
        public ObjectA[] kkk;
        public List<int> l;
        public List<List<List<int>>> ll;
        public List<List<ObjectA>> lll;
        public List<string> llll;
        public List<Dictionary<int, string>> lllll;
        public Dictionary<int, string> m;
        public Dictionary<int, ObjectA> mm;
        public Dictionary<ObjectA, List<int>> mmm;
        public Dictionary<List<List<ObjectA>>, List<List<List<int>>>> mmmm;
        public Dictionary<List<Dictionary<int, string>>, HashSet<Dictionary<int, string>>> mmmmm;
        public HashSet<int> s;
        public HashSet<HashSet<List<int>>> ss;
        public HashSet<HashSet<ObjectA>> sss;
        public HashSet<string> ssss;
        public HashSet<Dictionary<int, string>> sssss;

        public static ComplexObject ValueOf(byte a, byte aa, byte[] aaa, byte[] aaaa, short b, short bb, short[] bbb, short[] bbbb, int c, int cc, int[] ccc, int[] cccc, long d, long dd, long[] ddd, long[] dddd, float e, float ee, float[] eee, float[] eeee, double f, double ff, double[] fff, double[] ffff, bool g, bool gg, bool[] ggg, bool[] gggg, char h, char hh, char[] hhh, char[] hhhh, string jj, string[] jjj, ObjectA kk, ObjectA[] kkk, List<int> l, List<List<List<int>>> ll, List<List<ObjectA>> lll, List<string> llll, List<Dictionary<int, string>> lllll, Dictionary<int, string> m, Dictionary<int, ObjectA> mm, Dictionary<ObjectA, List<int>> mmm, Dictionary<List<List<ObjectA>>, List<List<List<int>>>> mmmm, Dictionary<List<Dictionary<int, string>>, HashSet<Dictionary<int, string>>> mmmmm, HashSet<int> s, HashSet<HashSet<List<int>>> ss, HashSet<HashSet<ObjectA>> sss, HashSet<string> ssss, HashSet<Dictionary<int, string>> sssss)
        {
            var packet = new ComplexObject();
            packet.a = a;
            packet.aa = aa;
            packet.aaa = aaa;
            packet.aaaa = aaaa;
            packet.b = b;
            packet.bb = bb;
            packet.bbb = bbb;
            packet.bbbb = bbbb;
            packet.c = c;
            packet.cc = cc;
            packet.ccc = ccc;
            packet.cccc = cccc;
            packet.d = d;
            packet.dd = dd;
            packet.ddd = ddd;
            packet.dddd = dddd;
            packet.e = e;
            packet.ee = ee;
            packet.eee = eee;
            packet.eeee = eeee;
            packet.f = f;
            packet.ff = ff;
            packet.fff = fff;
            packet.ffff = ffff;
            packet.g = g;
            packet.gg = gg;
            packet.ggg = ggg;
            packet.gggg = gggg;
            packet.h = h;
            packet.hh = hh;
            packet.hhh = hhh;
            packet.hhhh = hhhh;
            packet.jj = jj;
            packet.jjj = jjj;
            packet.kk = kk;
            packet.kkk = kkk;
            packet.l = l;
            packet.ll = ll;
            packet.lll = lll;
            packet.llll = llll;
            packet.lllll = lllll;
            packet.m = m;
            packet.mm = mm;
            packet.mmm = mmm;
            packet.mmmm = mmmm;
            packet.mmmmm = mmmmm;
            packet.s = s;
            packet.ss = ss;
            packet.sss = sss;
            packet.ssss = ssss;
            packet.sssss = sssss;
            return packet;
        }


        public short ProtocolId()
        {
            return 100;
        }
    }


    public class ComplexObjectRegistration : IProtocolRegistration
    {
        public short ProtocolId()
        {
            return 100;
        }

        public void Write(ByteBuffer buffer, IPacket packet)
        {
            if (buffer.WritePacketFlag(packet))
            {
                return;
            }
            ComplexObject message = (ComplexObject) packet;
            buffer.WriteByte(message.a);
            buffer.WriteByte(message.aa);
            buffer.WriteByteArray(message.aaa);
            buffer.WriteByteArray(message.aaaa);
            buffer.WriteShort(message.b);
            buffer.WriteShort(message.bb);
            buffer.WriteShortArray(message.bbb);
            buffer.WriteShortArray(message.bbbb);
            buffer.WriteInt(message.c);
            buffer.WriteInt(message.cc);
            buffer.WriteIntArray(message.ccc);
            buffer.WriteIntArray(message.cccc);
            buffer.WriteLong(message.d);
            buffer.WriteLong(message.dd);
            buffer.WriteLongArray(message.ddd);
            buffer.WriteLongArray(message.dddd);
            buffer.WriteFloat(message.e);
            buffer.WriteFloat(message.ee);
            buffer.WriteFloatArray(message.eee);
            buffer.WriteFloatArray(message.eeee);
            buffer.WriteDouble(message.f);
            buffer.WriteDouble(message.ff);
            buffer.WriteDoubleArray(message.fff);
            buffer.WriteDoubleArray(message.ffff);
            buffer.WriteBool(message.g);
            buffer.WriteBool(message.gg);
            buffer.WriteBooleanArray(message.ggg);
            buffer.WriteBooleanArray(message.gggg);
            buffer.WriteChar(message.h);
            buffer.WriteChar(message.hh);
            buffer.WriteCharArray(message.hhh);
            buffer.WriteCharArray(message.hhhh);
            buffer.WriteString(message.jj);
            buffer.WriteStringArray(message.jjj);
            ProtocolManager.GetProtocol(102).Write(buffer, message.kk);
            buffer.WritePacketArray<ObjectA>(message.kkk, 102);
            buffer.WriteIntList(message.l);
            if (message.ll == null)
            {
                buffer.WriteInt(0);
            }
            else
            {
                buffer.WriteInt(message.ll.Count);
                int length0 = message.ll.Count;
                for (int i1 = 0; i1 < length0; i1++)
                {
                    var element2 = message.ll[i1];
                    if (element2 == null)
                    {
                        buffer.WriteInt(0);
                    }
                    else
                    {
                        buffer.WriteInt(element2.Count);
                        int length3 = element2.Count;
                        for (int i4 = 0; i4 < length3; i4++)
                        {
                            var element5 = element2[i4];
                            buffer.WriteIntList(element5);
                        }
                    }
                }
            }
            if (message.lll == null)
            {
                buffer.WriteInt(0);
            }
            else
            {
                buffer.WriteInt(message.lll.Count);
                int length6 = message.lll.Count;
                for (int i7 = 0; i7 < length6; i7++)
                {
                    var element8 = message.lll[i7];
                    buffer.WritePacketList<ObjectA>(element8, 102);
                }
            }
            buffer.WriteStringList(message.llll);
            if (message.lllll == null)
            {
                buffer.WriteInt(0);
            }
            else
            {
                buffer.WriteInt(message.lllll.Count);
                int length9 = message.lllll.Count;
                for (int i10 = 0; i10 < length9; i10++)
                {
                    var element11 = message.lllll[i10];
                    if ((element11 == null) || (element11.Count == 0))
                    {
                        buffer.WriteInt(0);
                    }
                    else
                    {
                        buffer.WriteInt(element11.Count);
                        foreach (var i12 in element11)
                        {
                            var keyElement13 = i12.Key;
                            var valueElement14 = i12.Value;
                            buffer.WriteInt(keyElement13);
                            buffer.WriteString(valueElement14);
                        }
                    }
                }
            }
            if ((message.m == null) || (message.m.Count == 0))
            {
                buffer.WriteInt(0);
            }
            else
            {
                buffer.WriteInt(message.m.Count);
                foreach (var i15 in message.m)
                {
                    var keyElement16 = i15.Key;
                    var valueElement17 = i15.Value;
                    buffer.WriteInt(keyElement16);
                    buffer.WriteString(valueElement17);
                }
            }
            if ((message.mm == null) || (message.mm.Count == 0))
            {
                buffer.WriteInt(0);
            }
            else
            {
                buffer.WriteInt(message.mm.Count);
                foreach (var i18 in message.mm)
                {
                    var keyElement19 = i18.Key;
                    var valueElement20 = i18.Value;
                    buffer.WriteInt(keyElement19);
                    ProtocolManager.GetProtocol(102).Write(buffer, valueElement20);
                }
            }
            if ((message.mmm == null) || (message.mmm.Count == 0))
            {
                buffer.WriteInt(0);
            }
            else
            {
                buffer.WriteInt(message.mmm.Count);
                foreach (var i21 in message.mmm)
                {
                    var keyElement22 = i21.Key;
                    var valueElement23 = i21.Value;
                    ProtocolManager.GetProtocol(102).Write(buffer, keyElement22);
                    buffer.WriteIntList(valueElement23);
                }
            }
            if ((message.mmmm == null) || (message.mmmm.Count == 0))
            {
                buffer.WriteInt(0);
            }
            else
            {
                buffer.WriteInt(message.mmmm.Count);
                foreach (var i24 in message.mmmm)
                {
                    var keyElement25 = i24.Key;
                    var valueElement26 = i24.Value;
                    if (keyElement25 == null)
                    {
                        buffer.WriteInt(0);
                    }
                    else
                    {
                        buffer.WriteInt(keyElement25.Count);
                        int length27 = keyElement25.Count;
                        for (int i28 = 0; i28 < length27; i28++)
                        {
                            var element29 = keyElement25[i28];
                            buffer.WritePacketList<ObjectA>(element29, 102);
                        }
                    }
                    if (valueElement26 == null)
                    {
                        buffer.WriteInt(0);
                    }
                    else
                    {
                        buffer.WriteInt(valueElement26.Count);
                        int length30 = valueElement26.Count;
                        for (int i31 = 0; i31 < length30; i31++)
                        {
                            var element32 = valueElement26[i31];
                            if (element32 == null)
                            {
                                buffer.WriteInt(0);
                            }
                            else
                            {
                                buffer.WriteInt(element32.Count);
                                int length33 = element32.Count;
                                for (int i34 = 0; i34 < length33; i34++)
                                {
                                    var element35 = element32[i34];
                                    buffer.WriteIntList(element35);
                                }
                            }
                        }
                    }
                }
            }
            if ((message.mmmmm == null) || (message.mmmmm.Count == 0))
            {
                buffer.WriteInt(0);
            }
            else
            {
                buffer.WriteInt(message.mmmmm.Count);
                foreach (var i36 in message.mmmmm)
                {
                    var keyElement37 = i36.Key;
                    var valueElement38 = i36.Value;
                    if (keyElement37 == null)
                    {
                        buffer.WriteInt(0);
                    }
                    else
                    {
                        buffer.WriteInt(keyElement37.Count);
                        int length39 = keyElement37.Count;
                        for (int i40 = 0; i40 < length39; i40++)
                        {
                            var element41 = keyElement37[i40];
                            if ((element41 == null) || (element41.Count == 0))
                            {
                                buffer.WriteInt(0);
                            }
                            else
                            {
                                buffer.WriteInt(element41.Count);
                                foreach (var i42 in element41)
                                {
                                    var keyElement43 = i42.Key;
                                    var valueElement44 = i42.Value;
                                    buffer.WriteInt(keyElement43);
                                    buffer.WriteString(valueElement44);
                                }
                            }
                        }
                    }
                    if (valueElement38 == null)
                    {
                        buffer.WriteInt(0);
                    }
                    else
                    {
                        buffer.WriteInt(valueElement38.Count);
                        foreach (var i45 in valueElement38)
                        {
                            if ((i45 == null) || (i45.Count == 0))
                            {
                                buffer.WriteInt(0);
                            }
                            else
                            {
                                buffer.WriteInt(i45.Count);
                                foreach (var i46 in i45)
                                {
                                    var keyElement47 = i46.Key;
                                    var valueElement48 = i46.Value;
                                    buffer.WriteInt(keyElement47);
                                    buffer.WriteString(valueElement48);
                                }
                            }
                        }
                    }
                }
            }
            if (message.s == null)
            {
                buffer.WriteInt(0);
            }
            else
            {
                buffer.WriteInt(message.s.Count);
                foreach (var i49 in message.s)
                {
                    buffer.WriteInt(i49);
                }
            }
            if (message.ss == null)
            {
                buffer.WriteInt(0);
            }
            else
            {
                buffer.WriteInt(message.ss.Count);
                foreach (var i50 in message.ss)
                {
                    if (i50 == null)
                    {
                        buffer.WriteInt(0);
                    }
                    else
                    {
                        buffer.WriteInt(i50.Count);
                        foreach (var i51 in i50)
                        {
                            buffer.WriteIntList(i51);
                        }
                    }
                }
            }
            if (message.sss == null)
            {
                buffer.WriteInt(0);
            }
            else
            {
                buffer.WriteInt(message.sss.Count);
                foreach (var i52 in message.sss)
                {
                    if (i52 == null)
                    {
                        buffer.WriteInt(0);
                    }
                    else
                    {
                        buffer.WriteInt(i52.Count);
                        foreach (var i53 in i52)
                        {
                            ProtocolManager.GetProtocol(102).Write(buffer, i53);
                        }
                    }
                }
            }
            if (message.ssss == null)
            {
                buffer.WriteInt(0);
            }
            else
            {
                buffer.WriteInt(message.ssss.Count);
                foreach (var i54 in message.ssss)
                {
                    buffer.WriteString(i54);
                }
            }
            if (message.sssss == null)
            {
                buffer.WriteInt(0);
            }
            else
            {
                buffer.WriteInt(message.sssss.Count);
                foreach (var i55 in message.sssss)
                {
                    if ((i55 == null) || (i55.Count == 0))
                    {
                        buffer.WriteInt(0);
                    }
                    else
                    {
                        buffer.WriteInt(i55.Count);
                        foreach (var i56 in i55)
                        {
                            var keyElement57 = i56.Key;
                            var valueElement58 = i56.Value;
                            buffer.WriteInt(keyElement57);
                            buffer.WriteString(valueElement58);
                        }
                    }
                }
            }
        }

        public IPacket Read(ByteBuffer buffer)
        {
            if (!buffer.ReadBool())
            {
                return null;
            }
            ComplexObject packet = new ComplexObject();
            byte result59 = buffer.ReadByte();
            packet.a = result59;
            byte result60 = buffer.ReadByte();
            packet.aa = result60;
            int size63 = buffer.ReadInt();
            byte[] result61 = new byte[size63];
            if (size63 > 0)
            {
                for (int index62 = 0; index62 < size63; index62++)
                {
                    byte result64 = buffer.ReadByte();
                    result61[index62] = result64;
                }
            }
            packet.aaa = result61;
            int size67 = buffer.ReadInt();
            byte[] result65 = new byte[size67];
            if (size67 > 0)
            {
                for (int index66 = 0; index66 < size67; index66++)
                {
                    byte result68 = buffer.ReadByte();
                    result65[index66] = result68;
                }
            }
            packet.aaaa = result65;
            short result69 = buffer.ReadShort();
            packet.b = result69;
            short result70 = buffer.ReadShort();
            packet.bb = result70;
            int size73 = buffer.ReadInt();
            short[] result71 = new short[size73];
            if (size73 > 0)
            {
                for (int index72 = 0; index72 < size73; index72++)
                {
                    short result74 = buffer.ReadShort();
                    result71[index72] = result74;
                }
            }
            packet.bbb = result71;
            int size77 = buffer.ReadInt();
            short[] result75 = new short[size77];
            if (size77 > 0)
            {
                for (int index76 = 0; index76 < size77; index76++)
                {
                    short result78 = buffer.ReadShort();
                    result75[index76] = result78;
                }
            }
            packet.bbbb = result75;
            int result79 = buffer.ReadInt();
            packet.c = result79;
            int result80 = buffer.ReadInt();
            packet.cc = result80;
            int size83 = buffer.ReadInt();
            int[] result81 = new int[size83];
            if (size83 > 0)
            {
                for (int index82 = 0; index82 < size83; index82++)
                {
                    int result84 = buffer.ReadInt();
                    result81[index82] = result84;
                }
            }
            packet.ccc = result81;
            int size87 = buffer.ReadInt();
            int[] result85 = new int[size87];
            if (size87 > 0)
            {
                for (int index86 = 0; index86 < size87; index86++)
                {
                    int result88 = buffer.ReadInt();
                    result85[index86] = result88;
                }
            }
            packet.cccc = result85;
            long result89 = buffer.ReadLong();
            packet.d = result89;
            long result90 = buffer.ReadLong();
            packet.dd = result90;
            int size93 = buffer.ReadInt();
            long[] result91 = new long[size93];
            if (size93 > 0)
            {
                for (int index92 = 0; index92 < size93; index92++)
                {
                    long result94 = buffer.ReadLong();
                    result91[index92] = result94;
                }
            }
            packet.ddd = result91;
            int size97 = buffer.ReadInt();
            long[] result95 = new long[size97];
            if (size97 > 0)
            {
                for (int index96 = 0; index96 < size97; index96++)
                {
                    long result98 = buffer.ReadLong();
                    result95[index96] = result98;
                }
            }
            packet.dddd = result95;
            float result99 = buffer.ReadFloat();
            packet.e = result99;
            float result100 = buffer.ReadFloat();
            packet.ee = result100;
            int size103 = buffer.ReadInt();
            float[] result101 = new float[size103];
            if (size103 > 0)
            {
                for (int index102 = 0; index102 < size103; index102++)
                {
                    float result104 = buffer.ReadFloat();
                    result101[index102] = result104;
                }
            }
            packet.eee = result101;
            int size107 = buffer.ReadInt();
            float[] result105 = new float[size107];
            if (size107 > 0)
            {
                for (int index106 = 0; index106 < size107; index106++)
                {
                    float result108 = buffer.ReadFloat();
                    result105[index106] = result108;
                }
            }
            packet.eeee = result105;
            double result109 = buffer.ReadDouble();
            packet.f = result109;
            double result110 = buffer.ReadDouble();
            packet.ff = result110;
            int size113 = buffer.ReadInt();
            double[] result111 = new double[size113];
            if (size113 > 0)
            {
                for (int index112 = 0; index112 < size113; index112++)
                {
                    double result114 = buffer.ReadDouble();
                    result111[index112] = result114;
                }
            }
            packet.fff = result111;
            int size117 = buffer.ReadInt();
            double[] result115 = new double[size117];
            if (size117 > 0)
            {
                for (int index116 = 0; index116 < size117; index116++)
                {
                    double result118 = buffer.ReadDouble();
                    result115[index116] = result118;
                }
            }
            packet.ffff = result115;
            bool result119 = buffer.ReadBool();
            packet.g = result119;
            bool result120 = buffer.ReadBool();
            packet.gg = result120;
            int size123 = buffer.ReadInt();
            bool[] result121 = new bool[size123];
            if (size123 > 0)
            {
                for (int index122 = 0; index122 < size123; index122++)
                {
                    bool result124 = buffer.ReadBool();
                    result121[index122] = result124;
                }
            }
            packet.ggg = result121;
            int size127 = buffer.ReadInt();
            bool[] result125 = new bool[size127];
            if (size127 > 0)
            {
                for (int index126 = 0; index126 < size127; index126++)
                {
                    bool result128 = buffer.ReadBool();
                    result125[index126] = result128;
                }
            }
            packet.gggg = result125;
            char result129 = buffer.ReadChar();
            packet.h = result129;
            char result130 = buffer.ReadChar();
            packet.hh = result130;
            int size133 = buffer.ReadInt();
            char[] result131 = new char[size133];
            if (size133 > 0)
            {
                for (int index132 = 0; index132 < size133; index132++)
                {
                    char result134 = buffer.ReadChar();
                    result131[index132] = result134;
                }
            }
            packet.hhh = result131;
            int size137 = buffer.ReadInt();
            char[] result135 = new char[size137];
            if (size137 > 0)
            {
                for (int index136 = 0; index136 < size137; index136++)
                {
                    char result138 = buffer.ReadChar();
                    result135[index136] = result138;
                }
            }
            packet.hhhh = result135;
            string result139 = buffer.ReadString();
            packet.jj = result139;
            int size142 = buffer.ReadInt();
            string[] result140 = new string[size142];
            if (size142 > 0)
            {
                for (int index141 = 0; index141 < size142; index141++)
                {
                    string result143 = buffer.ReadString();
                    result140[index141] = result143;
                }
            }
            packet.jjj = result140;
            ObjectA result144 = (ObjectA) ProtocolManager.GetProtocol(102).Read(buffer);
            packet.kk = result144;
            var array145 = buffer.ReadPacketArray<ObjectA>(102);
            packet.kkk = array145;
            var list146 = buffer.ReadIntList();
            packet.l = list146;
            int size149 = buffer.ReadInt();
            var result147 = new List<List<List<int>>>(size149);
            if (size149 > 0)
            {
                for (int index148 = 0; index148 < size149; index148++)
                {
                    int size152 = buffer.ReadInt();
                    var result150 = new List<List<int>>(size152);
                    if (size152 > 0)
                    {
                        for (int index151 = 0; index151 < size152; index151++)
                        {
                            var list153 = buffer.ReadIntList();
                            result150.Add(list153);
                        }
                    }
                    result147.Add(result150);
                }
            }
            packet.ll = result147;
            int size156 = buffer.ReadInt();
            var result154 = new List<List<ObjectA>>(size156);
            if (size156 > 0)
            {
                for (int index155 = 0; index155 < size156; index155++)
                {
                    var list157 = buffer.ReadPacketList<ObjectA>(102);
                    result154.Add(list157);
                }
            }
            packet.lll = result154;
            var list158 = buffer.ReadStringList();
            packet.llll = list158;
            int size161 = buffer.ReadInt();
            var result159 = new List<Dictionary<int, string>>(size161);
            if (size161 > 0)
            {
                for (int index160 = 0; index160 < size161; index160++)
                {
                    int size163 = buffer.ReadInt();
                    var result162 = new Dictionary<int, string>(size163);
                    if (size163 > 0)
                    {
                        for (var index164 = 0; index164 < size163; index164++)
                        {
                            int result165 = buffer.ReadInt();
                            string result166 = buffer.ReadString();
                            result162[result165] = result166;
                        }
                    }
                    result159.Add(result162);
                }
            }
            packet.lllll = result159;
            int size168 = buffer.ReadInt();
            var result167 = new Dictionary<int, string>(size168);
            if (size168 > 0)
            {
                for (var index169 = 0; index169 < size168; index169++)
                {
                    int result170 = buffer.ReadInt();
                    string result171 = buffer.ReadString();
                    result167[result170] = result171;
                }
            }
            packet.m = result167;
            int size173 = buffer.ReadInt();
            var result172 = new Dictionary<int, ObjectA>(size173);
            if (size173 > 0)
            {
                for (var index174 = 0; index174 < size173; index174++)
                {
                    int result175 = buffer.ReadInt();
                    ObjectA result176 = (ObjectA) ProtocolManager.GetProtocol(102).Read(buffer);
                    result172[result175] = result176;
                }
            }
            packet.mm = result172;
            int size178 = buffer.ReadInt();
            var result177 = new Dictionary<ObjectA, List<int>>(size178);
            if (size178 > 0)
            {
                for (var index179 = 0; index179 < size178; index179++)
                {
                    ObjectA result180 = (ObjectA) ProtocolManager.GetProtocol(102).Read(buffer);
                    var list181 = buffer.ReadIntList();
                    result177[result180] = list181;
                }
            }
            packet.mmm = result177;
            int size183 = buffer.ReadInt();
            var result182 = new Dictionary<List<List<ObjectA>>, List<List<List<int>>>>(size183);
            if (size183 > 0)
            {
                for (var index184 = 0; index184 < size183; index184++)
                {
                    int size187 = buffer.ReadInt();
                    var result185 = new List<List<ObjectA>>(size187);
                    if (size187 > 0)
                    {
                        for (int index186 = 0; index186 < size187; index186++)
                        {
                            var list188 = buffer.ReadPacketList<ObjectA>(102);
                            result185.Add(list188);
                        }
                    }
                    int size191 = buffer.ReadInt();
                    var result189 = new List<List<List<int>>>(size191);
                    if (size191 > 0)
                    {
                        for (int index190 = 0; index190 < size191; index190++)
                        {
                            int size194 = buffer.ReadInt();
                            var result192 = new List<List<int>>(size194);
                            if (size194 > 0)
                            {
                                for (int index193 = 0; index193 < size194; index193++)
                                {
                                    var list195 = buffer.ReadIntList();
                                    result192.Add(list195);
                                }
                            }
                            result189.Add(result192);
                        }
                    }
                    result182[result185] = result189;
                }
            }
            packet.mmmm = result182;
            int size197 = buffer.ReadInt();
            var result196 = new Dictionary<List<Dictionary<int, string>>, HashSet<Dictionary<int, string>>>(size197);
            if (size197 > 0)
            {
                for (var index198 = 0; index198 < size197; index198++)
                {
                    int size201 = buffer.ReadInt();
                    var result199 = new List<Dictionary<int, string>>(size201);
                    if (size201 > 0)
                    {
                        for (int index200 = 0; index200 < size201; index200++)
                        {
                            int size203 = buffer.ReadInt();
                            var result202 = new Dictionary<int, string>(size203);
                            if (size203 > 0)
                            {
                                for (var index204 = 0; index204 < size203; index204++)
                                {
                                    int result205 = buffer.ReadInt();
                                    string result206 = buffer.ReadString();
                                    result202[result205] = result206;
                                }
                            }
                            result199.Add(result202);
                        }
                    }
                    int size209 = buffer.ReadInt();
                    var result207 = new HashSet<Dictionary<int, string>>();
                    if (size209 > 0)
                    {
                        for (int index208 = 0; index208 < size209; index208++)
                        {
                            int size211 = buffer.ReadInt();
                            var result210 = new Dictionary<int, string>(size211);
                            if (size211 > 0)
                            {
                                for (var index212 = 0; index212 < size211; index212++)
                                {
                                    int result213 = buffer.ReadInt();
                                    string result214 = buffer.ReadString();
                                    result210[result213] = result214;
                                }
                            }
                            result207.Add(result210);
                        }
                    }
                    result196[result199] = result207;
                }
            }
            packet.mmmmm = result196;
            int size217 = buffer.ReadInt();
            var result215 = new HashSet<int>();
            if (size217 > 0)
            {
                for (int index216 = 0; index216 < size217; index216++)
                {
                    int result218 = buffer.ReadInt();
                    result215.Add(result218);
                }
            }
            packet.s = result215;
            int size221 = buffer.ReadInt();
            var result219 = new HashSet<HashSet<List<int>>>();
            if (size221 > 0)
            {
                for (int index220 = 0; index220 < size221; index220++)
                {
                    int size224 = buffer.ReadInt();
                    var result222 = new HashSet<List<int>>();
                    if (size224 > 0)
                    {
                        for (int index223 = 0; index223 < size224; index223++)
                        {
                            var list225 = buffer.ReadIntList();
                            result222.Add(list225);
                        }
                    }
                    result219.Add(result222);
                }
            }
            packet.ss = result219;
            int size228 = buffer.ReadInt();
            var result226 = new HashSet<HashSet<ObjectA>>();
            if (size228 > 0)
            {
                for (int index227 = 0; index227 < size228; index227++)
                {
                    int size231 = buffer.ReadInt();
                    var result229 = new HashSet<ObjectA>();
                    if (size231 > 0)
                    {
                        for (int index230 = 0; index230 < size231; index230++)
                        {
                            ObjectA result232 = (ObjectA) ProtocolManager.GetProtocol(102).Read(buffer);
                            result229.Add(result232);
                        }
                    }
                    result226.Add(result229);
                }
            }
            packet.sss = result226;
            int size235 = buffer.ReadInt();
            var result233 = new HashSet<string>();
            if (size235 > 0)
            {
                for (int index234 = 0; index234 < size235; index234++)
                {
                    string result236 = buffer.ReadString();
                    result233.Add(result236);
                }
            }
            packet.ssss = result233;
            int size239 = buffer.ReadInt();
            var result237 = new HashSet<Dictionary<int, string>>();
            if (size239 > 0)
            {
                for (int index238 = 0; index238 < size239; index238++)
                {
                    int size241 = buffer.ReadInt();
                    var result240 = new Dictionary<int, string>(size241);
                    if (size241 > 0)
                    {
                        for (var index242 = 0; index242 < size241; index242++)
                        {
                            int result243 = buffer.ReadInt();
                            string result244 = buffer.ReadString();
                            result240[result243] = result244;
                        }
                    }
                    result237.Add(result240);
                }
            }
            packet.sssss = result237;
            return packet;
        }
    }
}
