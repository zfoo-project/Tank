using System.Collections.Generic;
using NUnit.Framework;
using Spring.Util;

namespace Test.Editor.Spring.Util
{
    public class StringUtilsTest
    {

        [Test]
        public void JoinWithTest()
        {
            var list = new List<string>()
            {
                "a", "b", "c"
            };

            var str = StringUtils.JoinWith(",", list.ToArray());
            AssertionUtils.Equals(str, "a,b,c");
        }

    }
}