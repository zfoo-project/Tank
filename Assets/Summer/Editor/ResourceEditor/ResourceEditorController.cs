using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using Summer.Base.Model;
using Summer.Editor.ResourceCollection;
using Spring.Util;
using Summer.Editor.ResourceEditor.Model;
using Summer.Resource.Model.Constant;
using UnityEditor;

namespace Summer.Editor.ResourceEditor
{
    public sealed class ResourceEditorController
    {
        private const string DefaultSourceAssetRootPath = "Assets";

        private readonly string configurationPath;
        private readonly ResourceCollection.ResourceCollection resourceCollection;
        private readonly List<string> sourceAssetSearchPaths;
        private readonly List<string> sourceAssetSearchRelativePaths;
        private readonly Dictionary<string, SourceAsset> sourceAssets;
        private SourceFolder sourceAssetRoot;
        private string sourceAssetRootPath;
        private string sourceAssetUnionTypeFilter;
        private string sourceAssetUnionLabelFilter;
        private string sourceAssetExceptTypeFilter;
        private string sourceAssetExceptLabelFilter;
        private AssetSorterType assetSorter;

        public ResourceEditorController()
        {
            configurationPath = (string) AssemblyUtils.GetAllFieldsByAttribute<ResourceEditorConfigPathAttribute>().First().GetValue(null);
            resourceCollection = new ResourceCollection.ResourceCollection();
            resourceCollection.OnLoadingResource += delegate(int index, int count)
            {
                if (OnLoadingResource != null)
                {
                    OnLoadingResource(index, count);
                }
            };

            resourceCollection.OnLoadingAsset += delegate(int index, int count)
            {
                if (OnLoadingAsset != null)
                {
                    OnLoadingAsset(index, count);
                }
            };

            resourceCollection.OnLoadCompleted += delegate()
            {
                if (OnLoadCompleted != null)
                {
                    OnLoadCompleted();
                }
            };

            sourceAssetSearchPaths = new List<string>();
            sourceAssetSearchRelativePaths = new List<string>();
            sourceAssets = new Dictionary<string, SourceAsset>(StringComparer.Ordinal);
            sourceAssetRoot = null;
            sourceAssetRootPath = null;
            sourceAssetUnionTypeFilter = null;
            sourceAssetUnionLabelFilter = null;
            sourceAssetExceptTypeFilter = null;
            sourceAssetExceptLabelFilter = null;
            assetSorter = AssetSorterType.Path;

            SourceAssetRootPath = DefaultSourceAssetRootPath;
        }

        public int ResourceCount
        {
            get { return resourceCollection.ResourceCount; }
        }

        public int AssetCount
        {
            get { return resourceCollection.AssetCount; }
        }

        public SourceFolder SourceAssetRoot
        {
            get { return sourceAssetRoot; }
        }

        public string SourceAssetRootPath
        {
            get { return sourceAssetRootPath; }
            set
            {
                if (sourceAssetRootPath == value)
                {
                    return;
                }

                sourceAssetRootPath = value.Replace('\\', '/');
                sourceAssetRoot = new SourceFolder(sourceAssetRootPath, null);
                RefreshSourceAssetSearchPaths();
            }
        }

        public string SourceAssetUnionTypeFilter
        {
            get { return sourceAssetUnionTypeFilter; }
            set
            {
                if (sourceAssetUnionTypeFilter == value)
                {
                    return;
                }

                sourceAssetUnionTypeFilter = value;
            }
        }

        public string SourceAssetUnionLabelFilter
        {
            get { return sourceAssetUnionLabelFilter; }
            set
            {
                if (sourceAssetUnionLabelFilter == value)
                {
                    return;
                }

                sourceAssetUnionLabelFilter = value;
            }
        }

        public string SourceAssetExceptTypeFilter
        {
            get { return sourceAssetExceptTypeFilter; }
            set
            {
                if (sourceAssetExceptTypeFilter == value)
                {
                    return;
                }

                sourceAssetExceptTypeFilter = value;
            }
        }

