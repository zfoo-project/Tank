using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml;
using Summer.Base.Model;
using Spring.Util;
using Summer.Resource.Model.Constant;
using UnityEditor;
using UnityEngine;

namespace Summer.Editor.ResourceCollection
{
    /// <summary>
    /// 资源集合。
    /// </summary>
    public sealed class ResourceCollection
    {
        private const string SceneExtension = ".unity";
        private static readonly Regex ResourceNameRegex = new Regex(@"^([A-Za-z0-9\._-]+/)*[A-Za-z0-9\._-]+$");
        private static readonly Regex ResourceVariantRegex = new Regex(@"^[a-z0-9_-]+$");

        private readonly string configurationPath;
        private readonly SortedDictionary<string, Resource> resources;
        private readonly SortedDictionary<string, Asset> assets;

        public ResourceCollection()
        {
            configurationPath = (string) AssemblyUtils.GetAllFieldsByAttribute<ResourceCollectionConfigPathAttribute>().First().GetValue(null);
            resources = new SortedDictionary<string, Resource>(StringComparer.Ordinal);
            assets = new SortedDictionary<string, Asset>(StringComparer.Ordinal);
        }

        public int ResourceCount
        {
            get
            {
                return resources.Count;
            }
        }

        public int AssetCount
        {
            get
            {
                return assets.Count;
            }
        }

        public event GameFrameworkAction<int, int> OnLoadingResource = null;

        public event GameFrameworkAction<int, int> OnLoadingAsset = null;

        public event GameFrameworkAction OnLoadCompleted = null;

        public void Clear()
        {
            resources.Clear();
            assets.Clear();
        }

        public bool Load()
        {
            Clear();

            if (!File.Exists(configurationPath))
            {
                return false;
            }

            try
            {
                XmlDocument xmlDocument = new XmlDocument();
                xmlDocument.Load(configurationPath);
                XmlNode xmlRoot = xmlDocument.SelectSingleNode("Summer");
                XmlNode xmlCollection = xmlRoot.SelectSingleNode("ResourceCollection");
                XmlNode xmlResources = xmlCollection.SelectSingleNode("Resources");
                XmlNode xmlAssets = xmlCollection.SelectSingleNode("Assets");

                XmlNodeList xmlNodeList = null;
                XmlNode xmlNode = null;
                int count = 0;

                xmlNodeList = xmlResources.ChildNodes;
                count = xmlNodeList.Count;
                for (int i = 0; i < count; i++)
                {
                    if (OnLoadingResource != null)
                    {
                        OnLoadingResource(i, count);
                    }

                    xmlNode = xmlNodeList.Item(i);
                    if (xmlNode.Name != "Resource")
                    {
                        continue;
                    }

                    string name = xmlNode.Attributes.GetNamedItem("Name").Value;
                    string variant = xmlNode.Attributes.GetNamedItem("Variant") != null ? xmlNode.Attributes.GetNamedItem("Variant").Value : null;
                    string fileSystem = xmlNode.Attributes.GetNamedItem("FileSystem") != null ? xmlNode.Attributes.GetNamedItem("FileSystem").Value : null;
                    byte loadType = 0;
                    if (xmlNode.Attributes.GetNamedItem("LoadType") != null)
                    {
                        byte.TryParse(xmlNode.Attributes.GetNamedItem("LoadType").Value, out loadType);
                    }

                    bool packed = false;
                    if (xmlNode.Attributes.GetNamedItem("Packed") != null)
                    {
                        bool.TryParse(xmlNode.Attributes.GetNamedItem("Packed").Value, out packed);
                    }

                    string[] resourceGroups = xmlNode.Attributes.GetNamedItem("ResourceGroups") != null ? xmlNode.Attributes.GetNamedItem("ResourceGroups").Value.Split(',') : null;
                    if (!AddResource(name, variant, fileSystem, (LoadType)loadType, packed, resourceGroups))
                    {
                        Debug.LogWarning(StringUtils.Format("Can not add resource '{}'.", GetResourceFullName(name, variant)));
                        continue;
                    }
                }

                xmlNodeList = xmlAssets.ChildNodes;
                count = xmlNodeList.Count;
                for (int i = 0; i < count; i++)
                {
                    if (OnLoadingAsset != null)
                    {
                        OnLoadingAsset(i, count);
                    }

                    xmlNode = xmlNodeList.Item(i);
                    if (xmlNode.Name != "Asset")
                    {
                        continue;
                    }

                    string guid = xmlNode.Attributes.GetNamedItem("Guid").Value;
                    string name = xmlNode.Attributes.GetNamedItem("ResourceName").Value;
                    string variant = xmlNode.Attributes.GetNamedItem("ResourceVariant") != null ? xmlNode.Attributes.GetNamedItem("ResourceVariant").Value : null;
                    if (!AssignAsset(guid, name, variant))
                    {
                        Debug.LogWarning(StringUtils.Format("Can not assign asset '{}' to resource '{}'.", guid, GetResourceFullName(name, variant)));
                        continue;
                    }
                }

                if (OnLoadCompleted != null)
                {
                    OnLoadCompleted();
                }

                return true;
            }
            catch
            {
                File.Delete(configurationPath);
                if (OnLoadCompleted != null)
                {
                    OnLoadCompleted();
                }

                return false;
            }
        }

