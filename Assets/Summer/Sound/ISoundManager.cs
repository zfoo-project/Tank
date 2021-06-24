using Summer.Resource;
using Summer.Resource.Model.Constant;
using Summer.Sound.Model;
using UnityEngine.Audio;

namespace Summer.Sound
{
    /// <summary>
    /// 声音管理器接口。
    /// </summary>
    public interface ISoundManager
    {
        int SoundGroupCount { get; }

        /// <summary>
        /// 是否存在指定声音组。
        /// </summary>
        /// <param name="soundGroupName">声音组名称。</param>
        /// <returns>指定声音组是否存在。</returns>
        bool HasSoundGroup(string soundGroupName);

        /// <summary>
        /// 增加声音组。
        /// </summary>
        /// <param name="soundGroupName">声音组名称。</param>
        /// <param name="soundGroupAvoidBeingReplacedBySamePriority">声音组中的声音是否避免被同优先级声音替换。</param>
        /// <param name="soundGroupMute">声音组是否静音。</param>
        /// <param name="soundGroupVolume">声音组音量。</param>
        /// <param name="soundGroupHelper">声音组辅助器。</param>
        /// <returns>是否增加声音组成功。</returns>
        bool AddSoundGroup(string soundGroupName, bool soundGroupAvoidBeingReplacedBySamePriority, bool soundGroupMute,
            float soundGroupVolume, AudioMixerGroup audioMixerGroup);

        /// <summary>
        /// 增加声音代理辅助器。
        /// </summary>
        /// <param name="soundGroupName">声音组名称。</param>
        /// <param name="soundAgentMono">要增加的声音代理辅助器。</param>
        void AddSoundAgentHelper(string soundGroupName, SoundAgentMono soundAgentMono);

        void StopAllLoadingSounds();
        void StopAllLoadedSounds();

        int PlaySound(string soundAssetName, string soundGroupName, int priority = ResourceConstant.DefaultPriority,
            PlaySoundParams playSoundParams = null, object userData = null);

        bool StopSound(int serialId, float fadeOutSeconds);

        SoundGroup GetSoundGroup(string soundGroupName);
    }
}