using System;
using System.Runtime.InteropServices;
using Summer.Base.Model;
using Spring.Util;

namespace Summer.FileSystem.Model
{
    /// <summary>
    /// 字符串数据。
    /// </summary>
    [StructLayout(LayoutKind.Auto)]
    public struct StringData
    {
        private static readonly byte[] cachedBytes = new byte[byte.MaxValue + 1];

        private readonly byte length;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = byte.MaxValue)]
        private readonly byte[] bytes;

        public StringData(byte length, byte[] bytes)
        {
            this.length = length;
            this.bytes = bytes;
        }

        public string GetString(byte[] encryptBytes)
        {
            if (length <= 0)
            {
                return null;
            }

            Array.Copy(bytes, 0, cachedBytes, 0, length);
            EncryptionUtils.GetSelfXorBytes(cachedBytes, 0, length, encryptBytes);
            return ConverterUtils.GetString(cachedBytes, 0, length);
        }

        public StringData SetString(string value, byte[] encryptBytes)
        {
            if (string.IsNullOrEmpty(value))
            {
                return Clear();
            }

            int length = ConverterUtils.GetBytes(value, cachedBytes);
            if (length > byte.MaxValue)
            {
                throw new GameFrameworkException(StringUtils.Format("String '{}' is too long.", value));
            }

            EncryptionUtils.GetSelfXorBytes(cachedBytes, encryptBytes);
            Array.Copy(cachedBytes, 0, bytes, 0, length);
            return new StringData((byte) length, bytes);
        }

        public StringData Clear()
        {
            return new StringData(0, bytes);
        }
    }
}