        public bool Save()
        {
            try
            {
                XmlDocument xmlDocument = new XmlDocument();
                xmlDocument.AppendChild(xmlDocument.CreateXmlDeclaration("1.0", "UTF-8", null));

                XmlElement xmlRoot = xmlDocument.CreateElement("Summer");
                xmlDocument.AppendChild(xmlRoot);

                XmlElement xmlCollection = xmlDocument.CreateElement("ResourceCollection");
                xmlRoot.AppendChild(xmlCollection);

                XmlElement xmlResources = xmlDocument.CreateElement("Resources");
                xmlCollection.AppendChild(xmlResources);

                XmlElement xmlAssets = xmlDocument.CreateElement("Assets");
                xmlCollection.AppendChild(xmlAssets);

                XmlElement xmlElement = null;
                XmlAttribute xmlAttribute = null;

                foreach (Resource resource in resources.Values)
                {
                    xmlElement = xmlDocument.CreateElement("Resource");
                    xmlAttribute = xmlDocument.CreateAttribute("Name");
                    xmlAttribute.Value = resource.Name;
                    xmlElement.Attributes.SetNamedItem(xmlAttribute);

                    if (resource.Variant != null)
                    {
                        xmlAttribute = xmlDocument.CreateAttribute("Variant");
                        xmlAttribute.Value = resource.Variant;
                        xmlElement.Attributes.SetNamedItem(xmlAttribute);
                    }

                    if (resource.FileSystem != null)
                    {
                        xmlAttribute = xmlDocument.CreateAttribute("FileSystem");
                        xmlAttribute.Value = resource.FileSystem;
                        xmlElement.Attributes.SetNamedItem(xmlAttribute);
                    }

                    xmlAttribute = xmlDocument.CreateAttribute("LoadType");
                    xmlAttribute.Value = ((byte)resource.LoadType).ToString();
                    xmlElement.Attributes.SetNamedItem(xmlAttribute);
                    xmlAttribute = xmlDocument.CreateAttribute("Packed");
                    xmlAttribute.Value = resource.Packed.ToString();
                    xmlElement.Attributes.SetNamedItem(xmlAttribute);
                    string[] resourceGroups = resource.GetResourceGroups();
                    if (resourceGroups.Length > 0)
                    {
                        xmlAttribute = xmlDocument.CreateAttribute("ResourceGroups");
                        xmlAttribute.Value = string.Join(",", resourceGroups);
                        xmlElement.Attributes.SetNamedItem(xmlAttribute);
                    }

                    xmlResources.AppendChild(xmlElement);
                }

                foreach (Asset asset in assets.Values)
                {
                    xmlElement = xmlDocument.CreateElement("Asset");
                    xmlAttribute = xmlDocument.CreateAttribute("Guid");
                    xmlAttribute.Value = asset.guid;
                    xmlElement.Attributes.SetNamedItem(xmlAttribute);
                    xmlAttribute = xmlDocument.CreateAttribute("ResourceName");
                    xmlAttribute.Value = asset.resource.Name;
                    xmlElement.Attributes.SetNamedItem(xmlAttribute);
                    if (asset.resource.Variant != null)
                    {
                        xmlAttribute = xmlDocument.CreateAttribute("ResourceVariant");
                        xmlAttribute.Value = asset.resource.Variant;
                        xmlElement.Attributes.SetNamedItem(xmlAttribute);
                    }

                    xmlAssets.AppendChild(xmlElement);
                }

                string configurationDirectoryName = Path.GetDirectoryName(configurationPath);
                if (!Directory.Exists(configurationDirectoryName))
                {
                    Directory.CreateDirectory(configurationDirectoryName);
                }

                xmlDocument.Save(configurationPath);
                AssetDatabase.Refresh();
                return true;
            }
            catch
            {
                if (File.Exists(configurationPath))
                {
                    File.Delete(configurationPath);
                }

                return false;
            }
        }

