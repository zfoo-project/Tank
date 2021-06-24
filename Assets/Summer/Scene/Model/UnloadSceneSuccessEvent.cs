using Spring.Collection.Reference;
using Spring.Event;

namespace Summer.Scene.Model
{
    /// <summary>
    /// 卸载场景成功事件。
    /// </summary>
    public sealed class UnloadSceneSuccessEvent : IEvent, IReference
    {
        /// <summary>
        /// 获取场景资源名称。
        /// </summary>
        public string sceneAssetName;

        /// <summary>
        /// 获取用户自定义数据。
        /// </summary>
        public object userData;

        /// <summary>
        /// 创建卸载场景成功事件。
        /// </summary>
        /// <param name="sceneAssetName">场景资源名称。</param>
        /// <param name="userData">用户自定义数据。</param>
        /// <returns>创建的卸载场景成功事件。</returns>
        public static UnloadSceneSuccessEvent ValueOf(string sceneAssetName, object userData)
        {
            var eve = ReferenceCache.Acquire<UnloadSceneSuccessEvent>();
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