        public string SourceAssetExceptLabelFilter
        {
            get { return sourceAssetExceptLabelFilter; }
            set
            {
                if (sourceAssetExceptLabelFilter == value)
                {
                    return;
                }

                sourceAssetExceptLabelFilter = value;
            }
        }

        public AssetSorterType AssetSorter
        {
            get { return assetSorter; }
            set
            {
                if (assetSorter == value)
                {
                    return;
                }

                assetSorter = value;
            }
        }

        public event GameFrameworkAction<int, int> OnLoadingResource = null;

        public event GameFrameworkAction<int, int> OnLoadingAsset = null;

        public event GameFrameworkAction OnLoadCompleted = null;

        public event GameFrameworkAction<SourceAsset[]> OnAssetAssigned = null;

        public event GameFrameworkAction<SourceAsset[]> OnAssetUnassigned = null;

        public bool Load()
        {
            if (!File.Exists(configurationPath))
            {
                return false;
            }

            try
            {
                XmlDocument xmlDocument = new XmlDocument();
                xmlDocument.Load(configurationPath);
                XmlNode xmlRoot = xmlDocument.SelectSingleNode("Summer");
                XmlNode xmlEditor = xmlRoot.SelectSingleNode("ResourceEditor");
                XmlNode xmlSettings = xmlEditor.SelectSingleNode("Settings");

                XmlNodeList xmlNodeList = null;
                XmlNode xmlNode = null;

                xmlNodeList = xmlSettings.ChildNodes;
                for (int i = 0; i < xmlNodeList.Count; i++)
                {
                    xmlNode = xmlNodeList.Item(i);
                    switch (xmlNode.Name)
                    {
                        case "SourceAssetRootPath":
                            SourceAssetRootPath = xmlNode.InnerText;
                            break;

                        case "SourceAssetSearchPaths":
                            sourceAssetSearchRelativePaths.Clear();
                            XmlNodeList xmlNodeListInner = xmlNode.ChildNodes;
                            XmlNode xmlNodeInner = null;
                            for (int j = 0; j < xmlNodeListInner.Count; j++)
                            {
                                xmlNodeInner = xmlNodeListInner.Item(j);
                                if (xmlNodeInner.Name != "SourceAssetSearchPath")
                                {
                                    continue;
                                }

                                sourceAssetSearchRelativePaths.Add(xmlNodeInner.Attributes.GetNamedItem("RelativePath").Value);
                            }

                            break;

                        case "SourceAssetUnionTypeFilter":
                            SourceAssetUnionTypeFilter = xmlNode.InnerText;
                            break;

                        case "SourceAssetUnionLabelFilter":
                            SourceAssetUnionLabelFilter = xmlNode.InnerText;
                            break;

                        case "SourceAssetExceptTypeFilter":
                            SourceAssetExceptTypeFilter = xmlNode.InnerText;
                            break;

                        case "SourceAssetExceptLabelFilter":
                            SourceAssetExceptLabelFilter = xmlNode.InnerText;
                            break;

                        case "AssetSorter":
                            AssetSorter = (AssetSorterType) Enum.Parse(typeof(AssetSorterType), xmlNode.InnerText);
                            break;
                    }
                }

                RefreshSourceAssetSearchPaths();
            }
            catch
            {
                File.Delete(configurationPath);
                return false;
            }

            ScanSourceAssets();

            resourceCollection.Load();

            return true;
        }

