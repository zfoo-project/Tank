using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;
using Spring.Util;
using Summer.Base.Model;

namespace Summer.Editor.ResourceBuilder.Model
{
    public sealed class BuildReport
    {
        private const string BuildReportName = "BuildReport.xml";
        private const string BuildLogName = "BuildLog.txt";

        private string buildReportName = null;
        private string buildLogName = null;
        private string productName = null;
        private string companyName = null;
        private string gameIdentifier = null;
        private string unityVersion = null;
        private string applicableGameVersion = null;
        private int internalResourceVersion = 0;
        private Platform platforms = Platform.Undefined;
        private bool zipSelected = false;
        private int buildAssetBundleOptions = 0;
        private StringBuilder logBuilder = null;
        private SortedDictionary<string, ResourceData> resourceDatas = null;

        public void Initialize(string buildReportPath, string productName, string companyName, string gameIdentifier, string unityVersion, string applicableGameVersion, int internalResourceVersion,
            Platform platforms, bool zipSelected, int buildAssetBundleOptions, SortedDictionary<string, ResourceData> resourceDatas)
        {
            if (string.IsNullOrEmpty(buildReportPath))
            {
                throw new GameFrameworkException("Build report path is invalid.");
            }

            buildReportName = PathUtils.GetRegularPath(Path.Combine(buildReportPath, BuildReportName));
            buildLogName = PathUtils.GetRegularPath(Path.Combine(buildReportPath, BuildLogName));
            this.productName = productName;
            this.companyName = companyName;
            this.gameIdentifier = gameIdentifier;
            this.unityVersion = unityVersion;
            this.applicableGameVersion = applicableGameVersion;
            this.internalResourceVersion = internalResourceVersion;
            this.platforms = platforms;
            this.zipSelected = zipSelected;
            this.buildAssetBundleOptions = buildAssetBundleOptions;
            logBuilder = new StringBuilder();
            this.resourceDatas = resourceDatas;
        }

        public void LogInfo(string format, params object[] args)
        {
            LogInternal("INFO", format, args);
        }

        public void LogWarning(string format, params object[] args)
        {
            LogInternal("WARNING", format, args);
        }

        public void LogError(string format, params object[] args)
        {
            LogInternal("ERROR", format, args);
        }

        public void LogFatal(string format, params object[] args)
        {
            LogInternal("FATAL", format, args);
        }

