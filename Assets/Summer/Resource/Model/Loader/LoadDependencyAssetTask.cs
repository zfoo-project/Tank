using Summer.Resource.Manager;
using Summer.Resource.Model.Constant;
using Summer.Resource.Model.Vo;
using Spring.Collection.Reference;
using Spring.Util;

namespace Summer.Resource.Model.Loader
{
    public sealed class LoadDependencyAssetTask : LoadResourceTaskBase
    {
        private LoadResourceTaskBase mainTask;

        public override bool IsScene
        {
            get { return false; }
        }

        public static LoadDependencyAssetTask Create(string assetName, int priority, ResourceInfo resourceInfo,
            string[] dependencyAssetNames, LoadResourceTaskBase mainTask, object userData)
        {
            LoadDependencyAssetTask loadDependencyAssetTask = ReferenceCache.Acquire<LoadDependencyAssetTask>();
            loadDependencyAssetTask.Initialize(assetName, null, priority, resourceInfo, dependencyAssetNames, userData);
            loadDependencyAssetTask.mainTask = mainTask;
            loadDependencyAssetTask.mainTask.TotalDependencyAssetCount++;
            return loadDependencyAssetTask;
        }

        public override void Clear()
        {
            base.Clear();
            mainTask = null;
        }

        public override void OnLoadAssetSuccess(LoadResourceTaskAgent taskAgent, object asset, float duration)
        {
            base.OnLoadAssetSuccess(taskAgent, asset, duration);
            mainTask.OnLoadDependencyAsset(taskAgent, AssetName, asset);
        }

        public override void OnLoadAssetFailure(LoadResourceTaskAgent taskAgent, LoadResourceStatus status, string errorMessage)
        {
            base.OnLoadAssetFailure(taskAgent, status, errorMessage);
            mainTask.OnLoadAssetFailure(taskAgent, LoadResourceStatus.DependencyError,
                StringUtils.Format("Can not load dependency asset '{}', status '{}', error message '{}'.",
                    AssetName, status.ToString(), errorMessage));
        }
    }
}