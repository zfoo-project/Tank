using System;
using Summer.Resource.Manager;
using Summer.Resource.Model.Callback;
using Summer.Resource.Model.Constant;
using Summer.Resource.Model.Vo;
using Spring.Collection.Reference;

namespace Summer.Resource.Model.Loader
{
    public sealed class LoadAssetTask : LoadResourceTaskBase
    {
        private LoadAssetCallbacks loadAssetCallbacks;


        public override bool IsScene
        {
            get { return false; }
        }

        public static LoadAssetTask Create(string assetName, Type assetType, int priority,
            ResourceInfo resourceInfo, string[] dependencyAssetNames, LoadAssetCallbacks loadAssetCallbacks,
            object userData)
        {
            LoadAssetTask loadAssetTask = ReferenceCache.Acquire<LoadAssetTask>();
            loadAssetTask.Initialize(assetName, assetType, priority, resourceInfo, dependencyAssetNames, userData);
            loadAssetTask.loadAssetCallbacks = loadAssetCallbacks;
            return loadAssetTask;
        }

        public override void Clear()
        {
            base.Clear();
            loadAssetCallbacks = null;
        }

        public override void OnLoadAssetSuccess(LoadResourceTaskAgent taskAgent, object asset, float duration)
        {
            base.OnLoadAssetSuccess(taskAgent, asset, duration);
            if (loadAssetCallbacks.LoadAssetSuccessCallback != null)
            {
                loadAssetCallbacks.LoadAssetSuccessCallback(AssetName, asset, duration, UserData);
            }
        }

        public override void OnLoadAssetFailure(LoadResourceTaskAgent taskAgent, LoadResourceStatus status,
            string errorMessage)
        {
            base.OnLoadAssetFailure(taskAgent, status, errorMessage);
            if (loadAssetCallbacks.LoadAssetFailureCallback != null)
            {
                loadAssetCallbacks.LoadAssetFailureCallback(AssetName, status, errorMessage, UserData);
            }
        }

        public override void OnLoadAssetUpdate(LoadResourceTaskAgent taskAgent, LoadResourceProgress type, float progress)
        {
            base.OnLoadAssetUpdate(taskAgent, type, progress);
            if (type == LoadResourceProgress.LoadAsset)
            {
                if (loadAssetCallbacks.LoadAssetUpdateCallback != null)
                {
                    loadAssetCallbacks.LoadAssetUpdateCallback(AssetName, progress, UserData);
                }
            }
        }

        public override void OnLoadDependencyAsset(LoadResourceTaskAgent taskAgent, string dependencyAssetName,
            object dependencyAsset)
        {
            base.OnLoadDependencyAsset(taskAgent, dependencyAssetName, dependencyAsset);
            if (loadAssetCallbacks.LoadAssetDependencyAssetCallback != null)
            {
                loadAssetCallbacks.LoadAssetDependencyAssetCallback(AssetName, dependencyAssetName,
                    LoadedDependencyAssetCount, TotalDependencyAssetCount, UserData);
            }
        }
    }
}