using LitJson;
using NUnit.Framework;
using Spring.Logger;
using Spring.Util.Json;

namespace Test.Editor.Spring.Util
{
    public class StudentTest
    {
        public int a;

        public string b;
    }


    public class JsonUtilsTest
    {
        [Test]
        public void Test()
        {
            var student = new StudentTest();
            student.a = 100;
            student.b = "Jack";

            var jsonStr = JsonUtils.object2String(student);
            Log.Info(jsonStr);

            var obj = JsonUtils.string2Object<StudentTest>(jsonStr);
            Log.Info("{}-{}", obj.a, obj.b);

            var array = new string[] {"a", "b"};
            var arrayJson = JsonMapper.ToJson(array);
            var arrayObj = JsonMapper.ToObject(arrayJson, typeof(string[]));
            Log.Info(arrayObj);
        }
    }
}