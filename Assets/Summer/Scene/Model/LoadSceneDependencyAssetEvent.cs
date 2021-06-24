using Spring.Collection.Reference;
using Spring.Event;

namespace Summer.Scene.Model
{
    /// <summary>
    /// 加载场景时加载依赖资源事件。
    /// </summary>
    public sealed class LoadSceneDependencyAssetEvent : IEvent, IReference
    {

        /// <summary>
        /// 获取场景资源名称。
        /// </summary>
        public string sceneAssetName;

        /// <summary>
        /// 获取被加载的依赖资源名称。
        /// </summary>
        public string dependencyAssetName;

        /// <summary>
        /// 获取当前已加载依赖资源数量。
        /// </summary>
        public int loadedCount;

        /// <summary>
        /// 获取总共加载依赖资源数量。
        /// </summary>
        public int totalCount;

        /// <summary>
        /// 获取用户自定义数据。
        /// </summary>
        public object userData;

        public static LoadSceneDependencyAssetEvent ValueOf(string sceneAssetName, string dependencyAssetName, int loadedCount, int totalCount, object userData)
        {
            var eve = ReferenceCache.Acquire<LoadSceneDependencyAssetEvent>();
            eve.sceneAssetName = sceneAssetName;
            eve.dependencyAssetName = dependencyAssetName;
            eve.loadedCount = loadedCount;
            eve.totalCount = totalCount;
            eve.userData = userData;
            return eve;
        }

        public void Clear()
        {
            sceneAssetName = null;
            dependencyAssetName = null;
            loadedCount = 0;
            totalCount = 0;
            userData = null;
        }
    }
}