        public bool Save()
        {
            try
            {
                XmlDocument xmlDocument = new XmlDocument();
                xmlDocument.AppendChild(xmlDocument.CreateXmlDeclaration("1.0", "UTF-8", null));

                XmlElement xmlRoot = xmlDocument.CreateElement("Summer");
                xmlDocument.AppendChild(xmlRoot);

                XmlElement xmlEditor = xmlDocument.CreateElement("ResourceEditor");
                xmlRoot.AppendChild(xmlEditor);

                XmlElement xmlSettings = xmlDocument.CreateElement("Settings");
                xmlEditor.AppendChild(xmlSettings);

                XmlElement xmlElement = null;
                XmlAttribute xmlAttribute = null;

                xmlElement = xmlDocument.CreateElement("SourceAssetRootPath");
                xmlElement.InnerText = SourceAssetRootPath.ToString();
                xmlSettings.AppendChild(xmlElement);

                xmlElement = xmlDocument.CreateElement("SourceAssetSearchPaths");
                xmlSettings.AppendChild(xmlElement);

                foreach (string sourceAssetSearchRelativePath in sourceAssetSearchRelativePaths)
                {
                    XmlElement xmlElementInner = xmlDocument.CreateElement("SourceAssetSearchPath");
                    xmlAttribute = xmlDocument.CreateAttribute("RelativePath");
                    xmlAttribute.Value = sourceAssetSearchRelativePath;
                    xmlElementInner.Attributes.SetNamedItem(xmlAttribute);
                    xmlElement.AppendChild(xmlElementInner);
                }

                xmlElement = xmlDocument.CreateElement("SourceAssetUnionTypeFilter");
                xmlElement.InnerText = SourceAssetUnionTypeFilter ?? string.Empty;
                xmlSettings.AppendChild(xmlElement);
                xmlElement = xmlDocument.CreateElement("SourceAssetUnionLabelFilter");
                xmlElement.InnerText = SourceAssetUnionLabelFilter ?? string.Empty;
                xmlSettings.AppendChild(xmlElement);
                xmlElement = xmlDocument.CreateElement("SourceAssetExceptTypeFilter");
                xmlElement.InnerText = SourceAssetExceptTypeFilter ?? string.Empty;
                xmlSettings.AppendChild(xmlElement);
                xmlElement = xmlDocument.CreateElement("SourceAssetExceptLabelFilter");
                xmlElement.InnerText = SourceAssetExceptLabelFilter ?? string.Empty;
                xmlSettings.AppendChild(xmlElement);
                xmlElement = xmlDocument.CreateElement("AssetSorter");
                xmlElement.InnerText = AssetSorter.ToString();
                xmlSettings.AppendChild(xmlElement);

                string configurationDirectoryName = Path.GetDirectoryName(configurationPath);
                if (!Directory.Exists(configurationDirectoryName))
                {
                    Directory.CreateDirectory(configurationDirectoryName);
                }

                xmlDocument.Save(configurationPath);
                AssetDatabase.Refresh();
            }
            catch
            {
                if (File.Exists(configurationPath))
                {
                    File.Delete(configurationPath);
                }

                return false;
            }

            return resourceCollection.Save();
        }

        public ResourceCollection.Resource[] GetResources()
        {
            return resourceCollection.GetResources();
        }

        public ResourceCollection.Resource GetResource(string name, string variant)
        {
            return resourceCollection.GetResource(name, variant);
        }

        public bool HasResource(string name, string variant)
        {
            return resourceCollection.HasResource(name, variant);
        }

        public bool AddResource(string name, string variant, string fileSystem, LoadType loadType, bool packed)
        {
            return resourceCollection.AddResource(name, variant, fileSystem, loadType, packed);
        }

        public bool RenameResource(string oldName, string oldVariant, string newName, string newVariant)
        {
            return resourceCollection.RenameResource(oldName, oldVariant, newName, newVariant);
        }

        public bool RemoveResource(string name, string variant)
        {
            Asset[] assetsToRemove = resourceCollection.GetAssets(name, variant);
            if (resourceCollection.RemoveResource(name, variant))
            {
                List<SourceAsset> unassignedSourceAssets = new List<SourceAsset>();
                foreach (Asset asset in assetsToRemove)
                {
                    SourceAsset sourceAsset = GetSourceAsset(asset.guid);
                    if (sourceAsset != null)
                    {
                        unassignedSourceAssets.Add(sourceAsset);
                    }
                }

                if (OnAssetUnassigned != null)
                {
                    OnAssetUnassigned(unassignedSourceAssets.ToArray());
                }

                return true;
            }

            return false;
        }

