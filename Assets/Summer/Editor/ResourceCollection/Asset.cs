using System;
using UnityEditor;

namespace Summer.Editor.ResourceCollection
{
    /// <summary>
    /// 资源。
    /// </summary>
    public sealed class Asset : IComparable<Asset>
    {

        public string guid;
        
        public Resource resource;
        
        public string Name
        {
            get
            {
                return AssetDatabase.GUIDToAssetPath(guid);
            }
        }


        public int CompareTo(Asset asset)
        {
            return string.Compare(guid, asset.guid, StringComparison.Ordinal);
        }

        public static Asset ValueOf(string guid)
        {
            var asset = new Asset();
            asset.guid = guid;
            return asset;
        }
    }
}
