using System.Runtime.InteropServices;
using Spring.Util;

namespace Summer.FileSystem.Model
{
    /// <summary>
    /// 头数据。
    /// </summary>
    [StructLayout(LayoutKind.Auto)]
    public struct HeaderData
    {
        private const int HeaderLength = 3;
        private const int EncryptBytesLength = 4;
        private static readonly byte[] Header = new byte[HeaderLength] {(byte) 'G', (byte) 'F', (byte) 'F'};

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = HeaderLength)]
        private readonly byte[] header;

        private readonly byte version;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = EncryptBytesLength)]
        private readonly byte[] encryptBytes;

        private readonly int maxFileCount;
        private readonly int maxBlockCount;
        private readonly int blockCount;

        public HeaderData(int maxFileCount, int maxBlockCount)
            : this(0, new byte[EncryptBytesLength], maxFileCount, maxBlockCount, 0)
        {
            RandomUtils.GetRandomBytes(encryptBytes);
        }

        public HeaderData(byte version, byte[] encryptBytes, int maxFileCount, int maxBlockCount, int blockCount)
        {
            this.header = Header;
            this.version = version;
            this.encryptBytes = encryptBytes;
            this.maxFileCount = maxFileCount;
            this.maxBlockCount = maxBlockCount;
            this.blockCount = blockCount;
        }

        public bool IsValid
        {
            get
            {
                return header.Length == HeaderLength && header[0] == Header[0] && header[1] == Header[1] &&
                       header[2] == Header[2] && version == 0 && encryptBytes.Length == EncryptBytesLength
                       && maxFileCount > 0 && maxBlockCount > 0 && maxFileCount <= maxBlockCount &&
                       blockCount > 0 && blockCount <= maxBlockCount;
            }
        }

        public byte Version
        {
            get { return version; }
        }

        public int MaxFileCount
        {
            get { return maxFileCount; }
        }

        public int MaxBlockCount
        {
            get { return maxBlockCount; }
        }

        public int BlockCount
        {
            get { return blockCount; }
        }

        public byte[] GetEncryptBytes()
        {
            return encryptBytes;
        }

        public HeaderData SetBlockCount(int blockCount)
        {
            return new HeaderData(version, encryptBytes, maxFileCount, maxBlockCount, blockCount);
        }
    }
}