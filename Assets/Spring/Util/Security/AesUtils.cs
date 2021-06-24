using System;
using System.Security.Cryptography;
using System.Text;

namespace Spring.Util.Security
{
    public abstract class AesUtils
    {
        private static readonly string AES_KEY = "mini-Dz$MDhMj`?WF*g%@DfX|A3v0PO&";
 
        /// <summary>
        /// AES加密
        /// </summary>
        /// <param name="content">明文</param>
        public static string GetEncryptString(string content)
        {
            byte[] contentBytes = Encoding.UTF8.GetBytes(content);
            byte[] keyBytes = Encoding.UTF8.GetBytes(AES_KEY);
            RijndaelManaged rm = new RijndaelManaged();
            rm.Key = keyBytes;
            rm.Mode = CipherMode.ECB;
            rm.Padding = PaddingMode.PKCS7;
            ICryptoTransform ict = rm.CreateEncryptor();
            byte[] resultBytes = ict.TransformFinalBlock(contentBytes, 0, contentBytes.Length);
            return Convert.ToBase64String(resultBytes, 0, resultBytes.Length);
        }
 
        /// <summary>
        /// AES解密
        /// </summary>
        /// <param name="content">密文</param>
        public static string GetDecryptString(string content)
        {
            byte[] contentBytes = Convert.FromBase64String(content);
            byte[] keyBytes = Encoding.UTF8.GetBytes(AES_KEY);
            RijndaelManaged rm = new RijndaelManaged();
            rm.Key = keyBytes;
            rm.Mode = CipherMode.ECB;
            rm.Padding = PaddingMode.PKCS7;
            ICryptoTransform ict = rm.CreateDecryptor();
            byte[] resultBytes = ict.TransformFinalBlock(contentBytes, 0, contentBytes.Length);
            return Encoding.UTF8.GetString(resultBytes);
        }

    }
}