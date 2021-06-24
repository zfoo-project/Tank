using System;
using System.Collections.Generic;
using Summer.Base.TaskPool;
using Summer.Resource.Manager;
using Summer.Resource.Model.Constant;
using Summer.Resource.Model.Vo;

namespace Summer.Resource.Model.Loader
{
    public abstract class LoadResourceTaskBase : TaskBase
    {
        private static int Serial = 0;

        private string assetName;
        private Type assetType;
        private ResourceInfo resourceInfo;
        private string[] dependencyAssetNames;
        private object userData;
        private readonly List<object> dependencyAssets = new List<object>();
        private ResourceObject resourceObject;
        private DateTime startTime= default(DateTime);
        private int totalDependencyAssetCount;


        public string AssetName
        {
            get { return assetName; }
        }

        public Type AssetType
        {
            get { return assetType; }
        }

        public ResourceInfo ResourceInfo
        {
            get { return resourceInfo; }
        }

        public ResourceObject ResourceObject
        {
            get { return resourceObject; }
        }

        public abstract bool IsScene { get; }

        public object UserData
        {
            get { return userData; }
        }

        public DateTime StartTime
        {
            get { return startTime; }
            set { startTime = value; }
        }

        public int LoadedDependencyAssetCount
        {
            get { return dependencyAssets.Count; }
        }

        public int TotalDependencyAssetCount
        {
            get { return totalDependencyAssetCount; }
            set { totalDependencyAssetCount = value; }
        }

        public override string Description
        {
            get { return assetName; }
        }

        public override void Clear()
        {
            base.Clear();
            assetName = null;
            assetType = null;
            resourceInfo = null;
            dependencyAssetNames = null;
            userData = null;
            dependencyAssets.Clear();
            resourceObject = null;
            startTime = default(DateTime);
            totalDependencyAssetCount = 0;
        }

        public string[] GetDependencyAssetNames()
        {
            return dependencyAssetNames;
        }

        public List<object> GetDependencyAssets()
        {
            return dependencyAssets;
        }

        public void LoadMain(LoadResourceTaskAgent taskAgent, ResourceObject resourceObject)
        {
            this.resourceObject = resourceObject;
            taskAgent.loadResourceAgentMono.LoadAsset(resourceObject.Target, AssetName, AssetType, IsScene);
        }

        public virtual void OnLoadAssetSuccess(LoadResourceTaskAgent taskAgent, object asset, float duration)
        {
        }

        public virtual void OnLoadAssetFailure(LoadResourceTaskAgent taskAgent, LoadResourceStatus status, string errorMessage)
        {
        }

        public virtual void OnLoadAssetUpdate(LoadResourceTaskAgent taskAgent, LoadResourceProgress type, float progress)
        {
        }

        public virtual void OnLoadDependencyAsset(LoadResourceTaskAgent taskAgent, string dependencyAssetName,
            object dependencyAsset)
        {
            dependencyAssets.Add(dependencyAsset);
        }

        protected void Initialize(string assetName, Type assetType, int priority, ResourceInfo resourceInfo,
            string[] dependencyAssetNames, object userData)
        {
            Initialize(++Serial, priority);
            this.assetName = assetName;
            this.assetType = assetType;
            this.resourceInfo = resourceInfo;
            this.dependencyAssetNames = dependencyAssetNames;
            this.userData = userData;
        }
    }
}