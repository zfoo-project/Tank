using MiniKing.Script.Constant;
using Spring.Core;
using Summer.Resource.Model.Constant;
using Summer.Sound;
using Summer.Sound.Model;

namespace MiniKing.Script.Util
{
    public abstract class MusicUtils
    {
        private static readonly float FadeVolumeDuration = 1f;

        private static int musicSerialId;

        public static void PlayMusic(string musicAsset)
        {
            StopMusic();
            
            var soundManager = SpringContext.GetBean<ISoundManager>();

            var playSoundParams = PlaySoundParams.Create();
            playSoundParams.Priority = 64;
            playSoundParams.Loop = true;
            playSoundParams.VolumeInSoundGroup = 1f;
            playSoundParams.FadeInSeconds = FadeVolumeDuration;
            playSoundParams.SpatialBlend = 0f;

            musicSerialId = soundManager.PlaySound(AssetPathUtils.GetMusicAsset(musicAsset), GameConstant.SOUND_GROUP_MUSIC, ResourceConstant.MusicAsset, playSoundParams, null);
        }

        public static void StopMusic()
        {
            if (musicSerialId <= 0)
            {
                return;
            }

            var soundManager = SpringContext.GetBean<ISoundManager>();
            soundManager.StopSound(musicSerialId, FadeVolumeDuration);
            musicSerialId = 0;
        }
    }
}