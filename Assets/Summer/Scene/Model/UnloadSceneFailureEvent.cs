using Spring.Collection.Reference;
using Spring.Event;

namespace Summer.Scene.Model
{
    /// <summary>
    /// 卸载场景失败事件。
    /// </summary>
    public sealed class UnloadSceneFailureEvent : IEvent, IReference
    {
        /// <summary>
        /// 获取场景资源名称。
        /// </summary>
        public string sceneAssetName;

        /// <summary>
        /// 获取用户自定义数据。
        /// </summary>
        public object userData;

        public static UnloadSceneFailureEvent ValueOf(string sceneAssetName, object userData)
        {
            var eve = ReferenceCache.Acquire<UnloadSceneFailureEvent>();
            eve.sceneAssetName = sceneAssetName;
            eve.userData = userData;
            return eve;
        }

        public void Clear()
        {
            sceneAssetName = null;
            userData = null;
        }
    }
}