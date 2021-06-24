using System;
using MiniKing.Script.Config;
using MiniKing.Script.Constant;
using MiniKing.Script.Util;
using Spring.Core;
using Spring.Logger;
using Summer.I18n;
using Summer.Procedure;
using Summer.Setting;
using UnityEngine;

namespace MiniKing.Script.Procedure
{
    [Bean]
    public class ProcedureMain : FsmState<IProcedureFsmManager>
    {
        [Autowired]
        private ISettingManager settingManager;

        [Autowired]
        private II18nManager i18nManager;

        public override void OnEnter(IFsm<IProcedureFsmManager> fsm)
        {
            base.OnEnter(fsm);

            // 加载所有被支持的语言
            SpringContext.GetBean<ConfigComponent>().InitSupportedLanguages();

            // 语言配置：设置当前使用的语言
            InitLanguageSettings();

            // 声音配置：根据用户配置数据，设置即将使用的声音选项
            InitSoundSettings();

            // 初始化主要字体
            InitMainFontAsset();

            // 构建信息：发布版本时，把一些数据以 Json 的格式写入 Assets/MiniKing/Configs/Config_Dev.json，供游戏逻辑读取
            SpringContext.GetBean<ConfigComponent>().InitConfigInfo();

            // 默认字典：加载默认字典文件 Assets/MiniKing/Configs/language.xml
            // 此字典文件记录了资源更新前使用的各种语言的字符串，会随 App 一起发布，故不可更新
            SpringContext.GetBean<ConfigComponent>().InitI18nInfo();
        }

        public override void OnUpdate(IFsm<IProcedureFsmManager> fsm, float elapseSeconds, float realElapseSeconds)
        {
            base.OnUpdate(fsm, elapseSeconds, realElapseSeconds);

            // 运行一帧即切换到 Splash 展示流程
            fsm.ChangeState<ProcedureSplash>();
        }

        private void InitLanguageSettings()
        {
            // 如果没有任何设置，默认使用系统语言
            var language = Application.systemLanguage;

            // 如果这个语言没有被支持，则默认使用英语
            if (!i18nManager.GetSupportedLanguages().Contains(language))
            {
                language = SystemLanguage.English;
            }

            // 优先级最高的是使用自己设置过的语言
            if (settingManager.HasSetting(GameConstant.SETTING_LANGUAGE))
            {
                var languageStringSetting = String.Empty;
                try
                {
                    languageStringSetting = settingManager.GetString(GameConstant.SETTING_LANGUAGE);
                    language = (SystemLanguage) Enum.Parse(typeof(SystemLanguage), languageStringSetting);
                }
                catch
                {
                    Log.Error("初始化[{}]语言错误", languageStringSetting);
                }
            }

            i18nManager.language = language;
            Log.Info("Init language settings complete, current language is '{}'.", language.ToString());
        }

        private void InitSoundSettings()
        {
            SettingUtils.Mute(GameConstant.SOUND_GROUP_MUSIC, settingManager.GetBool(GameConstant.SETTING_MUSIC_MUTED, false));
            SettingUtils.SetVolume(GameConstant.SOUND_GROUP_MUSIC, settingManager.GetFloat(GameConstant.SETTING_MUSIC_VOLUME, 0.3f));
            SettingUtils.Mute(GameConstant.SOUND_GROUP_SOUND_EFFECT, settingManager.GetBool(GameConstant.SETTING_SOUND_EFFECT_MUTED, false));
            SettingUtils.SetVolume(GameConstant.SOUND_GROUP_SOUND_EFFECT, settingManager.GetFloat(GameConstant.SETTING_SOUND_EFFECT_VOLUME, 1f));
            SettingUtils.Mute(GameConstant.SOUND_GROUP_SOUND_UI, settingManager.GetBool(GameConstant.SETTING_SOUND_UI_MUTED, false));
            SettingUtils.SetVolume(GameConstant.SOUND_GROUP_SOUND_UI, settingManager.GetFloat(GameConstant.SETTING_SOUND_UI_VOLUME, 1f));
            Log.Info("Init sound settings complete.");
        }

        private void InitMainFontAsset()
        {
            var mainFontAsset = Resources.Load(AssetPathUtils.MAIN_FONT_ASSET_PATH);
            i18nManager.mainFontAsset = mainFontAsset;
        }
    }
}