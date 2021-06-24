using System.Collections.Generic;
using Summer.Resource.Model.Constant;

namespace Summer.Editor.ResourceBuilder.Model
{
    public sealed class ResourceData
    {
        public readonly string name;
        public readonly string variant;
        public readonly string fileSystem;
        public readonly LoadType loadType;
        public readonly bool packed;
        public readonly string[] resourceGroups;
        public readonly List<AssetData> assetDatas;
        public readonly List<ResourceCode> codes;

        public ResourceData(string name, string variant, string fileSystem, LoadType loadType, bool packed, string[] resourceGroups)
        {
            this.name = name;
            this.variant = variant;
            this.fileSystem = fileSystem;
            this.loadType = loadType;
            this.packed = packed;
            this.resourceGroups = resourceGroups;
            assetDatas = new List<AssetData>();
            codes = new List<ResourceCode>();
        }


        public bool IsLoadFromBinary
        {
            get { return loadType == LoadType.LoadFromBinary || loadType == LoadType.LoadFromBinaryAndQuickDecrypt || loadType == LoadType.LoadFromBinaryAndDecrypt; }
        }

        public int AssetCount
        {
            get { return assetDatas.Count; }
        }

        public string[] GetResourceGroups()
        {
            return resourceGroups;
        }

        public string[] GetAssetGuids()
        {
            string[] assetGuids = new string[assetDatas.Count];
            for (int i = 0; i < assetDatas.Count; i++)
            {
                assetGuids[i] = assetDatas[i].guid;
            }

            return assetGuids;
        }

        public string[] GetAssetNames()
        {
            string[] assetNames = new string[assetDatas.Count];
            for (int i = 0; i < assetDatas.Count; i++)
            {
                assetNames[i] = assetDatas[i].name;
            }

            return assetNames;
        }

        public AssetData[] GetAssetDatas()
        {
            return assetDatas.ToArray();
        }

        public AssetData GetAssetData(string assetName)
        {
            foreach (AssetData assetData in assetDatas)
            {
                if (assetData.name == assetName)
                {
                    return assetData;
                }
            }

            return null;
        }

        public void AddAssetData(string guid, string name, int length, int hashCode, string[] dependencyAssetNames)
        {
            assetDatas.Add(new AssetData(guid, name, length, hashCode, dependencyAssetNames));
        }

        public ResourceCode GetCode(Platform platform)
        {
            foreach (ResourceCode code in codes)
            {
                if (code.platform == platform)
                {
                    return code;
                }
            }

            return null;
        }

        public ResourceCode[] GetCodes()
        {
            return codes.ToArray();
        }

        public void AddCode(Platform platform, int length, int hashCode, int zipLength, int zipHashCode)
        {
            codes.Add(new ResourceCode(platform, length, hashCode, zipLength, zipHashCode));
        }
    }
}