using System;
using System.Linq;
using Summer.Base;
using Summer.Sound.Model;
using Summer.Util;
using Spring.Core;
using Spring.Logger;
using Spring.Util;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;
using SpringComponent = Summer.Base.SpringComponent;

namespace Summer.Sound
{
    /// <summary>
    /// 声音组件。
    /// </summary>
    [DisallowMultipleComponent]
    [AddComponentMenu("Summer/Sound")]
    public sealed class SoundComponent : SpringComponent
    {

        [Autowired]
        private ISoundManager soundManager;

        private AudioListener audioListener;

        [SerializeField]
        private AudioMixer audioMixer;

        [SerializeField]
        private SoundGroupInfo[] soundGroups;


        [BeforePostConstruct]
        private void Init()
        {
            // 增加AudioListener监听组件
            audioListener = UnityUtils.GetOrAddComponent<AudioListener>(gameObject);
            
            AssertionUtils.NotNull(audioMixer, "Can not find audio mixer.");

            SceneManager.sceneLoaded += OnSceneLoaded;
            SceneManager.sceneUnloaded += OnSceneUnloaded;

            for (int i = 0; i < soundGroups.Length; i++)
            {
                if (!AddSoundGroup(soundGroups[i].Name, soundGroups[i].AvoidBeingReplacedBySamePriority, soundGroups[i].Mute, soundGroups[i].Volume, soundGroups[i].AgentHelperCount))
                {
                    Log.Warning("Add sound group '{}' failure.", soundGroups[i].Name);
                    continue;
                }
            }
        }


        /// <summary>
        /// 增加声音组。
        /// </summary>
        /// <param name="soundGroupName">声音组名称。</param>
        /// <param name="soundGroupAvoidBeingReplacedBySamePriority">声音组中的声音是否避免被同优先级声音替换。</param>
        /// <param name="soundGroupMute">声音组是否静音。</param>
        /// <param name="soundGroupVolume">声音组音量。</param>
        /// <param name="soundAgentHelperCount">声音代理辅助器数量。</param>
        /// <returns>是否增加声音组成功。</returns>
        public bool AddSoundGroup(string soundGroupName, bool soundGroupAvoidBeingReplacedBySamePriority, bool soundGroupMute, float soundGroupVolume, int soundAgentHelperCount)
        {
            if (soundManager.HasSoundGroup(soundGroupName))
            {
                return false;
            }

            var soundGroupObject = new GameObject();
            soundGroupObject.name = StringUtils.Format("Sound Group - {}", soundGroupName);
            soundGroupObject.transform.SetParent(gameObject.transform);
            soundGroupObject.transform.localScale = Vector3.one;


            AudioMixerGroup audioMixerGroup = audioMixer.FindMatchingGroups(StringUtils.Format("Master/{}", soundGroupName)).First();

            if (!soundManager.AddSoundGroup(soundGroupName, soundGroupAvoidBeingReplacedBySamePriority, soundGroupMute, soundGroupVolume, audioMixerGroup))
            {
                return false;
            }

            for (int i = 0; i < soundAgentHelperCount; i++)
            {
                if (!AddSoundAgentMono(soundGroupName, soundGroupObject, audioMixerGroup, i))
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// 增加声音代理辅助器。
        /// </summary>
        /// <param name="soundGroupName">声音组名称。</param>
        /// <param name="soundGroupHelper">声音组辅助器。</param>
        /// <param name="index">声音代理辅助器索引。</param>
        /// <returns>是否增加声音代理辅助器成功。</returns>
        private bool AddSoundAgentMono(string soundGroupName, GameObject soundGroupObject, AudioMixerGroup audioMixerGroup, int index)
        {
            var agentObject = new GameObject();
            var soundAgentMono = agentObject.AddComponent<SoundAgentMono>();
            agentObject.name = StringUtils.Format("Sound Agent Helper - {} - {}", soundGroupName, index.ToString());
            agentObject.transform.SetParent(soundGroupObject.transform);
            agentObject.transform.localScale = Vector3.one;

            AudioMixerGroup[] audioMixerGroups = audioMixer.FindMatchingGroups(StringUtils.Format("Master/{}/{}", soundGroupName, index.ToString()));
            if (audioMixerGroups.Length > 0)
            {
                soundAgentMono.audioSource.outputAudioMixerGroup = audioMixerGroups[0];
            }
            else
            {
                soundAgentMono.audioSource.outputAudioMixerGroup = audioMixerGroup;
            }

            soundManager.AddSoundAgentHelper(soundGroupName, soundAgentMono);

            return true;
        }

        private void OnDestroy()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            SceneManager.sceneUnloaded -= OnSceneUnloaded;
        }

        private void OnSceneLoaded(UnityEngine.SceneManagement.Scene scene, LoadSceneMode loadSceneMode)
        {
            RefreshAudioListener();
        }

        private void OnSceneUnloaded(UnityEngine.SceneManagement.Scene scene)
        {
            RefreshAudioListener();
        }

        private void RefreshAudioListener()
        {
            audioListener.enabled = FindObjectsOfType<AudioListener>().Length <= 1;
        }
        
    }
}