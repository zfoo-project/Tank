using System;
using System.Text;
using LitJson;

namespace Spring.Util.Json
{
    /// <summary>
    /// LitJSON 函数集辅助器。
    /// </summary>
    public class LitJsonHelper : IJsonHelper
    {
        /// <summary>
        /// 将对象序列化为 JSON 字符串。
        /// </summary>
        /// <param name="obj">要序列化的对象。</param>
        /// <returns>序列化后的 JSON 字符串。</returns>
        public string ToJson(object obj)
        {
            var sb = new StringBuilder();
            var jr = new JsonWriter(sb);
            // 设置为格式化模式，LitJson称其为PrettyPrint（美观的打印），在 Newtonsoft.Json里面则是 Formatting.Indented（锯齿状格式）
            jr.PrettyPrint = true;
            // 缩进空格个数
            jr.IndentValue = 4;
            JsonMapper.ToJson(obj, jr);
            return sb.ToString();
        }

        /// <summary>
        /// 将 JSON 字符串反序列化为对象。
        /// </summary>
        /// <typeparam name="T">对象类型。</typeparam>
        /// <param name="json">要反序列化的 JSON 字符串。</param>
        /// <returns>反序列化后的对象。</returns>
        public T ToObject<T>(string json)
        {
            return JsonMapper.ToObject<T>(json);
        }

        public object ToObject(string json, Type objectType)
        {
            return JsonMapper.ToObject(json, objectType);
        }
    }
}