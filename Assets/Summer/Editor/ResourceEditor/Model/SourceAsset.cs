using Spring.Util;
using Summer.Base.Model;
using UnityEditor;
using UnityEngine;

namespace Summer.Editor.ResourceEditor.Model
{
    public sealed class SourceAsset
    {
        public Texture cachedIcon;
        public string guid;
        public string path;
        public string name;
        public SourceFolder folder;
        
        public SourceAsset(string guid, string path, string name, SourceFolder folder)
        {
            if (folder == null)
            {
                throw new GameFrameworkException("Source asset folder is invalid.");
            }

            this.guid = guid;
            this.path = path;
            this.name = name;
            this.folder = folder;
            cachedIcon = null;
        }


        public string FromRootPath
        {
            get
            {
                return folder.folder == null ? name : StringUtils.Format("{}/{}", folder.FromRootPath, name);
            }
        }

        public int Depth
        {
            get
            {
                return folder != null ? folder.Depth + 1 : 0;
            }
        }

        public Texture Icon
        {
            get
            {
                if (cachedIcon == null)
                {
                    cachedIcon = AssetDatabase.GetCachedIcon(path);
                }

                return cachedIcon;
            }
        }
    }
}
