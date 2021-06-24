using System;
using System.Collections.Generic;
using Summer.Base.Model;
using Summer.Resource;
using Summer.Resource.Model.Callback;
using Summer.Resource.Model.Constant;
using Summer.Sound.Model;
using Spring.Collection.Reference;
using Spring.Core;
using Spring.Util;
using UnityEngine.Audio;

namespace Summer.Sound
{
    /// <summary>
    /// 声音管理器。
    /// </summary>
    [Bean]
    public sealed class SoundManager : AbstractManager, ISoundManager
    {
        [Autowired]
        private IResourceManager resourceManager;

        private readonly Dictionary<string, SoundGroup> soundGroups;
        private readonly List<int> soundsBeingLoaded;
        private readonly HashSet<int> soundsToReleaseOnLoad;
        private readonly LoadAssetCallbacks loadAssetCallbacks;

        private int serialIdCount;

        /// <summary>
        /// 初始化声音管理器的新实例。
        /// </summary>
        public SoundManager()
        {
            soundGroups = new Dictionary<string, SoundGroup>(StringComparer.Ordinal);
            soundsBeingLoaded = new List<int>();
            soundsToReleaseOnLoad = new HashSet<int>();
            loadAssetCallbacks = new LoadAssetCallbacks(LoadAssetSuccessCallback, LoadAssetFailureCallback, null, null);
            serialIdCount = 0;
        }

        /// <summary>
        /// 获取声音组数量。
        /// </summary>
        public int SoundGroupCount
        {
            get { return soundGroups.Count; }
        }


        /// <summary>
        /// 声音管理器轮询。
        /// </summary>
        /// <param name="elapseSeconds">逻辑流逝时间，以秒为单位。</param>
        /// <param name="realElapseSeconds">真实流逝时间，以秒为单位。</param>
        public override void Update(float elapseSeconds, float realElapseSeconds)
        {
        }

        /// <summary>
        /// 关闭并清理声音管理器。
        /// </summary>
        public override void Shutdown()
        {
            StopAllLoadedSounds();
            soundGroups.Clear();
            soundsBeingLoaded.Clear();
            soundsToReleaseOnLoad.Clear();
        }


        /// <summary>
        /// 是否存在指定声音组。
        /// </summary>
        /// <param name="soundGroupName">声音组名称。</param>
        /// <returns>指定声音组是否存在。</returns>
        public bool HasSoundGroup(string soundGroupName)
        {
            if (string.IsNullOrEmpty(soundGroupName))
            {
                throw new GameFrameworkException("Sound group name is invalid.");
            }

            return soundGroups.ContainsKey(soundGroupName);
        }

        /// <summary>
        /// 获取指定声音组。
        /// </summary>
        /// <param name="soundGroupName">声音组名称。</param>
        /// <returns>要获取的声音组。</returns>
        public SoundGroup GetSoundGroup(string soundGroupName)
        {
            if (string.IsNullOrEmpty(soundGroupName))
            {
                throw new GameFrameworkException("Sound group name is invalid.");
            }

            SoundGroup soundGroup = null;
            if (soundGroups.TryGetValue(soundGroupName, out soundGroup))
            {
                return soundGroup;
            }

            return null;
        }


        /// <summary>
        /// 增加声音组。
        /// </summary>
        /// <param name="soundGroupName">声音组名称。</param>
        /// <param name="soundGroupAvoidBeingReplacedBySamePriority">声音组中的声音是否避免被同优先级声音替换。</param>
        /// <param name="soundGroupMute">声音组是否静音。</param>
        /// <param name="soundGroupVolume">声音组音量。</param>
        /// <param name="soundGroupHelper">声音组辅助器。</param>
        /// <returns>是否增加声音组成功。</returns>
        public bool AddSoundGroup(string soundGroupName, bool soundGroupAvoidBeingReplacedBySamePriority, bool soundGroupMute, float soundGroupVolume, AudioMixerGroup audioMixerGroup)
        {
            if (string.IsNullOrEmpty(soundGroupName))
            {
                throw new GameFrameworkException("Sound group name is invalid.");
            }

            if (audioMixerGroup == null)
            {
                throw new GameFrameworkException("Sound group helper is invalid.");
            }

            if (HasSoundGroup(soundGroupName))
            {
                return false;
            }

            var soundGroup = new SoundGroup(soundGroupName, audioMixerGroup, soundGroupAvoidBeingReplacedBySamePriority, soundGroupMute, soundGroupVolume);

            soundGroups.Add(soundGroupName, soundGroup);

            return true;
        }

