using Summer.Setting;
using Summer.Sound;
using MiniKing.Script.Constant;
using Spring.Core;
using Spring.Util;

namespace MiniKing.Script.Util
{
    public abstract class SettingUtils
    {
        public static void Mute(string soundGroupName, bool mute)
        {
            var soundManager = SpringContext.GetBean<ISoundManager>();
            var soundGroup = soundManager.GetSoundGroup(soundGroupName);
            soundGroup.Mute = mute;

            var settingManager = SpringContext.GetBean<ISettingManager>();
            settingManager.SetBool(StringUtils.Format(GameConstant.SETTING_SOUND_GROUP_MUTED, soundGroupName), mute);
            settingManager.Save();
        }


        public static void SetVolume(string soundGroupName, float volume)
        {
            var soundManager = SpringContext.GetBean<ISoundManager>();
            var soundGroup = soundManager.GetSoundGroup(soundGroupName);
            soundGroup.Volume = volume;

            var settingManager = SpringContext.GetBean<ISettingManager>();
            settingManager.SetFloat(StringUtils.Format(GameConstant.SETTING_SOUND_GROUP_VOLUME, soundGroupName), volume);
            settingManager.Save();
        }
    }
}