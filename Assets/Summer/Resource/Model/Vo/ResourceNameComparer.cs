using System.Collections.Generic;

namespace Summer.Resource.Model.Vo
{
    /// <summary>
    /// 资源名称比较器。
    /// </summary>
    public sealed class ResourceNameComparer : IComparer<ResourceName>, IEqualityComparer<ResourceName>
    {
        public static readonly ResourceNameComparer COMPARER = new ResourceNameComparer();
        public ResourceNameComparer()
        {
        }

        public int Compare(ResourceName x, ResourceName y)
        {
            return x.CompareTo(y);
        }

        public bool Equals(ResourceName x, ResourceName y)
        {
            return x.Equals(y);
        }

        public int GetHashCode(ResourceName obj)
        {
            return obj.GetHashCode();
        }
    }
}