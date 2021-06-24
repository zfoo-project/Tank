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
    public sealed class ResourceObject : ObjectBase
    {
        private List<object> dependencyResources= new List<object>();

        public override bool CustomCanReleaseFlag
        {
            get
            {
                int targetReferenceCount = 0;
                var resourceLoader = SpringContext.GetBean<ResourceLoader>();
                resourceLoader.resourceDependencyCount.TryGetValue(Target, out targetReferenceCount);
                return base.CustomCanReleaseFlag && targetReferenceCount <= 0;
            }
        }

        public static ResourceObject Create(string name, object target)
        {
            ResourceObject resourceObject = ReferenceCache.Acquire<ResourceObject>();
            resourceObject.Initialize(name, target);
            return resourceObject;
        }

        public override void Clear()
        {
            base.Clear();
            dependencyResources.Clear();
        }

        public void AddDependencyResource(object dependencyResource)
        {
            if (Target == dependencyResource)
            {
                return;
            }

            if (dependencyResources.Contains(dependencyResource))
            {
                return;
            }

            dependencyResources.Add(dependencyResource);

            var resourceLoader = SpringContext.GetBean<ResourceLoader>();
            var referenceCount = 0;
            if (resourceLoader.resourceDependencyCount.TryGetValue(dependencyResource, out referenceCount))
            {
                resourceLoader.resourceDependencyCount[dependencyResource] = referenceCount + 1;
            }
            else
            {
                resourceLoader.resourceDependencyCount.Add(dependencyResource, 1);
            }
        }

        public override void Release(bool isShutdown)
        {
            var resourceLoader = SpringContext.GetBean<ResourceLoader>();
            if (!isShutdown)
            {
                int targetReferenceCount = 0;
                if (resourceLoader.resourceDependencyCount.TryGetValue(Target, out targetReferenceCount) &&
                    targetReferenceCount > 0)
                {
                    throw new GameFrameworkException(StringUtils.Format(
                        "Resource target '{}' reference count is '{}' larger than 0.", Name,
                        targetReferenceCount.ToString()));
                }

                foreach (object dependencyResource in dependencyResources)
                {
                    int referenceCount = 0;
                    if (resourceLoader.resourceDependencyCount.TryGetValue(dependencyResource, out referenceCount))
                    {
                        resourceLoader.resourceDependencyCount[dependencyResource] = referenceCount - 1;
                    }
                    else
                    {
                        throw new GameFrameworkException(
                            StringUtils.Format("Resource target '{}' dependency asset reference count is invalid.",
                                Name));
                    }
                }
            }

            resourceLoader.resourceDependencyCount.Remove(Target);
            SpringContext.GetBean<SimpleLoadResourceMono>().Release(Target);
        }
    }
}