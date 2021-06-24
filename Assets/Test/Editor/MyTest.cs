using MiniKing.Script.Constant;
using NUnit.Framework;
using Spring.Logger;

namespace Test.Editor
{
    public class MyTest
    {

        [Test]
        public void test()
        {
            Log.Info(I18nEnum.check_version_error.ToString());
        }
    }
}