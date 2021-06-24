using Spring.Collection.Reference;
using Spring.Event;

namespace Summer.Scene.Model
{
    /// <summary>
    /// 加载场景更新事件。
    /// </summary>
    public sealed class LoadSceneUpdateEvent : IEvent, IReference
    {
        /// <summary>
        /// 获取场景资源名称。
        /// </summary>
        public string sceneAssetName;

        /// <summary>
        /// 获取加载场景进度。
        /// </summary>
        public float progress;

        /// <summary>
        /// 获取用户自定义数据。
        /// </summary>
        public object userData;

        public static LoadSceneUpdateEvent ValueOf(string sceneAssetName, float progress, object userData)
        {
            var eve = ReferenceCache.Acquire<LoadSceneUpdateEvent>();
            eve.sceneAssetName = sceneAssetName;
            eve.progress = progress;
            eve.userData = userData;
            return eve;
        }

        public void Clear()
        {
            sceneAssetName = null;
            progress = 0f;
            userData = null;
        }
    }
}