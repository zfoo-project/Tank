using System.Collections.Generic;
using Spring.Util;
using Summer.Base.Model;
using UnityEditor;
using UnityEngine;

namespace Summer.Editor.ResourceEditor.Model
{
    public sealed class SourceFolder
    {
        private static Texture cachedIcon;

        private readonly List<SourceFolder> m_Folders;
        private readonly List<SourceAsset> m_Assets;
        public string name;
        public SourceFolder folder;
        
        public SourceFolder(string name, SourceFolder folder)
        {
            m_Folders = new List<SourceFolder>();
            m_Assets = new List<SourceAsset>();

            this.name = name;
            this.folder = folder;
        }


        public string FromRootPath
        {
            get
            {
                return folder == null ? string.Empty : (folder.folder == null ? name : StringUtils.Format("{}/{}", folder.FromRootPath, name));
            }
        }

        public int Depth
        {
            get
            {
                return folder != null ? folder.Depth + 1 : 0;
            }
        }

        public static Texture Icon
        {
            get
            {
                if (cachedIcon == null)
                {
                    cachedIcon = AssetDatabase.GetCachedIcon("Assets");
                }

                return cachedIcon;
            }
        }

        public void Clear()
        {
            m_Folders.Clear();
            m_Assets.Clear();
        }

        public SourceFolder[] GetFolders()
        {
            return m_Folders.ToArray();
        }

        public SourceFolder GetFolder(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new GameFrameworkException("Source folder name is invalid.");
            }

            foreach (SourceFolder folder in m_Folders)
            {
                if (folder.name == name)
                {
                    return folder;
                }
            }

            return null;
        }

        public SourceFolder AddFolder(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new GameFrameworkException("Source folder name is invalid.");
            }

            SourceFolder folder = GetFolder(name);
            if (folder != null)
            {
                throw new GameFrameworkException("Source folder is already exist.");
            }

            folder = new SourceFolder(name, this);
            m_Folders.Add(folder);

            return folder;
        }

        public SourceAsset[] GetAssets()
        {
            return m_Assets.ToArray();
        }

        public SourceAsset GetAsset(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new GameFrameworkException("Source asset name is invalid.");
            }

            foreach (SourceAsset asset in m_Assets)
            {
                if (asset.name == name)
                {
                    return asset;
                }
            }

            return null;
        }

        public SourceAsset AddAsset(string guid, string path, string name)
        {
            if (string.IsNullOrEmpty(guid))
            {
                throw new GameFrameworkException("Source asset guid is invalid.");
            }

            if (string.IsNullOrEmpty(path))
            {
                throw new GameFrameworkException("Source asset path is invalid.");
            }

            if (string.IsNullOrEmpty(name))
            {
                throw new GameFrameworkException("Source asset name is invalid.");
            }

            SourceAsset asset = GetAsset(name);
            if (asset != null)
            {
                throw new GameFrameworkException(StringUtils.Format("Source asset '{}' is already exist.", name));
            }

            asset = new SourceAsset(guid, path, name, this);
            m_Assets.Add(asset);

            return asset;
        }
    }
}
