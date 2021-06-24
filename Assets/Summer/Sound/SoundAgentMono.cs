using System;
using System.Collections;
using Summer.Resource;
using Summer.Sound.Model;
using Summer.Util;
using Spring.Core;
using UnityEngine;

namespace Summer.Sound
{
    /// <summary>
    /// 默认声音代理辅助器。
    /// </summary>
    public class SoundAgentMono : MonoBehaviour
    {
        public AudioSource audioSource;
        public SoundGroup soundGroup;

        private float volumeWhenPause;

        private int serialId;
        private object soundAsset;
        private DateTime setSoundAssetTime;
        private bool muteInSoundGroup;
        private float volumeInSoundGroup;

        /// <summary>
        /// 获取当前是否正在播放。
        /// </summary>
        public bool IsPlaying
        {
            get { return audioSource.isPlaying; }
        }


        /// <summary>
        /// 获取或设置播放位置。
        /// </summary>
        public float Time
        {
            get { return audioSource.time; }
            set { audioSource.time = value; }
        }

        /// <summary>
        /// 获取声音创建时间。
        /// </summary>
        public DateTime SetSoundAssetTime
        {
            get { return setSoundAssetTime; }
        }


        /// <summary>
        /// 获取或设置是否循环播放。
        /// </summary>
        public bool Loop
        {
            get { return audioSource.loop; }
            set { audioSource.loop = value; }
        }

        /// <summary>
        /// 获取或设置声音优先级。
        /// </summary>
        public int Priority
        {
            get { return 128 - audioSource.priority; }
            set { audioSource.priority = 128 - value; }
        }

        /// <summary>
        /// 获取或设置音量大小。
        /// </summary>
        public float Volume
        {
            get { return audioSource.volume; }
            set { audioSource.volume = value; }
        }

        /// <summary>
        /// 获取或设置在声音组内音量大小。
        /// </summary>
        public float VolumeInSoundGroup
        {
            get { return volumeInSoundGroup; }
            set
            {
                volumeInSoundGroup = value;
                RefreshVolume();
            }
        }

        /// <summary>
        /// 获取或设置声音音调。
        /// </summary>
        public float Pitch
        {
            get { return audioSource.pitch; }
            set { audioSource.pitch = value; }
        }

        /// <summary>
        /// 获取或设置声音立体声声相。
        /// </summary>
        public float PanStereo
        {
            get { return audioSource.panStereo; }
            set { audioSource.panStereo = value; }
        }

        /// <summary>
        /// 获取或设置声音的序列编号。
        /// </summary>
        public int SerialId
        {
            get { return serialId; }
            set { serialId = value; }
        }

        /// <summary>
        /// 获取或设置在声音组内是否静音。
        /// </summary>
        public bool MuteInSoundGroup
        {
            get { return muteInSoundGroup; }
            set
            {
                muteInSoundGroup = value;
                RefreshMute();
            }
        }

        /// <summary>
        /// 获取或设置声音空间混合量。
        /// </summary>
        public float SpatialBlend
        {
            get { return audioSource.spatialBlend; }
            set { audioSource.spatialBlend = value; }
        }

        /// <summary>
        /// 获取或设置声音最大距离。
        /// </summary>
        public float MaxDistance
        {
            get { return audioSource.maxDistance; }

            set { audioSource.maxDistance = value; }
        }

        /// <summary>
        /// 获取或设置声音多普勒等级。
        /// </summary>
        public float DopplerLevel
        {
            get { return audioSource.dopplerLevel; }
            set { audioSource.dopplerLevel = value; }
        }


        /// <summary>
        /// 播放声音。
        /// </summary>
        /// <param name="fadeInSeconds">声音淡入时间，以秒为单位。</param>
        public void Play(float fadeInSeconds)
        {
            StopAllCoroutines();

            audioSource.Play();
            if (fadeInSeconds > 0f)
            {
                float volume = audioSource.volume;
                audioSource.volume = 0f;
                StartCoroutine(FadeToVolume(audioSource, volume, fadeInSeconds));
            }
        }

