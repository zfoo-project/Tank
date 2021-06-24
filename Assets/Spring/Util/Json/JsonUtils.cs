using System;

namespace Spring.Util.Json
{
    /// <summary>
    /// JSON 相关的实用函数。
    /// </summary>
    public abstract class JsonUtils
    {
        private static IJsonHelper jsonHelper = new LitJsonHelper();

        /// <summary>
        /// 设置 JSON 辅助器。
        /// </summary>
        /// <param name="jsonHelper">要设置的 JSON 辅助器。</param>
        public static void SetJsonHelper(IJsonHelper jsonHelper)
        {
            JsonUtils.jsonHelper = jsonHelper;
        }

        /// <summary>
        /// 将对象序列化为 JSON 字符串。
        /// </summary>
        /// <param name="obj">要序列化的对象。</param>
        /// <returns>序列化后的 JSON 字符串。</returns>
        public static string object2String(object obj)
        {
            try
            {
                return jsonHelper.ToJson(obj);
            }
            catch (Exception exception)
            {
                throw new Exception(
                    StringUtils.Format("Can not convert to JSON with exception '{}'.", exception.ToString()),
                    exception);
            }
        }

        /// <summary>
        /// 将 JSON 字符串反序列化为对象。
        /// </summary>
        /// <typeparam name="T">对象类型。</typeparam>
        /// <param name="json">要反序列化的 JSON 字符串。</param>
        /// <returns>反序列化后的对象。</returns>
        public static T string2Object<T>(string json)
        {
            try
            {
                return jsonHelper.ToObject<T>(json);
            }
            catch (Exception exception)
            {
                throw new Exception(
                    StringUtils.Format("Can not convert to object with exception '{}'.", exception.ToString()),
                    exception);
            }
        }

        public static object string2Object(string json, Type objectType)
        {
            try
            {
                return jsonHelper.ToObject(json, objectType);
            }
            catch (Exception exception)
            {
                throw new Exception(
                    StringUtils.Format("Can not convert to object with exception '{}'.", exception.ToString()),
                    exception);
            }
        }
    }
}