using System.Collections.Generic;
using Summer.Base.Model;
using Summer.Resource.Model.Vo;

namespace Summer.Resource.Model.Group
{
    /// <summary>
    /// 资源组。
    /// </summary>
    public sealed class ResourceGroup : IResourceGroup
    {
        private readonly string name;
        private readonly Dictionary<ResourceName, ResourceInfo> resourceInfos;
        private readonly HashSet<ResourceName> resourceNames;
        private long totalLength;
        private long totalZipLength;

        /// <summary>
        /// 初始化资源组的新实例。
        /// </summary>
        /// <param name="name">资源组名称。</param>
        /// <param name="resourceInfos">资源信息引用。</param>
        public ResourceGroup(string name, Dictionary<ResourceName, ResourceInfo> resourceInfos)
        {
            if (name == null)
            {
                throw new GameFrameworkException("Name is invalid.");
            }

            if (resourceInfos == null)
            {
                throw new GameFrameworkException("Resource infos is invalid.");
            }

            this.name = name;
            this.resourceInfos = resourceInfos;
            resourceNames = new HashSet<ResourceName>();
        }

        /// <summary>
        /// 获取资源组名称。
        /// </summary>
        public string Name
        {
            get { return name; }
        }

        /// <summary>
        /// 获取资源组是否准备完毕。
        /// </summary>
        public bool Ready
        {
            get { return ReadyCount >= TotalCount; }
        }

        /// <summary>
        /// 获取资源组包含资源数量。
        /// </summary>
        public int TotalCount
        {
            get { return resourceNames.Count; }
        }

        /// <summary>
        /// 获取资源组中已准备完成资源数量。
        /// </summary>
        public int ReadyCount
        {
            get
            {
                int readyCount = 0;
                foreach (ResourceName resourceName in resourceNames)
                {
                    ResourceInfo resourceInfo = null;
                    if (resourceInfos.TryGetValue(resourceName, out resourceInfo) && resourceInfo.Ready)
                    {
                        readyCount++;
                    }
                }

                return readyCount;
            }
        }

        /// <summary>
        /// 获取资源组包含资源的总大小。
        /// </summary>
        public long TotalLength
        {
            get { return totalLength; }
        }

        /// <summary>
        /// 获取资源组包含资源压缩后的总大小。
        /// </summary>
        public long TotalZipLength
        {
            get { return totalZipLength; }
        }

        /// <summary>
        /// 获取资源组中已准备完成资源的总大小。
        /// </summary>
        public long ReadyLength
        {
            get
            {
                long totalReadyLength = 0L;
                foreach (ResourceName resourceName in resourceNames)
                {
                    ResourceInfo resourceInfo = null;
                    if (resourceInfos.TryGetValue(resourceName, out resourceInfo) && resourceInfo.Ready)
                    {
                        totalReadyLength += resourceInfo.Length;
                    }
                }

                return totalReadyLength;
            }
        }

        /// <summary>
        /// 获取资源组的完成进度。
        /// </summary>
        public float Progress
        {
            get { return totalLength > 0L ? (float) ReadyLength / totalLength : 1f; }
        }

        /// <summary>
        /// 获取资源组包含的资源名称列表。
        /// </summary>
        /// <returns>资源组包含的资源名称列表。</returns>
        public string[] GetResourceNames()
        {
            int index = 0;
            string[] resourceNames = new string[this.resourceNames.Count];
            foreach (ResourceName resourceName in this.resourceNames)
            {
                resourceNames[index++] = resourceName.FullName;
            }

            return resourceNames;
        }

        /// <summary>
        /// 获取资源组包含的资源名称列表。
        /// </summary>
        /// <param name="results">资源组包含的资源名称列表。</param>
        public void GetResourceNames(List<string> results)
        {
            if (results == null)
            {
                throw new GameFrameworkException("Results is invalid.");
            }

            results.Clear();
            foreach (ResourceName resourceName in resourceNames)
            {
                results.Add(resourceName.FullName);
            }
        }

        /// <summary>
        /// 获取资源组包含的资源名称列表。
        /// </summary>
        /// <returns>资源组包含的资源名称列表。</returns>
        public ResourceName[] InternalGetResourceNames()
        {
            int index = 0;
            ResourceName[] resourceNames = new ResourceName[this.resourceNames.Count];
            foreach (ResourceName resourceName in this.resourceNames)
            {
                resourceNames[index++] = resourceName;
            }

            return resourceNames;
        }

        /// <summary>
        /// 获取资源组包含的资源名称列表。
        /// </summary>
        /// <param name="results">资源组包含的资源名称列表。</param>
        public void InternalGetResourceNames(List<ResourceName> results)
        {
            if (results == null)
            {
                throw new GameFrameworkException("Results is invalid.");
            }

            results.Clear();
            foreach (ResourceName resourceName in resourceNames)
            {
                results.Add(resourceName);
            }
        }

        /// <summary>
        /// 检查指定资源是否属于资源组。
        /// </summary>
        /// <param name="resourceName">要检查的资源的名称。</param>
        /// <returns>指定资源是否属于资源组。</returns>
        public bool HasResource(ResourceName resourceName)
        {
            return resourceNames.Contains(resourceName);
        }

        /// <summary>
        /// 向资源组中增加资源。
        /// </summary>
        /// <param name="resourceName">资源名称。</param>
        /// <param name="length">资源大小。</param>
        /// <param name="zipLength">资源压缩后的大小。</param>
        public void AddResource(ResourceName resourceName, int length, int zipLength)
        {
            resourceNames.Add(resourceName);
            totalLength += length;
            totalZipLength += zipLength;
        }
    }
}