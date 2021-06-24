using System;
using System.Runtime.InteropServices;
using Summer.Base.Model;
using Spring.Util;

namespace Summer.Resource.Model.Vo
{
    /// <summary>
    /// 资源名称。
    /// </summary>
    [StructLayout(LayoutKind.Auto)]
    public struct ResourceName : IComparable, IComparable<ResourceName>, IEquatable<ResourceName>
    {
        private readonly string name;
        private readonly string variant;
        private readonly string extension;
        private string cachedFullName;

        /// <summary>
        /// 初始化资源名称的新实例。
        /// </summary>
        /// <param name="name">资源名称。</param>
        /// <param name="variant">变体名称。</param>
        /// <param name="extension">扩展名称。</param>
        public ResourceName(string name, string variant, string extension)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new GameFrameworkException("Resource name is invalid.");
            }

            if (string.IsNullOrEmpty(extension))
            {
                throw new GameFrameworkException("Resource extension is invalid.");
            }

            this.name = name;
            this.variant = variant;
            this.extension = extension;
            this.cachedFullName = null;
        }

        /// <summary>
        /// 获取资源名称。
        /// </summary>
        public string Name
        {
            get { return name; }
        }

        /// <summary>
        /// 获取变体名称。
        /// </summary>
        public string Variant
        {
            get { return variant; }
        }

        /// <summary>
        /// 获取扩展名称。
        /// </summary>
        public string Extension
        {
            get { return extension; }
        }

        public string FullName
        {
            get
            {
                if (cachedFullName == null)
                {
                    cachedFullName = variant != null
                        ? StringUtils.Format("{}.{}.{}", name, variant, extension)
                        : StringUtils.Format("{}.{}", name, extension);
                }

                return cachedFullName;
            }
        }

        public override string ToString()
        {
            return FullName;
        }

        public override int GetHashCode()
        {
            if (variant == null)
            {
                return name.GetHashCode() ^ extension.GetHashCode();
            }

            return name.GetHashCode() ^ variant.GetHashCode() ^ extension.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            return (obj is ResourceName) && Equals((ResourceName) obj);
        }

        public bool Equals(ResourceName value)
        {
            return string.Equals(name, value.name, StringComparison.Ordinal) &&
                   string.Equals(variant, value.variant, StringComparison.Ordinal) &&
                   string.Equals(extension, value.extension, StringComparison.Ordinal);
        }

        public static bool operator ==(ResourceName a, ResourceName b)
        {
            return a.Equals(b);
        }

        public static bool operator !=(ResourceName a, ResourceName b)
        {
            return !(a == b);
        }

        public int CompareTo(object value)
        {
            if (value == null)
            {
                return 1;
            }

            if (!(value is ResourceName))
            {
                throw new GameFrameworkException("Type of value is invalid.");
            }

            return CompareTo((ResourceName) value);
        }

        public int CompareTo(ResourceName resourceName)
        {
            int result = string.CompareOrdinal(name, resourceName.name);
            if (result != 0)
            {
                return result;
            }

            result = string.CompareOrdinal(variant, resourceName.variant);
            if (result != 0)
            {
                return result;
            }

            return string.CompareOrdinal(extension, resourceName.extension);
        }
    }
}