using System.Collections.Generic;
using Spring.Util;
using Summer.Base.Model;
using UnityEditor;
using UnityEngine;

namespace Summer.Editor.ResourceEditor.Model
{
    public sealed class ResourceFolder
    {
        private static Texture s_CachedIcon = null;

        private readonly List<ResourceFolder> folders;
        private readonly List<ResourceItem> items;

        public ResourceFolder(string name, ResourceFolder folder)
        {
            folders = new List<ResourceFolder>();
            items = new List<ResourceItem>();

            Name = name;
            Folder = folder;
        }

        public string Name { get; private set; }

        public ResourceFolder Folder { get; private set; }

        public string FromRootPath
        {
            get { return Folder == null ? string.Empty : (Folder.Folder == null ? Name : StringUtils.Format("{}/{}", Folder.FromRootPath, Name)); }
        }

        public int Depth
        {
            get { return Folder != null ? Folder.Depth + 1 : 0; }
        }

        public static Texture Icon
        {
            get
            {
                if (s_CachedIcon == null)
                {
                    s_CachedIcon = AssetDatabase.GetCachedIcon("Assets");
                }

                return s_CachedIcon;
            }
        }

        public void Clear()
        {
            folders.Clear();
            items.Clear();
        }

        public ResourceFolder[] GetFolders()
        {
            return folders.ToArray();
        }

        public ResourceFolder GetFolder(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new GameFrameworkException("Resource folder name is invalid.");
            }

            foreach (ResourceFolder folder in folders)
            {
                if (folder.Name == name)
                {
                    return folder;
                }
            }

            return null;
        }

        public ResourceFolder AddFolder(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new GameFrameworkException("Resource folder name is invalid.");
            }

            ResourceFolder folder = GetFolder(name);
            if (folder != null)
            {
                throw new GameFrameworkException("Resource folder is already exist.");
            }

            folder = new ResourceFolder(name, this);
            folders.Add(folder);

            return folder;
        }

        public ResourceItem[] GetItems()
        {
            return items.ToArray();
        }

        public ResourceItem GetItem(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new GameFrameworkException("Resource item name is invalid.");
            }

            foreach (ResourceItem item in items)
            {
                if (item.Name == name)
                {
                    return item;
                }
            }

            return null;
        }

        public void AddItem(string name, ResourceCollection.Resource resource)
        {
            ResourceItem item = GetItem(name);
            if (item != null)
            {
                throw new GameFrameworkException("Resource item is already exist.");
            }

            item = new ResourceItem(name, resource, this);
            items.Add(item);
            items.Sort(ResourceItemComparer);
        }

        private int ResourceItemComparer(ResourceItem a, ResourceItem b)
        {
            return a.Name.CompareTo(b.Name);
        }
    }
}