using Spring.Collection.Reference;

namespace Summer.Sound.Model
{
    /// <summary>
    /// 播放声音参数。
    /// </summary>
    public sealed class PlaySoundParams : IReference
    {
        private bool m_Referenced;
        private float m_Time;
        private bool m_MuteInSoundGroup;
        private bool m_Loop;
        private int m_Priority;
        private float m_VolumeInSoundGroup;
        private float m_FadeInSeconds;
        private float m_Pitch;
        private float m_PanStereo;
        private float m_SpatialBlend;
        private float m_MaxDistance;
        private float m_DopplerLevel;

        /// <summary>
        /// 初始化播放声音参数的新实例。
        /// </summary>
        public PlaySoundParams()
        {
            m_Referenced = false;
            m_Time = SoundConstant.DefaultTime;
            m_MuteInSoundGroup = SoundConstant.DefaultMute;
            m_Loop = SoundConstant.DefaultLoop;
            m_Priority = SoundConstant.DefaultPriority;
            m_VolumeInSoundGroup = SoundConstant.DefaultVolume;
            m_FadeInSeconds = SoundConstant.DefaultFadeInSeconds;
            m_Pitch = SoundConstant.DefaultPitch;
            m_PanStereo = SoundConstant.DefaultPanStereo;
            m_SpatialBlend = SoundConstant.DefaultSpatialBlend;
            m_MaxDistance = SoundConstant.DefaultMaxDistance;
            m_DopplerLevel = SoundConstant.DefaultDopplerLevel;
        }

        /// <summary>
        /// 获取或设置播放位置。
        /// </summary>
        public float Time
        {
            get
            {
                return m_Time;
            }
            set
            {
                m_Time = value;
            }
        }

        /// <summary>
        /// 获取或设置在声音组内是否静音。
        /// </summary>
        public bool MuteInSoundGroup
        {
            get
            {
                return m_MuteInSoundGroup;
            }
            set
            {
                m_MuteInSoundGroup = value;
            }
        }

        /// <summary>
        /// 获取或设置是否循环播放。
        /// </summary>
        public bool Loop
        {
            get
            {
                return m_Loop;
            }
            set
            {
                m_Loop = value;
            }
        }

        /// <summary>
        /// 获取或设置声音优先级。
        /// </summary>
        public int Priority
        {
            get
            {
                return m_Priority;
            }
            set
            {
                m_Priority = value;
            }
        }

        /// <summary>
        /// 获取或设置在声音组内音量大小。
        /// </summary>
        public float VolumeInSoundGroup
        {
            get
            {
                return m_VolumeInSoundGroup;
            }
            set
            {
                m_VolumeInSoundGroup = value;
            }
        }

        /// <summary>
        /// 获取或设置声音淡入时间，以秒为单位。
        /// </summary>
        public float FadeInSeconds
        {
            get
            {
                return m_FadeInSeconds;
            }
            set
            {
                m_FadeInSeconds = value;
            }
        }

        /// <summary>
        /// 获取或设置声音音调。
        /// </summary>
        public float Pitch
        {
            get
            {
                return m_Pitch;
            }
            set
            {
                m_Pitch = value;
            }
        }

        /// <summary>
        /// 获取或设置声音立体声声相。
        /// </summary>
        public float PanStereo
        {
            get
            {
                return m_PanStereo;
            }
            set
            {
                m_PanStereo = value;
            }
        }

        /// <summary>
        /// 获取或设置声音空间混合量。
        /// </summary>
        public float SpatialBlend
        {
            get
            {
                return m_SpatialBlend;
            }
            set
            {
                m_SpatialBlend = value;
            }
        }

        /// <summary>
        /// 获取或设置声音最大距离。
        /// </summary>
        public float MaxDistance
        {
            get
            {
                return m_MaxDistance;
            }
            set
            {
                m_MaxDistance = value;
            }
        }

        /// <summary>
        /// 获取或设置声音多普勒等级。
        /// </summary>
        public float DopplerLevel
        {
            get
            {
                return m_DopplerLevel;
            }
            set
            {
                m_DopplerLevel = value;
            }
        }

        public bool Referenced
        {
            get
            {
                return m_Referenced;
            }
        }

        /// <summary>
        /// 创建播放声音参数。
        /// </summary>
        /// <returns>创建的播放声音参数。</returns>
        public static PlaySoundParams Create()
        {
            PlaySoundParams playSoundParams = ReferenceCache.Acquire<PlaySoundParams>();
            playSoundParams.m_Referenced = true;
            return playSoundParams;
        }

        /// <summary>
        /// 清理播放声音参数。
        /// </summary>
        public void Clear()
        {
            m_Time = SoundConstant.DefaultTime;
            m_MuteInSoundGroup = SoundConstant.DefaultMute;
            m_Loop = SoundConstant.DefaultLoop;
            m_Priority = SoundConstant.DefaultPriority;
            m_VolumeInSoundGroup = SoundConstant.DefaultVolume;
            m_FadeInSeconds = SoundConstant.DefaultFadeInSeconds;
            m_Pitch = SoundConstant.DefaultPitch;
            m_PanStereo = SoundConstant.DefaultPanStereo;
            m_SpatialBlend = SoundConstant.DefaultSpatialBlend;
            m_MaxDistance = SoundConstant.DefaultMaxDistance;
            m_DopplerLevel = SoundConstant.DefaultDopplerLevel;
        }
    }
}
