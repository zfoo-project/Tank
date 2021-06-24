using Summer.Resource.Model.Callback;
using Summer.Resource.Model.Constant;
using Summer.Resource.Model.Vo;
using Spring.Collection.Reference;

namespace Summer.Resource.Model.Loader
{
    public sealed class LoadSceneTask : LoadResourceTaskBase
    {
        private LoadSceneCallbacks loadSceneCallbacks;


        public override bool IsScene
        {
            get { return true; }
        }

        public static LoadSceneTask Create(string sceneAssetName, int priority, ResourceInfo resourceInfo,
            string[] dependencyAssetNames, LoadSceneCallbacks loadSceneCallbacks, object userData)
        {
            LoadSceneTask loadSceneTask = ReferenceCache.Acquire<LoadSceneTask>();
            loadSceneTask.Initialize(sceneAssetName, null, priority, resourceInfo, dependencyAssetNames, userData);
            loadSceneTask.loadSceneCallbacks = loadSceneCallbacks;
            return loadSceneTask;
        }

        public override void Clear()
        {
            base.Clear();
            loadSceneCallbacks = null;
        }

        public override void OnLoadAssetSuccess(LoadResourceTaskAgent taskAgent, object asset, float duration)
        {
            base.OnLoadAssetSuccess(taskAgent, asset, duration);
            if (loadSceneCallbacks.LoadSceneSuccessCallback != null)
            {
                loadSceneCallbacks.LoadSceneSuccessCallback(AssetName, duration, UserData);
            }
        }

        public override void OnLoadAssetFailure(LoadResourceTaskAgent taskAgent, LoadResourceStatus status, string errorMessage)
        {
            base.OnLoadAssetFailure(taskAgent, status, errorMessage);
            if (loadSceneCallbacks.LoadSceneFailureCallback != null)
            {
                loadSceneCallbacks.LoadSceneFailureCallback(AssetName, status, errorMessage, UserData);
            }
        }

        public override void OnLoadAssetUpdate(LoadResourceTaskAgent taskAgent, LoadResourceProgress type, float progress)
        {
            base.OnLoadAssetUpdate(taskAgent, type, progress);
            if (type == LoadResourceProgress.LoadScene)
            {
                if (loadSceneCallbacks.LoadSceneUpdateCallback != null)
                {
                    loadSceneCallbacks.LoadSceneUpdateCallback(AssetName, progress, UserData);
                }
            }
        }

        public override void OnLoadDependencyAsset(LoadResourceTaskAgent taskAgent, string dependencyAssetName,
            object dependencyAsset)
        {
            base.OnLoadDependencyAsset(taskAgent, dependencyAssetName, dependencyAsset);
            if (loadSceneCallbacks.LoadSceneDependencyAssetCallback != null)
            {
                loadSceneCallbacks.LoadSceneDependencyAssetCallback(AssetName, dependencyAssetName,
                    LoadedDependencyAssetCount, TotalDependencyAssetCount, UserData);
            }
        }
    }
}