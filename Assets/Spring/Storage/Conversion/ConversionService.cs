using System;
using System.Collections.Generic;
using Spring.Core;
using Spring.Util;
using Spring.Util.Json;

namespace Spring.Storage.Conversion
{
    [Bean]
    public class ConversionService : IConversionService
    {
        private static readonly Dictionary<Type, Func<string, Type, object>> conversionMap = new Dictionary<Type, Func<string, Type, object>>();

        static ConversionService()
        {
            conversionMap[typeof(bool)] = (it, type) => bool.Parse(it);
            conversionMap[typeof(byte)] = (it, type) => byte.Parse(it);
            conversionMap[typeof(short)] = (it, type) => short.Parse(it);
            conversionMap[typeof(int)] = (it, type) => int.Parse(it);
            conversionMap[typeof(long)] = (it, type) => long.Parse(it);
            conversionMap[typeof(float)] = (it, type) => float.Parse(it);
            conversionMap[typeof(double)] = (it, type) => double.Parse(it);
            conversionMap[typeof(string)] = (it, type) => it;
        }

        public object Convert(string value, Type targetType)
        {
            Func<string, Type, object> func = null;
            conversionMap.TryGetValue(targetType, out func);
            if (func == null)
            {
                if (targetType.IsArray || typeof(object).IsAssignableFrom(targetType))
                {
                    return JsonUtils.string2Object(value, targetType);
                }

                throw new Exception(StringUtils.Format("无法找到[{}][{}]的转换策略", value, targetType.Name));
            }

            return func(value, targetType);
        }
    }
}