        /// <summary>
        /// 增加声音代理辅助器。
        /// </summary>
        /// <param name="soundGroupName">声音组名称。</param>
        /// <param name="soundAgentMono">要增加的声音代理辅助器。</param>
        public void AddSoundAgentHelper(string soundGroupName, SoundAgentMono soundAgentMono)
        {
            SoundGroup soundGroup = GetSoundGroup(soundGroupName);
            if (soundGroup == null)
            {
                throw new GameFrameworkException(StringUtils.Format("Sound group '{}' is not exist.", soundGroupName));
            }

            soundGroup.AddSoundAgentMono(soundAgentMono);
        }

        /// <summary>
        /// 获取所有正在加载声音的序列编号。
        /// </summary>
        /// <returns>所有正在加载声音的序列编号。</returns>
        public int[] GetAllLoadingSoundSerialIds()
        {
            return soundsBeingLoaded.ToArray();
        }

        /// <summary>
        /// 获取所有正在加载声音的序列编号。
        /// </summary>
        /// <param name="results">所有正在加载声音的序列编号。</param>
        public void GetAllLoadingSoundSerialIds(List<int> results)
        {
            if (results == null)
            {
                throw new GameFrameworkException("Results is invalid.");
            }

            results.Clear();
            results.AddRange(soundsBeingLoaded);
        }

        /// <summary>
        /// 是否正在加载声音。
        /// </summary>
        /// <param name="serialId">声音序列编号。</param>
        /// <returns>是否正在加载声音。</returns>
        public bool IsLoadingSound(int serialId)
        {
            return soundsBeingLoaded.Contains(serialId);
        }

        /// <summary>
        /// 播放声音。
        /// </summary>
        /// <param name="soundAssetName">声音资源名称。</param>
        /// <param name="soundGroupName">声音组名称。</param>
        /// <param name="priority">加载声音资源的优先级。</param>
        /// <returns>声音的序列编号。</returns>
        public int PlaySound(string soundAssetName, string soundGroupName, int priority)
        {
            return PlaySound(soundAssetName, soundGroupName, priority, null, null);
        }

        /// <summary>
        /// 播放声音。
        /// </summary>
        /// <param name="soundAssetName">声音资源名称。</param>
        /// <param name="soundGroupName">声音组名称。</param>
        /// <param name="playSoundParams">播放声音参数。</param>
        /// <returns>声音的序列编号。</returns>
        public int PlaySound(string soundAssetName, string soundGroupName, PlaySoundParams playSoundParams)
        {
            return PlaySound(soundAssetName, soundGroupName, ResourceConstant.DefaultPriority, playSoundParams, null);
        }

        /// <summary>
        /// 播放声音。
        /// </summary>
        /// <param name="soundAssetName">声音资源名称。</param>
        /// <param name="soundGroupName">声音组名称。</param>
        /// <param name="userData">用户自定义数据。</param>
        /// <returns>声音的序列编号。</returns>
        public int PlaySound(string soundAssetName, string soundGroupName, object userData)
        {
            return PlaySound(soundAssetName, soundGroupName, ResourceConstant.DefaultPriority, null, userData);
        }

        /// <summary>
        /// 播放声音。
        /// </summary>
        /// <param name="soundAssetName">声音资源名称。</param>
        /// <param name="soundGroupName">声音组名称。</param>
        /// <param name="priority">加载声音资源的优先级。</param>
        /// <param name="playSoundParams">播放声音参数。</param>
        /// <returns>声音的序列编号。</returns>
        public int PlaySound(string soundAssetName, string soundGroupName, int priority, PlaySoundParams playSoundParams)
        {
            return PlaySound(soundAssetName, soundGroupName, priority, playSoundParams, null);
        }

        /// <summary>
        /// 播放声音。
        /// </summary>
        /// <param name="soundAssetName">声音资源名称。</param>
        /// <param name="soundGroupName">声音组名称。</param>
        /// <param name="priority">加载声音资源的优先级。</param>
        /// <param name="userData">用户自定义数据。</param>
        /// <returns>声音的序列编号。</returns>
        public int PlaySound(string soundAssetName, string soundGroupName, int priority, object userData)
        {
            return PlaySound(soundAssetName, soundGroupName, priority, null, userData);
        }

