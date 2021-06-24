using System.Collections.Generic;
using Spring.Util;
using Summer.Resource.Model.Constant;

namespace Summer.Editor.ResourceCollection
{
    /// <summary>
    /// 资源。
    /// </summary>
    public sealed class Resource
    {
        private readonly List<Asset> assets;
        private readonly List<string> mresourceGroups;

        private Resource(string name, string variant, string fileSystem, LoadType loadType, bool packed, string[] resourceGroups)
        {
            assets = new List<Asset>();
            mresourceGroups = new List<string>();

            Name = name;
            Variant = variant;
            AssetType = AssetType.Unknown;
            FileSystem = fileSystem;
            LoadType = loadType;
            Packed = packed;

            foreach (string resourceGroup in resourceGroups)
            {
                AddResourceGroup(resourceGroup);
            }
        }

        public string Name
        {
            get;
            private set;
        }

        public string Variant
        {
            get;
            private set;
        }

        public string FullName
        {
            get
            {
                return Variant != null ? StringUtils.Format("{}.{}", Name, Variant) : Name;
            }
        }

        public AssetType AssetType
        {
            get;
            private set;
        }

        public bool IsLoadFromBinary
        {
            get
            {
                return LoadType == LoadType.LoadFromBinary || LoadType == LoadType.LoadFromBinaryAndQuickDecrypt || LoadType == LoadType.LoadFromBinaryAndDecrypt;
            }
        }

        public string FileSystem
        {
            get;
            set;
        }

        public LoadType LoadType
        {
            get;
            set;
        }

        public bool Packed
        {
            get;
            set;
        }

        public static Resource Create(string name, string variant, string fileSystem, LoadType loadType, bool packed, string[] resourceGroups)
        {
            return new Resource(name, variant, fileSystem, loadType, packed, resourceGroups ?? new string[0]);
        }

        public Asset[] GetAssets()
        {
            return assets.ToArray();
        }

        public Asset GetFirstAsset()
        {
            return assets.Count > 0 ? assets[0] : null;
        }

        public void Rename(string name, string variant)
        {
            Name = name;
            Variant = variant;
        }

        public void AssignAsset(Asset asset, bool isScene)
        {
            if (asset.resource != null)
            {
                asset.resource.UnassignAsset(asset);
            }

            AssetType = isScene ? AssetType.Scene : AssetType.Asset;
            asset.resource = this;
            assets.Add(asset);
            assets.Sort(AssetComparer);
        }

        public void UnassignAsset(Asset asset)
        {
            asset.resource = null;
            assets.Remove(asset);
            if (assets.Count <= 0)
            {
                AssetType = AssetType.Unknown;
            }
        }

        public string[] GetResourceGroups()
        {
            return mresourceGroups.ToArray();
        }

        public bool HasResourceGroup(string resourceGroup)
        {
            if (string.IsNullOrEmpty(resourceGroup))
            {
                return false;
            }

            return mresourceGroups.Contains(resourceGroup);
        }

        public void AddResourceGroup(string resourceGroup)
        {
            if (string.IsNullOrEmpty(resourceGroup))
            {
                return;
            }

            if (mresourceGroups.Contains(resourceGroup))
            {
                return;
            }

            mresourceGroups.Add(resourceGroup);
            mresourceGroups.Sort();
        }

        public bool RemoveResourceGroup(string resourceGroup)
        {
            if (string.IsNullOrEmpty(resourceGroup))
            {
                return false;
            }

            return mresourceGroups.Remove(resourceGroup);
        }

        public void Clear()
        {
            foreach (Asset asset in assets)
            {
                asset.resource = null;
            }

            assets.Clear();
            mresourceGroups.Clear();
        }

        private int AssetComparer(Asset a, Asset b)
        {
            return a.guid.CompareTo(b.guid);
        }
    }
}