        public Resource[] GetResources()
        {
            return resources.Values.ToArray();
        }

        public Resource GetResource(string name, string variant)
        {
            if (!IsValidResourceName(name, variant))
            {
                return null;
            }

            Resource resource = null;
            if (resources.TryGetValue(GetResourceFullName(name, variant).ToLower(), out resource))
            {
                return resource;
            }

            return null;
        }

        public bool HasResource(string name, string variant)
        {
            if (!IsValidResourceName(name, variant))
            {
                return false;
            }

            return resources.ContainsKey(GetResourceFullName(name, variant).ToLower());
        }

        public bool AddResource(string name, string variant, string fileSystem, LoadType loadType, bool packed)
        {
            return AddResource(name, variant, fileSystem, loadType, packed, null);
        }

        public bool AddResource(string name, string variant, string fileSystem, LoadType loadType, bool packed, string[] resourceGroups)
        {
            if (!IsValidResourceName(name, variant))
            {
                return false;
            }

            if (!IsAvailableResourceName(name, variant, null))
            {
                return false;
            }

            if (fileSystem != null && !ResourceNameRegex.IsMatch(fileSystem))
            {
                return false;
            }

            Resource resource = Resource.Create(name, variant, fileSystem, loadType, packed, resourceGroups);
            resources.Add(resource.FullName.ToLower(), resource);

            return true;
        }

        public bool RenameResource(string oldName, string oldVariant, string newName, string newVariant)
        {
            if (!IsValidResourceName(oldName, oldVariant) || !IsValidResourceName(newName, newVariant))
            {
                return false;
            }

            Resource resource = GetResource(oldName, oldVariant);
            if (resource == null)
            {
                return false;
            }

            if (oldName == newName && oldVariant == newVariant)
            {
                return true;
            }

            if (!IsAvailableResourceName(newName, newVariant, resource))
            {
                return false;
            }

            resources.Remove(resource.FullName.ToLower());
            resource.Rename(newName, newVariant);
            resources.Add(resource.FullName.ToLower(), resource);

            return true;
        }

        public bool RemoveResource(string name, string variant)
        {
            if (!IsValidResourceName(name, variant))
            {
                return false;
            }

            Resource resource = GetResource(name, variant);
            if (resource == null)
            {
                return false;
            }

            Asset[] assets = resource.GetAssets();
            resource.Clear();
            resources.Remove(resource.FullName.ToLower());
            foreach (Asset asset in assets)
            {
                this.assets.Remove(asset.guid);
            }

            return true;
        }

        public bool SetResourceLoadType(string name, string variant, LoadType loadType)
        {
            if (!IsValidResourceName(name, variant))
            {
                return false;
            }

            Resource resource = GetResource(name, variant);
            if (resource == null)
            {
                return false;
            }

            if ((loadType == LoadType.LoadFromBinary || loadType == LoadType.LoadFromBinaryAndQuickDecrypt || loadType == LoadType.LoadFromBinaryAndDecrypt) && resource.GetAssets().Length > 1)
            {
                return false;
            }

            resource.LoadType = loadType;
            return true;
        }

        public bool SetResourcePacked(string name, string variant, bool packed)
        {
            if (!IsValidResourceName(name, variant))
            {
                return false;
            }

            Resource resource = GetResource(name, variant);
            if (resource == null)
            {
                return false;
            }

            resource.Packed = packed;
            return true;
        }

        public Asset[] GetAssets()
        {
            return assets.Values.ToArray();
        }