        /// <summary>
        /// 停止播放声音。
        /// </summary>
        /// <param name="fadeOutSeconds">声音淡出时间，以秒为单位。</param>
        public void Stop(float fadeOutSeconds)
        {
            StopAllCoroutines();

            if (fadeOutSeconds > 0f && gameObject.activeInHierarchy)
            {
                StartCoroutine(StopCo(fadeOutSeconds));
            }
            else
            {
                audioSource.Stop();
                Reset();
            }
        }

        /// <summary>
        /// 暂停播放声音。
        /// </summary>
        /// <param name="fadeOutSeconds">声音淡出时间，以秒为单位。</param>
        public void Pause(float fadeOutSeconds)
        {
            StopAllCoroutines();

            volumeWhenPause = audioSource.volume;
            if (fadeOutSeconds > 0f && gameObject.activeInHierarchy)
            {
                StartCoroutine(PauseCo(fadeOutSeconds));
            }
            else
            {
                audioSource.Pause();
            }
        }

        /// <summary>
        /// 恢复播放声音。
        /// </summary>
        /// <param name="fadeInSeconds">声音淡入时间，以秒为单位。</param>
        public void Resume(float fadeInSeconds)
        {
            StopAllCoroutines();

            audioSource.UnPause();
            if (fadeInSeconds > 0f)
            {
                StartCoroutine(FadeToVolume(audioSource, volumeWhenPause, fadeInSeconds));
            }
            else
            {
                audioSource.volume = volumeWhenPause;
            }
        }

        /// <summary>
        /// 重置声音代理辅助器。
        /// </summary>
        public void Reset()
        {
            if (soundAsset != null)
            {
                SpringContext.GetBean<IResourceManager>().UnloadAsset(soundAsset);
                soundAsset = null;
            }

            setSoundAssetTime = DateTime.MinValue;
            Time = SoundConstant.DefaultTime;
            MuteInSoundGroup = SoundConstant.DefaultMute;
            Loop = SoundConstant.DefaultLoop;
            Priority = SoundConstant.DefaultPriority;
            VolumeInSoundGroup = SoundConstant.DefaultVolume;
            Pitch = SoundConstant.DefaultPitch;
            PanStereo = SoundConstant.DefaultPanStereo;
            SpatialBlend = SoundConstant.DefaultSpatialBlend;
            MaxDistance = SoundConstant.DefaultMaxDistance;
            DopplerLevel = SoundConstant.DefaultDopplerLevel;

            transform.localPosition = Vector3.zero;
            audioSource.clip = null;
            volumeWhenPause = 0f;
        }

        /// <summary>
        /// 设置声音资源。
        /// </summary>
        /// <param name="soundAsset">声音资源。</param>
        /// <returns>是否设置声音资源成功。</returns>
        public void SetSoundAsset(object soundAsset)
        {
            AudioClip audioClip = soundAsset as AudioClip;
            audioSource.clip = audioClip;
        }


        private void Awake()
        {
            audioSource = UnityUtils.GetOrAddComponent<AudioSource>(gameObject);
            audioSource.playOnAwake = false;
            audioSource.rolloffMode = AudioRolloffMode.Custom;
        }

        // Home出：
        // OnApplicationPause, isPause=True
        // OnApplicationFocus, isFocus=False
        // 
        // Home进：
        // OnApplicationPause, isPause=False
        // OnApplicationFocus, _isFocus=True
        private void OnApplicationPause(bool pause)
        {
        }


        private IEnumerator StopCo(float fadeOutSeconds)
        {
            yield return FadeToVolume(audioSource, 0f, fadeOutSeconds);
            audioSource.Stop();
            Reset();
        }

        private IEnumerator PauseCo(float fadeOutSeconds)
        {
            yield return FadeToVolume(audioSource, 0f, fadeOutSeconds);
            audioSource.Pause();
        }

        private IEnumerator FadeToVolume(AudioSource audioSource, float volume, float duration)
        {
            float time = 0f;
            float originalVolume = audioSource.volume;
            while (time < duration)
            {
                time += UnityEngine.Time.deltaTime;
                audioSource.volume = Mathf.Lerp(originalVolume, volume, time / duration);
                yield return new WaitForEndOfFrame();
            }

            audioSource.volume = volume;
        }

        public void RefreshMute()
        {
            audioSource.mute = soundGroup.Mute || muteInSoundGroup;
        }

        public void RefreshVolume()
        {
            audioSource.volume = soundGroup.Volume * volumeInSoundGroup;
        }
    }
}