        public bool SetResourceLoadType(string name, string variant, LoadType loadType)
        {
            return resourceCollection.SetResourceLoadType(name, variant, loadType);
        }

        public bool SetResourcePacked(string name, string variant, bool packed)
        {
            return resourceCollection.SetResourcePacked(name, variant, packed);
        }

        public int RemoveUnusedResources()
        {
            List<ResourceCollection.Resource> resources = new List<ResourceCollection.Resource>(resourceCollection.GetResources());
            List<ResourceCollection.Resource> removeResources = resources.FindAll(resource => GetAssets(resource.Name, resource.Variant).Length <= 0);
            foreach (ResourceCollection.Resource removeResource in removeResources)
            {
                resourceCollection.RemoveResource(removeResource.Name, removeResource.Variant);
            }

            return removeResources.Count;
        }

        public Asset[] GetAssets(string name, string variant)
        {
            List<Asset> assets = new List<Asset>(resourceCollection.GetAssets(name, variant));
            switch (AssetSorter)
            {
                case AssetSorterType.Path:
                    assets.Sort(AssetPathComparer);
                    break;

                case AssetSorterType.Name:
                    assets.Sort(AssetNameComparer);
                    break;

                case AssetSorterType.Guid:
                    assets.Sort(AssetGuidComparer);
                    break;
            }

            return assets.ToArray();
        }

        public Asset GetAsset(string guid)
        {
            return resourceCollection.GetAsset(guid);
        }

        public bool AssignAsset(string guid, string name, string variant)
        {
            if (resourceCollection.AssignAsset(guid, name, variant))
            {
                if (OnAssetAssigned != null)
                {
                    OnAssetAssigned(new SourceAsset[] {GetSourceAsset(guid)});
                }

                return true;
            }

            return false;
        }

        public bool UnassignAsset(string guid)
        {
            if (resourceCollection.UnassignAsset(guid))
            {
                SourceAsset sourceAsset = GetSourceAsset(guid);
                if (sourceAsset != null)
                {
                    if (OnAssetUnassigned != null)
                    {
                        OnAssetUnassigned(new SourceAsset[] {sourceAsset});
                    }
                }

                return true;
            }

            return false;
        }

        public int RemoveUnknownAssets()
        {
            List<Asset> assets = new List<Asset>(resourceCollection.GetAssets());
            List<Asset> removeAssets = assets.FindAll(asset => GetSourceAsset(asset.guid) == null);
            foreach (Asset asset in removeAssets)
            {
                resourceCollection.UnassignAsset(asset.guid);
            }

            return removeAssets.Count;
        }

        public SourceAsset[] GetSourceAssets()
        {
            int count = 0;
            SourceAsset[] sourceAssets = new SourceAsset[this.sourceAssets.Count];
            foreach (KeyValuePair<string, SourceAsset> sourceAsset in this.sourceAssets)
            {
                sourceAssets[count++] = sourceAsset.Value;
            }

            return sourceAssets;
        }

        public SourceAsset GetSourceAsset(string guid)
        {
            if (string.IsNullOrEmpty(guid))
            {
                return null;
            }

            SourceAsset sourceAsset = null;
            if (sourceAssets.TryGetValue(guid, out sourceAsset))
            {
                return sourceAsset;
            }

            return null;
        }

