using System.Collections.Generic;
using Summer.Base.Model;
using Summer.ObjectPool;
using Summer.Resource.Model.Loader;
using Spring.Collection.Reference;
using Spring.Core;
using Spring.Util;

namespace Summer.Resource.Manager
{
    /// <summary>
    /// 资源对象。
    /// </summary>
    public sealed class AssetObject : ObjectBase
    {
        private List<object> dependencyAssets = new List<object>();
        private object resource;


        public override bool CustomCanReleaseFlag
        {
            get
            {
                int targetReferenceCount = 0;
                SpringContext.GetBean<ResourceLoader>().assetDependencyCount.TryGetValue(Target, out targetReferenceCount);
                return base.CustomCanReleaseFlag && targetReferenceCount <= 0;
            }
        }

        public static AssetObject Create(string name, object target, List<object> dependencyAssets, object resource)
        {
            if (dependencyAssets == null)
            {
                throw new GameFrameworkException("Dependency assets is invalid.");
            }

            if (resource == null)
            {
                throw new GameFrameworkException("Resource is invalid.");
            }


            AssetObject assetObject = ReferenceCache.Acquire<AssetObject>();
            assetObject.Initialize(name, target);
            assetObject.dependencyAssets.AddRange(dependencyAssets);
            assetObject.resource = resource;

            foreach (object dependencyAsset in dependencyAssets)
            {
                var referenceCount = 0;
                var resourceLoader = SpringContext.GetBean<ResourceLoader>();
                if (resourceLoader.assetDependencyCount.TryGetValue(dependencyAsset, out referenceCount))
                {
                    resourceLoader.assetDependencyCount[dependencyAsset] = referenceCount + 1;
                }
                else
                {
                    resourceLoader.assetDependencyCount.Add(dependencyAsset, 1);
                }
            }

            return assetObject;
        }

        public override void Clear()
        {
            base.Clear();
            dependencyAssets.Clear();
            resource = null;
        }

        public override void OnUnspawn()
        {
            base.OnUnspawn();
            foreach (object dependencyAsset in dependencyAssets)
            {
                SpringContext.GetBean<ResourceLoader>().assetPool.Unspawn(dependencyAsset);
            }
        }

        public override void Release(bool isShutdown)
        {
            var resourceLoader = SpringContext.GetBean<ResourceLoader>();
            var resourceHelper =  SpringContext.GetBean<SimpleLoadResourceMono>();
            if (!isShutdown)
            {
                int targetReferenceCount = 0;
                if (SpringContext.GetBean<ResourceLoader>().assetDependencyCount.TryGetValue(Target, out targetReferenceCount) &&
                    targetReferenceCount > 0)
                {
                    throw new GameFrameworkException(StringUtils.Format("Asset target '{}' reference count is '{}' larger than 0.", Name, targetReferenceCount.ToString()));
                }

                foreach (object dependencyAsset in dependencyAssets)
                {
                    int referenceCount = 0;
                    if (resourceLoader.assetDependencyCount.TryGetValue(dependencyAsset, out referenceCount))
                    {
                        resourceLoader.assetDependencyCount[dependencyAsset] = referenceCount - 1;
                    }
                    else
                    {
                        throw new GameFrameworkException(StringUtils.Format("Asset target '{}' dependency asset reference count is invalid.", Name));
                    }
                }

                resourceLoader.resourcePool.Unspawn(resource);
            }

            resourceLoader.assetDependencyCount.Remove(Target);
            resourceLoader.assetToResourceMap.Remove(Target);
            resourceHelper.Release(Target);
        }
    }
}