        public void SaveReport()
        {
            XmlElement xmlElement = null;
            XmlAttribute xmlAttribute = null;

            XmlDocument xmlDocument = new XmlDocument();
            xmlDocument.AppendChild(xmlDocument.CreateXmlDeclaration("1.0", "UTF-8", null));

            XmlElement xmlRoot = xmlDocument.CreateElement("Summer");
            xmlDocument.AppendChild(xmlRoot);

            XmlElement xmlBuildReport = xmlDocument.CreateElement("BuildReport");
            xmlRoot.AppendChild(xmlBuildReport);

            XmlElement xmlSummary = xmlDocument.CreateElement("Summary");
            xmlBuildReport.AppendChild(xmlSummary);

            xmlElement = xmlDocument.CreateElement("ProductName");
            xmlElement.InnerText = productName;
            xmlSummary.AppendChild(xmlElement);
            xmlElement = xmlDocument.CreateElement("CompanyName");
            xmlElement.InnerText = companyName;
            xmlSummary.AppendChild(xmlElement);
            xmlElement = xmlDocument.CreateElement("GameIdentifier");
            xmlElement.InnerText = gameIdentifier;
            xmlSummary.AppendChild(xmlElement);
            xmlSummary.AppendChild(xmlElement);
            xmlElement = xmlDocument.CreateElement("UnityVersion");
            xmlElement.InnerText = unityVersion;
            xmlSummary.AppendChild(xmlElement);
            xmlElement = xmlDocument.CreateElement("ApplicableGameVersion");
            xmlElement.InnerText = applicableGameVersion;
            xmlSummary.AppendChild(xmlElement);
            xmlElement = xmlDocument.CreateElement("InternalResourceVersion");
            xmlElement.InnerText = internalResourceVersion.ToString();
            xmlSummary.AppendChild(xmlElement);
            xmlElement = xmlDocument.CreateElement("Platforms");
            xmlElement.InnerText = platforms.ToString();
            xmlSummary.AppendChild(xmlElement);
            xmlElement = xmlDocument.CreateElement("ZipSelected");
            xmlElement.InnerText = zipSelected.ToString();
            xmlSummary.AppendChild(xmlElement);
            xmlElement = xmlDocument.CreateElement("BuildAssetBundleOptions");
            xmlElement.InnerText = buildAssetBundleOptions.ToString();
            xmlSummary.AppendChild(xmlElement);

            XmlElement xmlResources = xmlDocument.CreateElement("Resources");
            xmlAttribute = xmlDocument.CreateAttribute("Count");
            xmlAttribute.Value = resourceDatas.Count.ToString();
            xmlResources.Attributes.SetNamedItem(xmlAttribute);
            xmlBuildReport.AppendChild(xmlResources);
            foreach (ResourceData resourceData in resourceDatas.Values)
            {
                XmlElement xmlResource = xmlDocument.CreateElement("Resource");
                xmlAttribute = xmlDocument.CreateAttribute("Name");
                xmlAttribute.Value = resourceData.name;
                xmlResource.Attributes.SetNamedItem(xmlAttribute);
                if (resourceData.variant != null)
                {
                    xmlAttribute = xmlDocument.CreateAttribute("Variant");
                    xmlAttribute.Value = resourceData.variant;
                    xmlResource.Attributes.SetNamedItem(xmlAttribute);
                }

                xmlAttribute = xmlDocument.CreateAttribute("Extension");
                xmlAttribute.Value = ResourceBuilderController.GetExtension(resourceData);
                xmlResource.Attributes.SetNamedItem(xmlAttribute);

                if (resourceData.fileSystem != null)
                {
                    xmlAttribute = xmlDocument.CreateAttribute("FileSystem");
                    xmlAttribute.Value = resourceData.fileSystem;
                    xmlResource.Attributes.SetNamedItem(xmlAttribute);
                }

                xmlAttribute = xmlDocument.CreateAttribute("LoadType");
                xmlAttribute.Value = ((byte) resourceData.loadType).ToString();
                xmlResource.Attributes.SetNamedItem(xmlAttribute);
                xmlAttribute = xmlDocument.CreateAttribute("Packed");
                xmlAttribute.Value = resourceData.packed.ToString();
                xmlResource.Attributes.SetNamedItem(xmlAttribute);
                string[] resourceGroups = resourceData.GetResourceGroups();
                if (resourceGroups.Length > 0)
                {
                    xmlAttribute = xmlDocument.CreateAttribute("ResourceGroups");
                    xmlAttribute.Value = string.Join(",", resourceGroups);
                    xmlResource.Attributes.SetNamedItem(xmlAttribute);
                }

                xmlResources.AppendChild(xmlResource);

                AssetData[] assetDatas = resourceData.GetAssetDatas();
                XmlElement xmlAssets = xmlDocument.CreateElement("Assets");
                xmlAttribute = xmlDocument.CreateAttribute("Count");
                xmlAttribute.Value = assetDatas.Length.ToString();
                xmlAssets.Attributes.SetNamedItem(xmlAttribute);
                xmlResource.AppendChild(xmlAssets);
                foreach (AssetData assetData in assetDatas)
                {
                    XmlElement xmlAsset = xmlDocument.CreateElement("Asset");
                    xmlAttribute = xmlDocument.CreateAttribute("Guid");
                    xmlAttribute.Value = assetData.guid;
                    xmlAsset.Attributes.SetNamedItem(xmlAttribute);
                    xmlAttribute = xmlDocument.CreateAttribute("Name");
                    xmlAttribute.Value = assetData.name;
                    xmlAsset.Attributes.SetNamedItem(xmlAttribute);
                    xmlAttribute = xmlDocument.CreateAttribute("Length");
                    xmlAttribute.Value = assetData.length.ToString();
                    xmlAsset.Attributes.SetNamedItem(xmlAttribute);
                    xmlAttribute = xmlDocument.CreateAttribute("HashCode");
                    xmlAttribute.Value = assetData.hashCode.ToString();
                    xmlAsset.Attributes.SetNamedItem(xmlAttribute);
                    xmlAssets.AppendChild(xmlAsset);
                    string[] dependencyAssetNames = assetData.dependencyAssetNames;
                    if (dependencyAssetNames.Length > 0)
                    {
                        XmlElement xmlDependencyAssets = xmlDocument.CreateElement("DependencyAssets");
                        xmlAttribute = xmlDocument.CreateAttribute("Count");
                        xmlAttribute.Value = dependencyAssetNames.Length.ToString();
                        xmlDependencyAssets.Attributes.SetNamedItem(xmlAttribute);
                        xmlAsset.AppendChild(xmlDependencyAssets);
                        foreach (string dependencyAssetName in dependencyAssetNames)
                        {
                            XmlElement xmlDependencyAsset = xmlDocument.CreateElement("DependencyAsset");
                            xmlAttribute = xmlDocument.CreateAttribute("Name");
                            xmlAttribute.Value = dependencyAssetName;
                            xmlDependencyAsset.Attributes.SetNamedItem(xmlAttribute);
                            xmlDependencyAssets.AppendChild(xmlDependencyAsset);
                        }
                    }
                }

                XmlElement xmlCodes = xmlDocument.CreateElement("Codes");
                xmlResource.AppendChild(xmlCodes);
                foreach (ResourceCode resourceCode in resourceData.GetCodes())
                {
                    XmlElement xmlCode = xmlDocument.CreateElement(resourceCode.platform.ToString());
                    xmlAttribute = xmlDocument.CreateAttribute("Length");
                    xmlAttribute.Value = resourceCode.length.ToString();
                    xmlCode.Attributes.SetNamedItem(xmlAttribute);
                    xmlAttribute = xmlDocument.CreateAttribute("HashCode");
                    xmlAttribute.Value = resourceCode.hashCode.ToString();
                    xmlCode.Attributes.SetNamedItem(xmlAttribute);
                    xmlAttribute = xmlDocument.CreateAttribute("ZipLength");
                    xmlAttribute.Value = resourceCode.zipLength.ToString();
                    xmlCode.Attributes.SetNamedItem(xmlAttribute);
                    xmlAttribute = xmlDocument.CreateAttribute("ZipHashCode");
                    xmlAttribute.Value = resourceCode.zipHashCode.ToString();
                    xmlCode.Attributes.SetNamedItem(xmlAttribute);
                    xmlCodes.AppendChild(xmlCode);
                }
            }

            xmlDocument.Save(buildReportName);
            File.WriteAllText(buildLogName, logBuilder.ToString());
        }

        private void LogInternal(string type, string format, object[] args)
        {
            logBuilder.Append("[");
            logBuilder.Append(DateTime.UtcNow.ToString("HH:mm:ss.fff"));
            logBuilder.Append("][");
            logBuilder.Append(type);
            logBuilder.Append("] ");
            logBuilder.Append(StringUtils.Format(format, args));
            logBuilder.AppendLine();
        }
    }
}