        public void ScanSourceAssets()
        {
            sourceAssets.Clear();
            sourceAssetRoot.Clear();

            string[] sourceAssetSearchPaths = this.sourceAssetSearchPaths.ToArray();
            HashSet<string> tempGuids = new HashSet<string>();
            tempGuids.UnionWith(AssetDatabase.FindAssets(SourceAssetUnionTypeFilter, sourceAssetSearchPaths));
            tempGuids.UnionWith(AssetDatabase.FindAssets(SourceAssetUnionLabelFilter, sourceAssetSearchPaths));
            tempGuids.ExceptWith(AssetDatabase.FindAssets(SourceAssetExceptTypeFilter, sourceAssetSearchPaths));
            tempGuids.ExceptWith(AssetDatabase.FindAssets(SourceAssetExceptLabelFilter, sourceAssetSearchPaths));

            string[] guids = new List<string>(tempGuids).ToArray();
            foreach (string guid in guids)
            {
                string fullPath = AssetDatabase.GUIDToAssetPath(guid);
                if (AssetDatabase.IsValidFolder(fullPath))
                {
                    // Skip folder.
                    continue;
                }

                string assetPath = fullPath.Substring(SourceAssetRootPath.Length + 1);
                string[] splitedPath = assetPath.Split('/');
                SourceFolder folder = sourceAssetRoot;
                for (int i = 0; i < splitedPath.Length - 1; i++)
                {
                    SourceFolder subFolder = folder.GetFolder(splitedPath[i]);
                    folder = subFolder == null ? folder.AddFolder(splitedPath[i]) : subFolder;
                }

                SourceAsset asset = folder.AddAsset(guid, fullPath, splitedPath[splitedPath.Length - 1]);
                sourceAssets.Add(asset.guid, asset);
            }
        }

        private void RefreshSourceAssetSearchPaths()
        {
            sourceAssetSearchPaths.Clear();

            if (string.IsNullOrEmpty(sourceAssetRootPath))
            {
                SourceAssetRootPath = DefaultSourceAssetRootPath;
            }

            if (sourceAssetSearchRelativePaths.Count > 0)
            {
                foreach (string sourceAssetSearchRelativePath in sourceAssetSearchRelativePaths)
                {
                    sourceAssetSearchPaths.Add(PathUtils.GetRegularPath(Path.Combine(sourceAssetRootPath, sourceAssetSearchRelativePath)));
                }
            }
            else
            {
                sourceAssetSearchPaths.Add(sourceAssetRootPath);
            }
        }

        private int AssetPathComparer(Asset a, Asset b)
        {
            SourceAsset sourceAssetA = GetSourceAsset(a.guid);
            SourceAsset sourceAssetB = GetSourceAsset(b.guid);

            if (sourceAssetA != null && sourceAssetB != null)
            {
                return sourceAssetA.path.CompareTo(sourceAssetB.path);
            }

            if (sourceAssetA == null && sourceAssetB == null)
            {
                return a.guid.CompareTo(b.guid);
            }

            if (sourceAssetA == null)
            {
                return -1;
            }

            if (sourceAssetB == null)
            {
                return 1;
            }

            return 0;
        }

        private int AssetNameComparer(Asset a, Asset b)
        {
            SourceAsset sourceAssetA = GetSourceAsset(a.guid);
            SourceAsset sourceAssetB = GetSourceAsset(b.guid);

            if (sourceAssetA != null && sourceAssetB != null)
            {
                return sourceAssetA.name.CompareTo(sourceAssetB.name);
            }

            if (sourceAssetA == null && sourceAssetB == null)
            {
                return a.guid.CompareTo(b.guid);
            }

            if (sourceAssetA == null)
            {
                return -1;
            }

            if (sourceAssetB == null)
            {
                return 1;
            }

            return 0;
        }

        private int AssetGuidComparer(Asset a, Asset b)
        {
            SourceAsset sourceAssetA = GetSourceAsset(a.guid);
            SourceAsset sourceAssetB = GetSourceAsset(b.guid);

            if (sourceAssetA != null && sourceAssetB != null || sourceAssetA == null && sourceAssetB == null)
            {
                return a.guid.CompareTo(b.guid);
            }

            if (sourceAssetA == null)
            {
                return -1;
            }

            if (sourceAssetB == null)
            {
                return 1;
            }

            return 0;
        }
    }
}