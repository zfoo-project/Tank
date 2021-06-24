using System.Collections.Generic;
using UnityEngine;

namespace Summer.I18n
{
    /// <summary>
    /// 本地化管理器接口。
    /// </summary>
    public interface II18nManager
    {

        SystemLanguage language
        {
            get;
            set;
        }
        object mainFontAsset { get; set; }

        List<SystemLanguage> GetSupportedLanguages();
        
        /// <summary>
        /// 根据字典主键获取字典内容字符串。
        /// </summary>
        /// <param name="key">字典主键。</param>
        /// <returns>要获取的字典内容字符串。</returns>
        string GetString(string key);

        string GetString(string key, object arg0);
        string GetString(string key, object arg0, object arg1);
        string GetString(string key, object arg0, object arg1, object arg2);
        string GetString(string key, params object[] args);
        
        
        /// <summary>
        /// 增加字典。
        /// </summary>
        /// <param name="key">字典主键。</param>
        /// <param name="value">字典内容。</param>
        /// <returns>是否增加字典成功。</returns>
        bool AddString(string key, string value);

        bool ParseSupportedLanguages(string dictionaryString);
        bool ParseData(string dictionaryString);
    }
}
