using System;
using System.Collections.Generic;
using System.Xml;
using Spring.Core;
using Spring.Logger;
using Spring.Util;
using Summer.Base.Model;
using UnityEngine;

namespace Summer.I18n
{
    /// <summary>
    /// 本地化管理器。
    /// </summary>
    [Bean]
    public sealed class I18nManager : II18nManager
    {
        private static readonly string LANGUAGES_TAG = "languages";
        private static readonly string LANGUAGE_TAG = "language";
        private static readonly string LANGUAGE_ATTRIBUTE = "name";
        private static readonly string PAIR_TAG = "pair";
        private static readonly string PAIR_KEY = "key";
        private static readonly string PAIR_VALUE = "value";

        private readonly Dictionary<string, string> dictionary = new Dictionary<string, string>(StringComparer.Ordinal);

        private readonly List<SystemLanguage> supportedLanguages = new List<SystemLanguage>();

        public SystemLanguage language { get; set; }


        public object mainFontAsset { get; set; }

        public List<SystemLanguage> GetSupportedLanguages()
        {
            return supportedLanguages;
        }

        /// <summary>
        /// 根据字典主键获取字典内容字符串。
        /// </summary>
        /// <param name="key">字典主键。</param>
        /// <returns>要获取的字典内容字符串。</returns>
        public string GetString(string key)
        {
            string value = GetValue(key);
            if (value == null)
            {
                return StringUtils.Format("<NoKey>{}", key);
            }

            return value;
        }

        /// <summary>
        /// 根据字典主键获取字典内容字符串。
        /// </summary>
        /// <param name="key">字典主键。</param>
        /// <param name="arg0">字典参数 0。</param>
        /// <returns>要获取的字典内容字符串。</returns>
        public string GetString(string key, object arg0)
        {
            string value = GetValue(key);
            if (value == null)
            {
                return StringUtils.Format("<NoKey>{}", key);
            }

            try
            {
                return StringUtils.Format(value, arg0);
            }
            catch (Exception exception)
            {
                return StringUtils.Format("<Error>{},{},{},{}", key, value, arg0, exception.ToString());
            }
        }

        /// <summary>
        /// 根据字典主键获取字典内容字符串。
        /// </summary>
        /// <param name="key">字典主键。</param>
        /// <param name="arg0">字典参数 0。</param>
        /// <param name="arg1">字典参数 1。</param>
        /// <returns>要获取的字典内容字符串。</returns>
        public string GetString(string key, object arg0, object arg1)
        {
            string value = GetValue(key);
            if (value == null)
            {
                return StringUtils.Format("<NoKey>{}", key);
            }

            try
            {
                return StringUtils.Format(value, arg0, arg1);
            }
            catch (Exception exception)
            {
                return StringUtils.Format("<Error>{},{},{},{},{}", key, value, arg0, arg1, exception.ToString());
            }
        }

        /// <summary>
        /// 根据字典主键获取字典内容字符串。
        /// </summary>
        /// <param name="key">字典主键。</param>
        /// <param name="arg0">字典参数 0。</param>
        /// <param name="arg1">字典参数 1。</param>
        /// <param name="arg2">字典参数 2。</param>
        /// <returns>要获取的字典内容字符串。</returns>
        public string GetString(string key, object arg0, object arg1, object arg2)
        {
            string value = GetValue(key);
            if (value == null)
            {
                return StringUtils.Format("<NoKey>{}", key);
            }

            try
            {
                return StringUtils.Format(value, arg0, arg1, arg2);
            }
            catch (Exception exception)
            {
                return StringUtils.Format("<Error>{},{},{},{},{},{}", key, value, arg0, arg1, arg2,
                    exception.ToString());
            }
        }

        /// <summary>
        /// 根据字典主键获取字典内容字符串。
        /// </summary>
        /// <param name="key">字典主键。</param>
        /// <param name="args">字典参数。</param>
        /// <returns>要获取的字典内容字符串。</returns>
        public string GetString(string key, params object[] args)
        {
            string value = GetValue(key);
            if (value == null)
            {
                return StringUtils.Format("<NoKey>{}", key);
            }

            try
            {
                return StringUtils.Format(value, args);
            }
            catch (Exception exception)
            {
                string errorString = StringUtils.Format("<Error>{},{}", key, value);
                if (args != null)
                {
                    foreach (object arg in args)
                    {
                        errorString += "," + arg.ToString();
                    }
                }

                errorString += "," + exception.ToString();
                return errorString;
            }
        }

        /// <summary>
        /// 根据字典主键获取字典值。
        /// </summary>
        /// <param name="key">字典主键。</param>
        /// <returns>字典值。</returns>
        public string GetValue(string key)
        {
            if (string.IsNullOrEmpty(key))
            {
                throw new GameFrameworkException("Key is invalid.");
            }

            string value = null;
            if (!dictionary.TryGetValue(key, out value))
            {
                return null;
            }

            return value;
        }

        /// <summary>
        /// 增加字典。
        /// </summary>
        /// <param name="key">字典主键。</param>
        /// <param name="value">字典内容。</param>
        /// <returns>是否增加字典成功。</returns>
        public bool AddString(string key, string value)
        {
            if (string.IsNullOrEmpty(key))
            {
                throw new GameFrameworkException("Key is invalid.");
            }

            if (dictionary.ContainsKey(key))
            {
                return false;
            }

            dictionary.Add(key, value ?? string.Empty);
            return true;
        }