        /// <summary>
        /// 播放声音。
        /// </summary>
        /// <param name="soundAssetName">声音资源名称。</param>
        /// <param name="soundGroupName">声音组名称。</param>
        /// <param name="playSoundParams">播放声音参数。</param>
        /// <param name="userData">用户自定义数据。</param>
        /// <returns>声音的序列编号。</returns>
        public int PlaySound(string soundAssetName, string soundGroupName, PlaySoundParams playSoundParams, object userData)
        {
            return PlaySound(soundAssetName, soundGroupName, ResourceConstant.DefaultPriority, playSoundParams, userData);
        }

        /// <summary>
        /// 播放声音。
        /// </summary>
        /// <param name="soundAssetName">声音资源名称。</param>
        /// <param name="soundGroupName">声音组名称。</param>
        /// <param name="priority">加载声音资源的优先级。</param>
        /// <param name="playSoundParams">播放声音参数。</param>
        /// <param name="userData">用户自定义数据。</param>
        /// <returns>声音的序列编号。</returns>
        public int PlaySound(string soundAssetName, string soundGroupName,
            int priority = ResourceConstant.DefaultPriority, PlaySoundParams playSoundParams = null, object userData = null)
        {
            if (resourceManager == null)
            {
                throw new GameFrameworkException("You must set resource manager first.");
            }

            if (playSoundParams == null)
            {
                playSoundParams = PlaySoundParams.Create();
            }

            int serialId = ++serialIdCount;
            var soundGroup = GetSoundGroup(soundGroupName);
            if (soundGroup == null)
            {
                throw new Exception(StringUtils.Format("Sound group '{}' is not exist.", soundGroupName));
            }

            if (soundGroup.SoundAgentCount <= 0)
            {
                throw new Exception(StringUtils.Format("Sound group '{}' is have no sound agent.", soundGroupName));
            }

            soundsBeingLoaded.Add(serialId);
            resourceManager.LoadAsset(soundAssetName, priority, loadAssetCallbacks, PlaySoundInfo.Create(serialId, soundGroup, playSoundParams, userData));
            return serialId;
        }

        /// <summary>
        /// 停止播放声音。
        /// </summary>
        /// <param name="serialId">要停止播放声音的序列编号。</param>
        /// <returns>是否停止播放声音成功。</returns>
        public bool StopSound(int serialId)
        {
            return StopSound(serialId, SoundConstant.DefaultFadeOutSeconds);
        }

