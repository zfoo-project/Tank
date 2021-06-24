using Spring.Util;
using Summer.Base.Model;
using Summer.Editor.ResourceCollection;
using UnityEditor;
using UnityEngine;

namespace Summer.Editor.ResourceEditor.Model
{
    public sealed class ResourceItem
    {
        private static Texture cachedUnknownIcon;
        private static Texture cachedAssetIcon;
        private static Texture cachedSceneIcon;

        public ResourceItem(string name, ResourceCollection.Resource resource, ResourceFolder folder)
        {
            if (resource == null)
            {
                throw new GameFrameworkException("Resource is invalid.");
            }

            if (folder == null)
            {
                throw new GameFrameworkException("Resource folder is invalid.");
            }

            Name = name;
            Resource = resource;
            Folder = folder;
        }

        public string Name { get; private set; }

        public ResourceCollection.Resource Resource { get; private set; }

        public ResourceFolder Folder { get; private set; }

        public string FromRootPath
        {
            get { return Folder.Folder == null ? Name : StringUtils.Format("{}/{}", Folder.FromRootPath, Name); }
        }

        public int Depth
        {
            get { return Folder != null ? Folder.Depth + 1 : 0; }
        }

        public Texture Icon
        {
            get
            {
                if (Resource.IsLoadFromBinary)
                {
                    Asset asset = Resource.GetFirstAsset();
                    if (asset != null)
                    {
                        Texture texture = AssetDatabase.GetCachedIcon(AssetDatabase.GUIDToAssetPath(asset.guid));
                        return texture != null ? texture : CachedUnknownIcon;
                    }
                }
                else
                {
                    switch (Resource.AssetType)
                    {
                        case AssetType.Asset:
                            return CachedAssetIcon;

                        case AssetType.Scene:
                            return CachedSceneIcon;
                    }
                }

                return CachedUnknownIcon;
            }
        }

        private static Texture CachedUnknownIcon
        {
            get
            {
                if (cachedUnknownIcon == null)
                {
                    string iconName = null;
#if UNITY_2018_3_OR_NEWER
                    iconName = "GameObject Icon";
#else
                        iconName = "Prefab Icon";
#endif
                    cachedUnknownIcon = GetIcon(iconName);
                }

                return cachedUnknownIcon;
            }
        }

        private static Texture CachedAssetIcon
        {
            get
            {
                if (cachedAssetIcon == null)
                {
                    string iconName = null;
#if UNITY_2018_3_OR_NEWER
                    iconName = "Prefab Icon";
#else
                        iconName = "PrefabNormal Icon";
#endif
                    cachedAssetIcon = GetIcon(iconName);
                }

                return cachedAssetIcon;
            }
        }

        private static Texture CachedSceneIcon
        {
            get
            {
                if (cachedSceneIcon == null)
                {
                    cachedSceneIcon = GetIcon("SceneAsset Icon");
                }

                return cachedSceneIcon;
            }
        }

        private static Texture GetIcon(string iconName)
        {
            return EditorGUIUtility.IconContent(iconName).image;
        }
    }
}