        public bool ParseSupportedLanguages(string dictionaryString)
        {
            try
            {
                var xmlDocument = new XmlDocument();
                xmlDocument.LoadXml(dictionaryString);
                var xmlRoot = xmlDocument.SelectSingleNode(LANGUAGES_TAG);
                var xmlNodeDictionaryList = xmlRoot.ChildNodes;
                for (var i = 0; i < xmlNodeDictionaryList.Count; i++)
                {
                    var xmlNodeDictionary = xmlNodeDictionaryList.Item(i);
                    if (xmlNodeDictionary.Name != LANGUAGE_TAG)
                    {
                        continue;
                    }

                    var xmlLanguage = xmlNodeDictionary.Attributes.GetNamedItem(LANGUAGE_ATTRIBUTE).Value;
                    var xmlLanguageEnum = (SystemLanguage) Enum.Parse(typeof(SystemLanguage), xmlLanguage);
                    supportedLanguages.Add(xmlLanguageEnum);
                }

                // 检查配置是否有序，不同语言之间的key是否完全一致
                var keyLanguage = SystemLanguage.English;
                var keyList = new List<string>();
                for (var i = 0; i < xmlNodeDictionaryList.Count; i++)
                {
                    var xmlNodeDictionary = xmlNodeDictionaryList.Item(i);
                    var xmlNodeStringList = xmlNodeDictionary.ChildNodes;
                    var xmlLanguage = xmlNodeDictionary.Attributes.GetNamedItem(LANGUAGE_ATTRIBUTE).Value;
                    keyLanguage = (SystemLanguage) Enum.Parse(typeof(SystemLanguage), xmlLanguage);
                    for (int j = 0; j < xmlNodeStringList.Count; j++)
                    {
                        var xmlNodeString = xmlNodeStringList.Item(j);
                        if (xmlNodeString.Name != PAIR_TAG)
                        {
                            continue;
                        }

                        var key = xmlNodeString.Attributes.GetNamedItem(PAIR_KEY).Value;
                        keyList.Add(key);
                    }
                }

                for (var i = 0; i < xmlNodeDictionaryList.Count; i++)
                {
                    var xmlNodeDictionary = xmlNodeDictionaryList.Item(i);
                    var xmlNodeStringList = xmlNodeDictionary.ChildNodes;
                    var xmlLanguage = xmlNodeDictionary.Attributes.GetNamedItem(LANGUAGE_ATTRIBUTE).Value;
                    for (int j = 0; j < xmlNodeStringList.Count; j++)
                    {
                        var xmlNodeString = xmlNodeStringList.Item(j);
                        if (xmlNodeString.Name != PAIR_TAG)
                        {
                            continue;
                        }

                        var key = xmlNodeString.Attributes.GetNamedItem(PAIR_KEY).Value;
                        if (!key.Equals(keyList[j]))
                        {
                            throw new Exception(StringUtils.Format("I18n.xml配置不合法，[language:{}][key:{}][language:{}][key:{}]"
                                , keyLanguage, keyList[j], xmlLanguage, key));
                        }
                    }
                }

                return true;
            }
            catch (Exception exception)
            {
                Log.Warning("Can not parse dictionary data with exception '{}'.", exception.ToString());
                return false;
            }
        }


        /// <summary>
        /// 解析字典。
        /// </summary>
        /// <param name="dictionaryString">要解析的字典字符串。</param>
        /// <returns>是否解析字典成功。</returns>
        public bool ParseData(string dictionaryString)
        {
            try
            {
                var xmlDocument = new XmlDocument();
                xmlDocument.LoadXml(dictionaryString);
                var xmlRoot = xmlDocument.SelectSingleNode(LANGUAGES_TAG);
                var xmlNodeDictionaryList = xmlRoot.ChildNodes;
                for (var i = 0; i < xmlNodeDictionaryList.Count; i++)
                {
                    var xmlNodeDictionary = xmlNodeDictionaryList.Item(i);
                    if (xmlNodeDictionary.Name != LANGUAGE_TAG)
                    {
                        continue;
                    }

                    var xmlLanguage = xmlNodeDictionary.Attributes.GetNamedItem(LANGUAGE_ATTRIBUTE).Value;
                    var xmlLanguageEnum = (SystemLanguage) Enum.Parse(typeof(SystemLanguage), xmlLanguage);
                    if (xmlLanguageEnum != language)
                    {
                        continue;
                    }

                    var xmlNodeStringList = xmlNodeDictionary.ChildNodes;
                    for (int j = 0; j < xmlNodeStringList.Count; j++)
                    {
                        var xmlNodeString = xmlNodeStringList.Item(j);
                        if (xmlNodeString.Name != PAIR_TAG)
                        {
                            continue;
                        }

                        string key = xmlNodeString.Attributes.GetNamedItem(PAIR_KEY).Value;
                        // unity把\n偷偷变成了\\n，所以要把它变回来才能实现换行
                        string value = xmlNodeString.Attributes.GetNamedItem(PAIR_VALUE).Value.Replace ("\\n", "\n");
                        if (!AddString(key, value))
                        {
                            Log.Warning("Can not add raw string with key '{}' which may be invalid or duplicate.", key);
                            return false;
                        }
                    }
                }

                return true;
            }
            catch (Exception exception)
            {
                Log.Warning("Can not parse dictionary data with exception '{}'.", exception.ToString());
                return false;
            }
        }
    }
}