        public Asset[] GetAssets(string name, string variant)
        {
            if (!IsValidResourceName(name, variant))
            {
                return new Asset[0];
            }

            Resource resource = GetResource(name, variant);
            if (resource == null)
            {
                return new Asset[0];
            }

            return resource.GetAssets();
        }

        public Asset GetAsset(string guid)
        {
            if (string.IsNullOrEmpty(guid))
            {
                return null;
            }

            Asset asset = null;
            if (assets.TryGetValue(guid, out asset))
            {
                return asset;
            }

            return null;
        }

        public bool HasAsset(string guid)
        {
            if (string.IsNullOrEmpty(guid))
            {
                return false;
            }

            return assets.ContainsKey(guid);
        }

        public bool AssignAsset(string guid, string name, string variant)
        {
            if (string.IsNullOrEmpty(guid))
            {
                return false;
            }

            if (!IsValidResourceName(name, variant))
            {
                return false;
            }

            Resource resource = GetResource(name, variant);
            if (resource == null)
            {
                return false;
            }

            string assetName = AssetDatabase.GUIDToAssetPath(guid);
            if (string.IsNullOrEmpty(assetName))
            {
                return false;
            }

            Asset[] assetsInResource = resource.GetAssets();
            foreach (Asset assetInResource in assetsInResource)
            {
                if (assetInResource.Name == assetName)
                {
                    continue;
                }

                if (assetInResource.Name.ToLower() == assetName.ToLower())
                {
                    return false;
                }
            }

            bool isScene = assetName.EndsWith(SceneExtension, StringComparison.Ordinal);
            if (isScene && resource.AssetType == AssetType.Asset || !isScene && resource.AssetType == AssetType.Scene)
            {
                return false;
            }

            Asset asset = GetAsset(guid);
            if (resource.IsLoadFromBinary && assetsInResource.Length > 0 && asset != assetsInResource[0])
            {
                return false;
            }

            if (asset == null)
            {
                asset = Asset.ValueOf(guid);
                assets.Add(asset.guid, asset);
            }

            resource.AssignAsset(asset, isScene);

            return true;
        }

        public bool UnassignAsset(string guid)
        {
            if (string.IsNullOrEmpty(guid))
            {
                return false;
            }

            Asset asset = GetAsset(guid);
            if (asset != null)
            {
                asset.resource.UnassignAsset(asset);
                assets.Remove(asset.guid);
            }

            return true;
        }

        private string GetResourceFullName(string name, string variant)
        {
            return !string.IsNullOrEmpty(variant) ? StringUtils.Format("{}.{}", name, variant) : name;
        }

        private bool IsValidResourceName(string name, string variant)
        {
            if (string.IsNullOrEmpty(name))
            {
                return false;
            }

            if (!ResourceNameRegex.IsMatch(name))
            {
                return false;
            }

            if (variant != null && !ResourceVariantRegex.IsMatch(variant))
            {
                return false;
            }

            return true;
        }

        private bool IsAvailableResourceName(string name, string variant, Resource current)
        {
            Resource found = GetResource(name, variant);
            if (found != null && found != current)
            {
                return false;
            }

            string[] foundPathNames = name.Split('/');
            foreach (Resource resource in resources.Values)
            {
                if (current != null && resource == current)
                {
                    continue;
                }

                if (resource.Name == name)
                {
                    if (resource.Variant == null && variant != null)
                    {
                        return false;
                    }

                    if (resource.Variant != null && variant == null)
                    {
                        return false;
                    }
                }

                if (resource.Name.Length > name.Length
                    && resource.Name.IndexOf(name, StringComparison.CurrentCultureIgnoreCase) == 0
                    && resource.Name[name.Length] == '/')
                {
                    return false;
                }

                if (name.Length > resource.Name.Length
                    && name.IndexOf(resource.Name, StringComparison.CurrentCultureIgnoreCase) == 0
                    && name[resource.Name.Length] == '/')
                {
                    return false;
                }

                string[] pathNames = resource.Name.Split('/');
                for (int i = 0; i < foundPathNames.Length - 1 && i < pathNames.Length - 1; i++)
                {
                    if (foundPathNames[i].ToLower() != pathNames[i].ToLower())
                    {
                        break;
                    }

                    if (foundPathNames[i] != pathNames[i])
                    {
                        return false;
                    }
                }
            }

            return true;
        }
    }
}