        /// <summary>
        /// 停止播放声音。
        /// </summary>
        /// <param name="serialId">要停止播放声音的序列编号。</param>
        /// <param name="fadeOutSeconds">声音淡出时间，以秒为单位。</param>
        /// <returns>是否停止播放声音成功。</returns>
        public bool StopSound(int serialId, float fadeOutSeconds)
        {
            if (IsLoadingSound(serialId))
            {
                soundsToReleaseOnLoad.Add(serialId);
                soundsBeingLoaded.Remove(serialId);
                return true;
            }

            foreach (KeyValuePair<string, SoundGroup> soundGroup in soundGroups)
            {
                if (soundGroup.Value.StopSound(serialId, fadeOutSeconds))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// 停止所有已加载的声音。
        /// </summary>
        public void StopAllLoadedSounds()
        {
            StopAllLoadedSounds(SoundConstant.DefaultFadeOutSeconds);
        }

        /// <summary>
        /// 停止所有已加载的声音。
        /// </summary>
        /// <param name="fadeOutSeconds">声音淡出时间，以秒为单位。</param>
        public void StopAllLoadedSounds(float fadeOutSeconds)
        {
            foreach (KeyValuePair<string, SoundGroup> soundGroup in soundGroups)
            {
                soundGroup.Value.StopAllLoadedSounds(fadeOutSeconds);
            }
        }

        /// <summary>
        /// 停止所有正在加载的声音。
        /// </summary>
        public void StopAllLoadingSounds()
        {
            foreach (int serialId in soundsBeingLoaded)
            {
                soundsToReleaseOnLoad.Add(serialId);
            }
        }

        /// <summary>
        /// 暂停播放声音。
        /// </summary>
        /// <param name="serialId">要暂停播放声音的序列编号。</param>
        public void PauseSound(int serialId)
        {
            PauseSound(serialId, SoundConstant.DefaultFadeOutSeconds);
        }

        /// <summary>
        /// 暂停播放声音。
        /// </summary>
        /// <param name="serialId">要暂停播放声音的序列编号。</param>
        /// <param name="fadeOutSeconds">声音淡出时间，以秒为单位。</param>
        public void PauseSound(int serialId, float fadeOutSeconds)
        {
            foreach (KeyValuePair<string, SoundGroup> soundGroup in soundGroups)
            {
                if (soundGroup.Value.PauseSound(serialId, fadeOutSeconds))
                {
                    return;
                }
            }

            throw new GameFrameworkException(StringUtils.Format("Can not find sound '{}'.", serialId.ToString()));
        }

        /// <summary>
        /// 恢复播放声音。
        /// </summary>
        /// <param name="serialId">要恢复播放声音的序列编号。</param>
        public void ResumeSound(int serialId)
        {
            ResumeSound(serialId, SoundConstant.DefaultFadeInSeconds);
        }

        /// <summary>
        /// 恢复播放声音。
        /// </summary>
        /// <param name="serialId">要恢复播放声音的序列编号。</param>
        /// <param name="fadeInSeconds">声音淡入时间，以秒为单位。</param>
        public void ResumeSound(int serialId, float fadeInSeconds)
        {
            foreach (KeyValuePair<string, SoundGroup> soundGroup in soundGroups)
            {
                if (soundGroup.Value.ResumeSound(serialId, fadeInSeconds))
                {
                    return;
                }
            }

            throw new GameFrameworkException(StringUtils.Format("Can not find sound '{}'.", serialId.ToString()));
        }

        private void LoadAssetSuccessCallback(string soundAssetName, object soundAsset, float duration, object userData)
        {
            var playSoundInfo = (PlaySoundInfo) userData;
            if (playSoundInfo == null)
            {
                throw new GameFrameworkException("Play sound info is invalid.");
            }

            if (soundsToReleaseOnLoad.Contains(playSoundInfo.SerialId))
            {
                soundsToReleaseOnLoad.Remove(playSoundInfo.SerialId);
                if (playSoundInfo.PlaySoundParams.Referenced)
                {
                    ReferenceCache.Release(playSoundInfo.PlaySoundParams);
                }

                ReferenceCache.Release(playSoundInfo);
                resourceManager.UnloadAsset(soundAsset);
                return;
            }

            soundsBeingLoaded.Remove(playSoundInfo.SerialId);

            var soundAgent = playSoundInfo.SoundGroup.PlaySound(playSoundInfo.SerialId, soundAsset, playSoundInfo.PlaySoundParams);
            if (soundAgent != null)
            {
                if (playSoundInfo.PlaySoundParams.Referenced)
                {
                    ReferenceCache.Release(playSoundInfo.PlaySoundParams);
                }

                ReferenceCache.Release(playSoundInfo);
                return;
            }

            soundsToReleaseOnLoad.Remove(playSoundInfo.SerialId);
            resourceManager.UnloadAsset(soundAsset);
            var errorMessage = StringUtils.Format("Sound group '{}' play sound '{}' failure.", playSoundInfo.SoundGroup.Name, soundAssetName);

            if (playSoundInfo.PlaySoundParams.Referenced)
            {
                ReferenceCache.Release(playSoundInfo.PlaySoundParams);
            }

            ReferenceCache.Release(playSoundInfo);
            throw new GameFrameworkException(errorMessage);
        }

        private void LoadAssetFailureCallback(string soundAssetName, LoadResourceStatus status, string errorMessage, object userData)
        {
            PlaySoundInfo playSoundInfo = (PlaySoundInfo) userData;
            if (playSoundInfo == null)
            {
                throw new GameFrameworkException("Play sound info is invalid.");
            }

            if (soundsToReleaseOnLoad.Contains(playSoundInfo.SerialId))
            {
                soundsToReleaseOnLoad.Remove(playSoundInfo.SerialId);
                if (playSoundInfo.PlaySoundParams.Referenced)
                {
                    ReferenceCache.Release(playSoundInfo.PlaySoundParams);
                }

                return;
            }

            soundsBeingLoaded.Remove(playSoundInfo.SerialId);
            string appendErrorMessage = StringUtils.Format("Load sound failure, asset name '{}', status '{}', error message '{}'.",
                    soundAssetName, status.ToString(), errorMessage);
            throw new GameFrameworkException(appendErrorMessage);
        }
    }
}