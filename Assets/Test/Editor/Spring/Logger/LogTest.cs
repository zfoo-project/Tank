using NUnit.Framework;
using Spring.Logger;

namespace Test.Editor.Spring.Logger
{
    public class LogTest
    {
        [Test]
        public void Test()
        {
            Log.Debug("Debug");
            Log.Info("Info");
            Log.Warning("Warning");
        }
    }
}