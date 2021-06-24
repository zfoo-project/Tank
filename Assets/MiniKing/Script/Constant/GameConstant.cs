using Spring.Util;

namespace MiniKing.Script.Constant
{
    public abstract class GameConstant
    {
        /**
         * 设置相关配置
         */
        public static readonly string SETTING_LANGUAGE = "Setting.Language";

        public static readonly string SETTING_SOUND_GROUP_MUTED = "Setting.{}Muted";
        public static readonly string SETTING_SOUND_GROUP_VOLUME = "Setting.{}Volume";

        public static readonly string SETTING_MUSIC_MUTED = "Setting.MusicMuted";
        public static readonly string SETTING_MUSIC_VOLUME = "Setting.MusicVolume";

        public static readonly string SETTING_SOUND_EFFECT_MUTED = "Setting.SoundEffectMuted";
        public static readonly string SETTING_SOUND_EFFECT_VOLUME = "Setting.SoundEffectVolume";

        public static readonly string SETTING_SOUND_UI_MUTED = "Setting.SoundUIMuted";
        public static readonly string SETTING_SOUND_UI_VOLUME = "Setting.SoundUIVolume";


        public static readonly string SETTING_LOGIN_TOKEN = "Setting.LoginToken";


        /**
         * 声音组
         */
        public static readonly string SOUND_GROUP_MUSIC = "Music";

        public static readonly string SOUND_GROUP_SOUND_EFFECT = "SoundEffect";
        public static readonly string SOUND_GROUP_SOUND_UI = "SoundUI";

        /**
         * 热更新相关
         */
        public static readonly string VERSION_INFO = "VersionInfo";

        public static readonly string UPDATE_RESOURCE_INFO = "UpdateResourceCount";


        // 为了防止快速点击，这个是按钮最大点击的间隔
        public static readonly long CLICK_INTERVAL = 5 * TimeUtils.NANO_PER_SECOND;
    }
}