using Summer.Base;
using Spring.Core;
using Spring.Logger;
using Spring.Util.Json;
using Summer.I18n;
using UnityEngine;
using SpringComponent = Summer.Base.SpringComponent;

namespace MiniKing.Script.Config
{
    public class ConfigComponent : SpringComponent
    {
        [SerializeField]
        private TextAsset configTextAsset;

        [SerializeField]
        private TextAsset languageTextAsset;

        public ConfigInfo configInfo;

        public void InitConfigInfo()
        {
            if (configTextAsset == null || string.IsNullOrEmpty(configTextAsset.text))
            {
                Log.Info("Config info can not be found or empty.");
                return;
            }

            configInfo = JsonUtils.string2Object<ConfigInfo>(configTextAsset.text);
            if (configInfo == null)
            {
                Log.Warning("Parse config info failure.");
                return;
            }
        }

        public void InitSupportedLanguages()
        {
            if (languageTextAsset == null || string.IsNullOrEmpty(languageTextAsset.text))
            {
                Log.Info("Default dictionary can not be found or empty.");
                return;
            }
            
            if (!SpringContext.GetBean<II18nManager>().ParseSupportedLanguages(languageTextAsset.text))
            {
                Log.Warning("Parse supported language failure.");
                return;
            }
        }

        public void InitI18nInfo()
        {
            if (!SpringContext.GetBean<II18nManager>().ParseData(languageTextAsset.text))
            {
                Log.Warning("Parse default dictionary failure.");
                return;
            }
        }
    }
}