using System.Collections;
using NUnit.Framework;
using Spring.Logger;
using UnityEngine;

namespace Test.Editor
{
    public class CoroutineTest : MonoBehaviour
    {
        // test 1
        // routine 1
        // test 2
        // routine 2 （等待5秒后才打印出来的）
        [Ignore("empty")]
        [Test]
        public void Test()
        {
            Log.Info("test 1");
            StartCoroutine(RoutineTest());
            Log.Info("test 2");
        }

        private IEnumerator RoutineTest()
        {
            Log.Info("routine 1");
            // yield return null;
            // 等待5秒后输出
            yield return new WaitForSeconds(5);
            Log.Info("routine 2");
        }
    }
}