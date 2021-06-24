using NUnit.Framework;
using Spring.Logger;
using Spring.Util;
using Spring.Util.Security;

namespace Test.Editor.Spring.Util
{
    public class AesUtilsTest
    {

        [Test]
        public void Test()
        {
            var str = "hello world";
            var encryptString = AesUtils.GetEncryptString(str);
            Log.Info(encryptString);
            var decryptString = AesUtils.GetDecryptString(encryptString);
            Log.Info(decryptString);
            AssertionUtils.Equals(str, decryptString);
        }

    }
}