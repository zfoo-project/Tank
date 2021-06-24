using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using MiniKing.Script.Config;
using Spring.Util;
using Spring.Util.Json;
using Spring.Util.Math;
using Spring.Util.Security;
using Spring.Util.Zip;
using Summer.Base.Model;
using Summer.Editor.ResourceAnalyzer;
using Summer.Editor.ResourceAnalyzer.Model;
using Summer.Editor.ResourceBuilder.Handler;
using Summer.Editor.ResourceBuilder.Model;
using Summer.Editor.ResourceCollection;
using Summer.FileSystem;
using Summer.FileSystem.Model;
using Summer.Resource;
using Summer.Resource.Model.Constant;
using Summer.Resource.Model.LocalVersion;
using Summer.Resource.Model.PackageVersion;
using Summer.Resource.Model.UpdatableVersion;
using UnityEditor;
using UnityEngine;

namespace Summer.Editor.ResourceBuilder
{
    public sealed class ResourceBuilderController
    {
        private const string RemoteVersionListFileName = "GameFrameworkVersion.dat";
        private const string LocalVersionListFileName = "GameFrameworkList.dat";
        private const string DefaultExtension = "dat";
        private const string NoneOptionName = "<None>";
        private static readonly int AssetsStringLength = "Assets".Length;

        private readonly string configurationPath;
        private readonly ResourceCollection.ResourceCollection resourceCollection;
        private readonly ResourceAnalyzerController resourceAnalyzerController;
        private readonly SortedDictionary<string, ResourceData> resourceDatas;
        private readonly Dictionary<string, IFileSystem> outputPackageFileSystems;
        private readonly Dictionary<string, IFileSystem> outputPackedFileSystems;
        private readonly BuildReport buildReport;
        private readonly List<string> buildEventHandlerTypeNames;
        private IBuildEventHandler buildEventHandler;
        private IFileSystemManager fileSystemManager;

        public ResourceBuilderController()
        {
            configurationPath = (string) AssemblyUtils.GetAllFieldsByAttribute<ResourceBuilderConfigPathAttribute>().First().GetValue(null);

            resourceCollection = new ResourceCollection.ResourceCollection();
            ZipUtils.SetZipHelper(new DefaultZipHelper());

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

            resourceAnalyzerController = new ResourceAnalyzerController(resourceCollection);

            resourceAnalyzerController.OnAnalyzingAsset += delegate(int index, int count)
            {
                if (OnAnalyzingAsset != null)
                {
                    OnAnalyzingAsset(index, count);
                }
            };

            resourceAnalyzerController.OnAnalyzeCompleted += delegate()
            {
                if (OnAnalyzeCompleted != null)
                {
                    OnAnalyzeCompleted();
                }
            };

            resourceDatas = new SortedDictionary<string, ResourceData>(StringComparer.Ordinal);
            outputPackageFileSystems = new Dictionary<string, IFileSystem>(StringComparer.Ordinal);
            outputPackedFileSystems = new Dictionary<string, IFileSystem>(StringComparer.Ordinal);
            buildReport = new BuildReport();

            buildEventHandlerTypeNames = new List<string>
            {
                NoneOptionName
            };

            buildEventHandlerTypeNames.AddRange(AssemblyUtils.GetAllSubClassNames(typeof(IBuildEventHandler)));
            buildEventHandler = null;
            fileSystemManager = null;

            Platforms = Platform.Undefined;
            ZipSelected = true;
            DeterministicAssetBundleSelected = ChunkBasedCompressionSelected = true;
            UncompressedAssetBundleSelected = DisableWriteTypeTreeSelected = ForceRebuildAssetBundleSelected = IgnoreTypeTreeChangesSelected = AppendHashToAssetBundleNameSelected = false;
            OutputPackageSelected = OutputFullSelected = OutputPackedSelected = true;
            BuildEventHandlerTypeName = string.Empty;
            OutputDirectory = string.Empty;
        }

        public string ProductName
        {
            get { return PlayerSettings.productName; }
        }

        public string CompanyName
        {
            get { return PlayerSettings.companyName; }
        }

        public string GameIdentifier
        {
            get { return PlayerSettings.applicationIdentifier; }
        }

        public string UnityVersion
        {
            get { return Application.unityVersion; }
        }

        public string ApplicableGameVersion
        {
            get { return Application.version; }
        }

        public int InternalResourceVersion { get; set; }

        public Platform Platforms { get; set; }

        public bool ZipSelected { get; set; }

        public bool UncompressedAssetBundleSelected { get; set; }

        public bool DisableWriteTypeTreeSelected { get; set; }

        public bool DeterministicAssetBundleSelected { get; set; }

        public bool ForceRebuildAssetBundleSelected { get; set; }

        public bool IgnoreTypeTreeChangesSelected { get; set; }

        public bool AppendHashToAssetBundleNameSelected { get; set; }

        public bool ChunkBasedCompressionSelected { get; set; }

        public bool OutputPackageSelected { get; set; }

        public bool OutputFullSelected { get; set; }

        public bool OutputPackedSelected { get; set; }

        public string BuildEventHandlerTypeName { get; set; }

        public string OutputDirectory { get; set; }

        public bool IsValidOutputDirectory
        {
            get
            {
                if (string.IsNullOrEmpty(OutputDirectory))
                {
                    return false;
                }

                if (!Directory.Exists(OutputDirectory))
                {
                    return false;
                }

                return true;
            }
        }

        public string WorkingPath
        {
            get
            {
                if (!IsValidOutputDirectory)
                {
                    return string.Empty;
                }

                return PathUtils.GetRegularPath(new DirectoryInfo(StringUtils.Format("{}/Working/", OutputDirectory)).FullName);
            }
        }

        public string OutputPackagePath
        {
            get
            {
                if (!IsValidOutputDirectory)
                {
                    return string.Empty;
                }

                return PathUtils.GetRegularPath(new DirectoryInfo(StringUtils.Format("{}/Package/{}_{}/", OutputDirectory, ApplicableGameVersion.Replace('.', '_'), InternalResourceVersion.ToString())).FullName);
            }
        }

        public string OutputFullPath
        {
            get
            {
                if (!IsValidOutputDirectory)
                {
                    return string.Empty;
                }

                return PathUtils.GetRegularPath(new DirectoryInfo(StringUtils.Format("{}/Full/{}_{}/", OutputDirectory, ApplicableGameVersion.Replace('.', '_'), InternalResourceVersion.ToString())).FullName);
            }
        }

        public string OutputPackedPath
        {
            get
            {
                if (!IsValidOutputDirectory)
                {
                    return string.Empty;
                }

                return PathUtils.GetRegularPath(new DirectoryInfo(StringUtils.Format("{}/Packed/{}_{}/", OutputDirectory, ApplicableGameVersion.Replace('.', '_'), InternalResourceVersion.ToString())).FullName);
            }
        }

        public string BuildReportPath
        {
            get
            {
                if (!IsValidOutputDirectory)
                {
                    return string.Empty;
                }

                return PathUtils.GetRegularPath(new DirectoryInfo(StringUtils.Format("{}/BuildReport/{}_{}/", OutputDirectory, ApplicableGameVersion.Replace('.', '_'), InternalResourceVersion.ToString())).FullName);
            }
        }

        public event GameFrameworkAction<int, int> OnLoadingResource = null;

        public event GameFrameworkAction<int, int> OnLoadingAsset = null;

        public event GameFrameworkAction OnLoadCompleted = null;

        public event GameFrameworkAction<int, int> OnAnalyzingAsset = null;

        public event GameFrameworkAction OnAnalyzeCompleted = null;

        public event GameFrameworkFunc<string, float, bool> ProcessingAssetBundle = null;

        public event GameFrameworkFunc<string, float, bool> ProcessingBinary = null;

        public event GameFrameworkAction<Platform> ProcessResourceComplete = null;

        public event GameFrameworkAction<string> BuildResourceError = null;

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
                XmlNode xmlEditor = xmlRoot.SelectSingleNode("ResourceBuilder");
                XmlNode xmlSettings = xmlEditor.SelectSingleNode("Settings");

                XmlNodeList xmlNodeList = null;
                XmlNode xmlNode = null;

                xmlNodeList = xmlSettings.ChildNodes;
                for (int i = 0; i < xmlNodeList.Count; i++)
                {
                    xmlNode = xmlNodeList.Item(i);
                    switch (xmlNode.Name)
                    {
                        case "InternalResourceVersion":
                            InternalResourceVersion = int.Parse(xmlNode.InnerText) + 1;
                            break;

                        case "Platforms":
                            Platforms = (Platform) int.Parse(xmlNode.InnerText);
                            break;

                        case "ZipSelected":
                            ZipSelected = bool.Parse(xmlNode.InnerText);
                            break;

                        case "UncompressedAssetBundleSelected":
                            UncompressedAssetBundleSelected = bool.Parse(xmlNode.InnerText);
                            if (UncompressedAssetBundleSelected)
                            {
                                ChunkBasedCompressionSelected = false;
                            }

                            break;

                        case "DisableWriteTypeTreeSelected":
                            DisableWriteTypeTreeSelected = bool.Parse(xmlNode.InnerText);
                            if (DisableWriteTypeTreeSelected)
                            {
                                IgnoreTypeTreeChangesSelected = false;
                            }

                            break;

                        case "DeterministicAssetBundleSelected":
                            DeterministicAssetBundleSelected = bool.Parse(xmlNode.InnerText);
                            break;

                        case "ForceRebuildAssetBundleSelected":
                            ForceRebuildAssetBundleSelected = bool.Parse(xmlNode.InnerText);
                            break;

                        case "IgnoreTypeTreeChangesSelected":
                            IgnoreTypeTreeChangesSelected = bool.Parse(xmlNode.InnerText);
                            if (IgnoreTypeTreeChangesSelected)
                            {
                                DisableWriteTypeTreeSelected = false;
                            }

                            break;

                        case "AppendHashToAssetBundleNameSelected":
                            AppendHashToAssetBundleNameSelected = false;
                            break;

                        case "ChunkBasedCompressionSelected":
                            ChunkBasedCompressionSelected = bool.Parse(xmlNode.InnerText);
                            if (ChunkBasedCompressionSelected)
                            {
                                UncompressedAssetBundleSelected = false;
                            }

                            break;

                        case "OutputPackageSelected":
                            OutputPackageSelected = bool.Parse(xmlNode.InnerText);
                            break;

                        case "OutputFullSelected":
                            OutputFullSelected = bool.Parse(xmlNode.InnerText);
                            break;

                        case "OutputPackedSelected":
                            OutputPackedSelected = bool.Parse(xmlNode.InnerText);
                            break;

                        case "BuildEventHandlerTypeName":
                            BuildEventHandlerTypeName = xmlNode.InnerText;
                            RefreshBuildEventHandler();
                            break;

                        case "OutputDirectory":
                            OutputDirectory = xmlNode.InnerText;
                            break;
                    }
                }
            }
            catch
            {
                File.Delete(configurationPath);
                return false;
            }

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

                XmlElement xmlBuilder = xmlDocument.CreateElement("ResourceBuilder");
                xmlRoot.AppendChild(xmlBuilder);

                XmlElement xmlSettings = xmlDocument.CreateElement("Settings");
                xmlBuilder.AppendChild(xmlSettings);

                XmlElement xmlElement = null;

                xmlElement = xmlDocument.CreateElement("InternalResourceVersion");
                xmlElement.InnerText = InternalResourceVersion.ToString();
                xmlSettings.AppendChild(xmlElement);
                xmlElement = xmlDocument.CreateElement("Platforms");
                xmlElement.InnerText = ((int) Platforms).ToString();
                xmlSettings.AppendChild(xmlElement);
                xmlElement = xmlDocument.CreateElement("ZipSelected");
                xmlElement.InnerText = ZipSelected.ToString();
                xmlSettings.AppendChild(xmlElement);
                xmlElement = xmlDocument.CreateElement("UncompressedAssetBundleSelected");
                xmlElement.InnerText = UncompressedAssetBundleSelected.ToString();
                xmlSettings.AppendChild(xmlElement);
                xmlElement = xmlDocument.CreateElement("DisableWriteTypeTreeSelected");
                xmlElement.InnerText = DisableWriteTypeTreeSelected.ToString();
                xmlSettings.AppendChild(xmlElement);
                xmlElement = xmlDocument.CreateElement("DeterministicAssetBundleSelected");
                xmlElement.InnerText = DeterministicAssetBundleSelected.ToString();
                xmlSettings.AppendChild(xmlElement);
                xmlElement = xmlDocument.CreateElement("ForceRebuildAssetBundleSelected");
                xmlElement.InnerText = ForceRebuildAssetBundleSelected.ToString();
                xmlSettings.AppendChild(xmlElement);
                xmlElement = xmlDocument.CreateElement("IgnoreTypeTreeChangesSelected");
                xmlElement.InnerText = IgnoreTypeTreeChangesSelected.ToString();
                xmlSettings.AppendChild(xmlElement);
                xmlElement = xmlDocument.CreateElement("AppendHashToAssetBundleNameSelected");
                xmlElement.InnerText = AppendHashToAssetBundleNameSelected.ToString();
                xmlSettings.AppendChild(xmlElement);
                xmlElement = xmlDocument.CreateElement("ChunkBasedCompressionSelected");
                xmlElement.InnerText = ChunkBasedCompressionSelected.ToString();
                xmlSettings.AppendChild(xmlElement);
                xmlElement = xmlDocument.CreateElement("OutputPackageSelected");
                xmlElement.InnerText = OutputPackageSelected.ToString();
                xmlSettings.AppendChild(xmlElement);
                xmlElement = xmlDocument.CreateElement("OutputFullSelected");
                xmlElement.InnerText = OutputFullSelected.ToString();
                xmlSettings.AppendChild(xmlElement);
                xmlElement = xmlDocument.CreateElement("OutputPackedSelected");
                xmlElement.InnerText = OutputPackedSelected.ToString();
                xmlSettings.AppendChild(xmlElement);
                xmlElement = xmlDocument.CreateElement("BuildEventHandlerTypeName");
                xmlElement.InnerText = BuildEventHandlerTypeName;
                xmlSettings.AppendChild(xmlElement);
                xmlElement = xmlDocument.CreateElement("OutputDirectory");
                xmlElement.InnerText = OutputDirectory;
                xmlSettings.AppendChild(xmlElement);

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

        public void SetBuildEventHandler(IBuildEventHandler buildEventHandler)
        {
            this.buildEventHandler = buildEventHandler;
        }

        public string[] GetBuildEventHandlerTypeNames()
        {
            return buildEventHandlerTypeNames.ToArray();
        }

        public bool IsPlatformSelected(Platform platform)
        {
            return (Platforms & platform) != 0;
        }

        public void SelectPlatform(Platform platform, bool selected)
        {
            if (selected)
            {
                Platforms |= platform;
            }
            else
            {
                Platforms &= ~platform;
            }
        }

        public bool RefreshBuildEventHandler()
        {
            bool retVal = false;
            if (!string.IsNullOrEmpty(BuildEventHandlerTypeName) && buildEventHandlerTypeNames.Contains(BuildEventHandlerTypeName))
            {
                Type buildEventHandlerType = AssemblyUtils.GetTypeByName(BuildEventHandlerTypeName);
                if (buildEventHandlerType != null)
                {
                    IBuildEventHandler buildEventHandler = (IBuildEventHandler) Activator.CreateInstance(buildEventHandlerType);
                    if (buildEventHandler != null)
                    {
                        SetBuildEventHandler(buildEventHandler);
                        return true;
                    }
                }
            }
            else
            {
                retVal = true;
            }

            BuildEventHandlerTypeName = string.Empty;
            SetBuildEventHandler(null);
            return retVal;
        }

        public bool BuildResources()
        {
            if (!IsValidOutputDirectory)
            {
                return false;
            }

            if (Directory.Exists(OutputPackagePath))
            {
                Directory.Delete(OutputPackagePath, true);
            }

            Directory.CreateDirectory(OutputPackagePath);

            if (Directory.Exists(OutputFullPath))
            {
                Directory.Delete(OutputFullPath, true);
            }

            Directory.CreateDirectory(OutputFullPath);

            if (Directory.Exists(OutputPackedPath))
            {
                Directory.Delete(OutputPackedPath, true);
            }

            Directory.CreateDirectory(OutputPackedPath);

            if (Directory.Exists(BuildReportPath))
            {
                Directory.Delete(BuildReportPath, true);
            }

            Directory.CreateDirectory(BuildReportPath);

            BuildAssetBundleOptions buildAssetBundleOptions = GetBuildAssetBundleOptions();
            buildReport.Initialize(BuildReportPath, ProductName, CompanyName, GameIdentifier, UnityVersion, ApplicableGameVersion, InternalResourceVersion,
                Platforms, ZipSelected, (int) buildAssetBundleOptions, resourceDatas);

            try
            {
                buildReport.LogInfo("Build Start Time: {}", DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss.fff"));

                if (buildEventHandler != null)
                {
                    buildReport.LogInfo("Execute build event handler 'OnPreprocessAllPlatforms'...");
                    buildEventHandler.OnPreprocessAllPlatforms(ProductName, CompanyName, GameIdentifier, UnityVersion, ApplicableGameVersion, InternalResourceVersion, buildAssetBundleOptions, ZipSelected,
                        OutputDirectory, WorkingPath, OutputPackageSelected, OutputPackagePath, OutputFullSelected, OutputFullPath, OutputPackedSelected, OutputPackedPath, BuildReportPath);
                }

                buildReport.LogInfo("Start prepare resource collection...");
                if (!resourceCollection.Load())
                {
                    buildReport.LogError("Can not parse 'ResourceCollection.xml', please use 'Resource Editor' tool first.");

                    if (buildEventHandler != null)
                    {
                        buildReport.LogInfo("Execute build event handler 'OnPostprocessAllPlatforms'...");
                        buildEventHandler.OnPostprocessAllPlatforms(ProductName, CompanyName, GameIdentifier, UnityVersion, ApplicableGameVersion, InternalResourceVersion, buildAssetBundleOptions, ZipSelected,
                            OutputDirectory, WorkingPath, OutputPackageSelected, OutputPackagePath, OutputFullSelected, OutputFullPath, OutputPackedSelected, OutputPackedPath, BuildReportPath);
                    }

                    buildReport.SaveReport();
                    return false;
                }

                if (Platforms == Platform.Undefined)
                {
                    buildReport.LogError("Platform undefined.");

                    if (buildEventHandler != null)
                    {
                        buildReport.LogInfo("Execute build event handler 'OnPostprocessAllPlatforms'...");
                        buildEventHandler.OnPostprocessAllPlatforms(ProductName, CompanyName, GameIdentifier, UnityVersion, ApplicableGameVersion, InternalResourceVersion, buildAssetBundleOptions, ZipSelected,
                            OutputDirectory, WorkingPath, OutputPackageSelected, OutputPackagePath, OutputFullSelected, OutputFullPath, OutputPackedSelected, OutputPackedPath, BuildReportPath);
                    }

                    buildReport.SaveReport();
                    return false;
                }

                buildReport.LogInfo("Prepare resource collection complete.");
                buildReport.LogInfo("Start analyze assets dependency...");

                resourceAnalyzerController.Analyze();

                buildReport.LogInfo("Analyze assets dependency complete.");
                buildReport.LogInfo("Start prepare build data...");

                AssetBundleBuild[] assetBundleBuildDatas = null;
                ResourceData[] assetBundleResourceDatas = null;
                ResourceData[] binaryResourceDatas = null;
                if (!PrepareBuildData(out assetBundleBuildDatas, out assetBundleResourceDatas, out binaryResourceDatas))
                {
                    buildReport.LogError("Prepare resource build data failure.");

                    if (buildEventHandler != null)
                    {
                        buildReport.LogInfo("Execute build event handler 'OnPostprocessAllPlatforms'...");
                        buildEventHandler.OnPostprocessAllPlatforms(ProductName, CompanyName, GameIdentifier, UnityVersion, ApplicableGameVersion, InternalResourceVersion, buildAssetBundleOptions, ZipSelected,
                            OutputDirectory, WorkingPath, OutputPackageSelected, OutputPackagePath, OutputFullSelected, OutputFullPath, OutputPackedSelected, OutputPackedPath, BuildReportPath);
                    }

                    buildReport.SaveReport();
                    return false;
                }

                buildReport.LogInfo("Prepare resource build data complete.");
                buildReport.LogInfo("Start build resources for selected platforms...");

                bool watchResult = buildEventHandler == null || !buildEventHandler.ContinueOnFailure;
                bool isSuccess = false;
                isSuccess = BuildResources(Platform.Windows, assetBundleBuildDatas, buildAssetBundleOptions, assetBundleResourceDatas, binaryResourceDatas);

                if (!watchResult || isSuccess)
                {
                    isSuccess = BuildResources(Platform.Windows64, assetBundleBuildDatas, buildAssetBundleOptions, assetBundleResourceDatas, binaryResourceDatas);
                }

                if (!watchResult || isSuccess)
                {
                    isSuccess = BuildResources(Platform.MacOS, assetBundleBuildDatas, buildAssetBundleOptions, assetBundleResourceDatas, binaryResourceDatas);
                }

                if (!watchResult || isSuccess)
                {
                    isSuccess = BuildResources(Platform.Linux, assetBundleBuildDatas, buildAssetBundleOptions, assetBundleResourceDatas, binaryResourceDatas);
                }

                if (!watchResult || isSuccess)
                {
                    isSuccess = BuildResources(Platform.IOS, assetBundleBuildDatas, buildAssetBundleOptions, assetBundleResourceDatas, binaryResourceDatas);
                }

                if (!watchResult || isSuccess)
                {
                    isSuccess = BuildResources(Platform.Android, assetBundleBuildDatas, buildAssetBundleOptions, assetBundleResourceDatas, binaryResourceDatas);
                }

                if (!watchResult || isSuccess)
                {
                    isSuccess = BuildResources(Platform.WindowsStore, assetBundleBuildDatas, buildAssetBundleOptions, assetBundleResourceDatas, binaryResourceDatas);
                }

                if (!watchResult || isSuccess)
                {
                    isSuccess = BuildResources(Platform.WebGL, assetBundleBuildDatas, buildAssetBundleOptions, assetBundleResourceDatas, binaryResourceDatas);
                }

                if (buildEventHandler != null)
                {
                    buildReport.LogInfo("Execute build event handler 'OnPostprocessAllPlatforms'...");
                    buildEventHandler.OnPostprocessAllPlatforms(ProductName, CompanyName, GameIdentifier, UnityVersion, ApplicableGameVersion, InternalResourceVersion, buildAssetBundleOptions, ZipSelected,
                        OutputDirectory, WorkingPath, OutputPackageSelected, OutputPackagePath, OutputFullSelected, OutputFullPath, OutputPackedSelected, OutputPackedPath, BuildReportPath);
                }

                buildReport.LogInfo("Build resources for selected platforms complete.");
                buildReport.SaveReport();

                return true;
            }
            catch (Exception exception)
            {
                string errorMessage = exception.ToString();
                buildReport.LogFatal(errorMessage);
                buildReport.SaveReport();
                if (BuildResourceError != null)
                {
                    BuildResourceError(errorMessage);
                }

                return false;
            }
            finally
            {
                outputPackageFileSystems.Clear();
                outputPackedFileSystems.Clear();
                if (fileSystemManager != null)
                {
                    fileSystemManager = null;
                }
            }
        }

        private bool BuildResources(Platform platform, AssetBundleBuild[] assetBundleBuildDatas, BuildAssetBundleOptions buildAssetBundleOptions, ResourceData[] assetBundleResourceDatas, ResourceData[] binaryResourceDatas)
        {
            if (!IsPlatformSelected(platform))
            {
                return true;
            }

            string platformName = platform.ToString();
            buildReport.LogInfo("Start build resources for '{}'...", platformName);

            string workingPath = StringUtils.Format("{}{}/", WorkingPath, platformName);
            buildReport.LogInfo("Working path is '{}'.", workingPath);

            string outputPackagePath = StringUtils.Format("{}{}/", OutputPackagePath, platformName);
            if (OutputPackageSelected)
            {
                Directory.CreateDirectory(outputPackagePath);
                buildReport.LogInfo("Output package is selected, path is '{}'.", outputPackagePath);
            }
            else
            {
                buildReport.LogInfo("Output package is not selected.");
            }

            string outputFullPath = StringUtils.Format("{}{}/", OutputFullPath, platformName);
            if (OutputFullSelected)
            {
                Directory.CreateDirectory(outputFullPath);
                buildReport.LogInfo("Output full is selected, path is '{}'.", outputFullPath);
            }
            else
            {
                buildReport.LogInfo("Output full is not selected.");
            }

            string outputPackedPath = StringUtils.Format("{}{}/", OutputPackedPath, platformName);
            if (OutputPackedSelected)
            {
                Directory.CreateDirectory(outputPackedPath);
                buildReport.LogInfo("Output packed is selected, path is '{}'.", outputPackedPath);
            }
            else
            {
                buildReport.LogInfo("Output packed is not selected.");
            }

            // Clean working path
            List<string> validNames = new List<string>();
            foreach (ResourceData assetBundleResourceData in assetBundleResourceDatas)
            {
                validNames.Add(GetResourceFullName(assetBundleResourceData.name, assetBundleResourceData.variant).ToLower());
            }

            if (Directory.Exists(workingPath))
            {
                Uri workingUri = new Uri(workingPath, UriKind.Absolute);
                string[] fileNames = Directory.GetFiles(workingPath, "*", SearchOption.AllDirectories);
                foreach (string fileName in fileNames)
                {
                    if (fileName.EndsWith(".manifest", StringComparison.Ordinal))
                    {
                        continue;
                    }

                    string relativeName = workingUri.MakeRelativeUri(new Uri(fileName, UriKind.Absolute)).ToString();
                    if (!validNames.Contains(relativeName))
                    {
                        File.Delete(fileName);
                    }
                }

                string[] manifestNames = Directory.GetFiles(workingPath, "*.manifest", SearchOption.AllDirectories);
                foreach (string manifestName in manifestNames)
                {
                    if (!File.Exists(manifestName.Substring(0, manifestName.LastIndexOf('.'))))
                    {
                        File.Delete(manifestName);
                    }
                }

                PathUtils.RemoveEmptyDirectory(workingPath);
            }

            if (!Directory.Exists(workingPath))
            {
                Directory.CreateDirectory(workingPath);
            }

            if (buildEventHandler != null)
            {
                buildReport.LogInfo("Execute build event handler 'OnPreprocessPlatform' for '{}'...", platformName);
                buildEventHandler.OnPreprocessPlatform(platform, workingPath, OutputPackageSelected, outputPackagePath, OutputFullSelected, outputFullPath, OutputPackedSelected, outputPackedPath);
            }

            // Build AssetBundles
            buildReport.LogInfo("Unity start build asset bundles for '{}'...", platformName);
            AssetBundleManifest assetBundleManifest = BuildPipeline.BuildAssetBundles(workingPath, assetBundleBuildDatas, buildAssetBundleOptions, GetBuildTarget(platform));
            if (assetBundleManifest == null)
            {
                buildReport.LogError("Build asset bundles for '{}' failure.", platformName);

                if (buildEventHandler != null)
                {
                    buildReport.LogInfo("Execute build event handler 'OnPostprocessPlatform' for '{}'...", platformName);
                    buildEventHandler.OnPostprocessPlatform(platform, workingPath, OutputPackageSelected, outputPackagePath, OutputFullSelected, outputFullPath, OutputPackedSelected, outputPackedPath, false);
                }

                return false;
            }

            if (buildEventHandler != null)
            {
                buildReport.LogInfo("Execute build event handler 'OnBuildAssetBundlesComplete' for '{}'...", platformName);
                buildEventHandler.OnBuildAssetBundlesComplete(platform, workingPath, OutputPackageSelected, outputPackagePath, OutputFullSelected, outputFullPath, OutputPackedSelected, outputPackedPath, assetBundleManifest);
            }

            buildReport.LogInfo("Unity build asset bundles for '{}' complete.", platformName);

            // Create FileSystems
            buildReport.LogInfo("Start create file system for '{}'...", platformName);

            if (OutputPackageSelected)
            {
                CreateFileSystems(resourceDatas.Values, outputPackagePath, outputPackageFileSystems);
            }

            if (OutputPackedSelected)
            {
                CreateFileSystems(GetPackedResourceDatas(), outputPackedPath, outputPackedFileSystems);
            }

            buildReport.LogInfo("Create file system for '{}' complete.", platformName);

            // Process AssetBundles
            for (int i = 0; i < assetBundleResourceDatas.Length; i++)
            {
                string fullName = GetResourceFullName(assetBundleResourceDatas[i].name, assetBundleResourceDatas[i].variant);
                if (ProcessingAssetBundle != null)
                {
                    if (ProcessingAssetBundle(fullName, (float) (i + 1) / assetBundleResourceDatas.Length))
                    {
                        buildReport.LogWarning("The build has been canceled by user.");

                        if (buildEventHandler != null)
                        {
                            buildReport.LogInfo("Execute build event handler 'OnPostprocessPlatform' for '{}'...", platformName);
                            buildEventHandler.OnPostprocessPlatform(platform, workingPath, OutputPackageSelected, outputPackagePath, OutputFullSelected, outputFullPath, OutputPackedSelected, outputPackedPath, false);
                        }

                        return false;
                    }
                }

                buildReport.LogInfo("Start process asset bundle '{}' for '{}'...", fullName, platformName);

                if (!ProcessAssetBundle(platform, workingPath, outputPackagePath, outputFullPath, outputPackedPath, ZipSelected, assetBundleResourceDatas[i].name, assetBundleResourceDatas[i].variant, assetBundleResourceDatas[i].fileSystem))
                {
                    return false;
                }

                buildReport.LogInfo("Process asset bundle '{}' for '{}' complete.", fullName, platformName);
            }

            // Process Binaries
            for (int i = 0; i < binaryResourceDatas.Length; i++)
            {
                string fullName = GetResourceFullName(binaryResourceDatas[i].name, binaryResourceDatas[i].variant);
                if (ProcessingBinary != null)
                {
                    if (ProcessingBinary(fullName, (float) (i + 1) / binaryResourceDatas.Length))
                    {
                        buildReport.LogWarning("The build has been canceled by user.");

                        if (buildEventHandler != null)
                        {
                            buildReport.LogInfo("Execute build event handler 'OnPostprocessPlatform' for '{}'...", platformName);
                            buildEventHandler.OnPostprocessPlatform(platform, workingPath, OutputPackageSelected, outputPackagePath, OutputFullSelected, outputFullPath, OutputPackedSelected, outputPackedPath, false);
                        }

                        return false;
                    }
                }

                buildReport.LogInfo("Start process binary '{}' for '{}'...", fullName, platformName);

                if (!ProcessBinary(platform, workingPath, outputPackagePath, outputFullPath, outputPackedPath, ZipSelected, binaryResourceDatas[i].name, binaryResourceDatas[i].variant, binaryResourceDatas[i].fileSystem))
                {
                    return false;
                }

                buildReport.LogInfo("Process binary '{}' for '{}' complete.", fullName, platformName);
            }

            if (OutputPackageSelected)
            {
                ProcessPackageVersionList(outputPackagePath, platform);
                buildReport.LogInfo("Process package version list for '{}' complete.", platformName);
            }

            if (OutputFullSelected)
            {
                VersionListData versionListData = ProcessUpdatableVersionList(outputFullPath, platform);
                buildReport.LogInfo("Process updatable version list for '{}' complete, updatable version list path is '{}', length is '{}', hash code is '{}[0x{}]', zip length is '{}', zip hash code is '{}[0x{}]'."
                    , platformName, versionListData.path, versionListData.length.ToString(), versionListData.hashCode, NumberUtils.ToHex(versionListData.hashCode), versionListData.zipLength.ToString(), versionListData.zipHashCode,
                    NumberUtils.ToHex(versionListData.zipHashCode));
                buildReport.LogInfo(JsonUtils.object2String(VersionInfoBundle.ValueOf(StringUtils.Format("/{}_{}", ApplicableGameVersion.Replace('.', '_'), InternalResourceVersion.ToString())
                    , InternalResourceVersion, versionListData.length, versionListData.hashCode, versionListData.zipLength, versionListData.zipHashCode)));
                buildReport.LogInfo(outputFullPath);
                buildReport.LogInfo(outputPackagePath);
                buildReport.LogInfo(OutputDirectory);
                if (buildEventHandler != null)
                {
                    buildReport.LogInfo("Execute build event handler 'OnOutputUpdatableVersionListData' for '{}'...", platformName);
                    buildEventHandler.OnOutputUpdatableVersionListData(platform, versionListData.path, versionListData.length, versionListData.hashCode, versionListData.zipLength, versionListData.zipHashCode);
                }
            }

            if (OutputPackedSelected)
            {
                ProcessReadOnlyVersionList(outputPackedPath, platform);
                buildReport.LogInfo("Process read only version list for '{}' complete.", platformName);
            }

            if (buildEventHandler != null)
            {
                buildReport.LogInfo("Execute build event handler 'OnPostprocessPlatform' for '{}'...", platformName);
                buildEventHandler.OnPostprocessPlatform(platform, workingPath, OutputPackageSelected, outputPackagePath, OutputFullSelected, outputFullPath, OutputPackedSelected, outputPackedPath, true);
            }

            if (ProcessResourceComplete != null)
            {
                ProcessResourceComplete(platform);
            }

            buildReport.LogInfo("Build resources for '{}' success.", platformName);
            return true;
        }

        private bool ProcessAssetBundle(Platform platform, string workingPath, string outputPackagePath, string outputFullPath, string outputPackedPath, bool zip, string name, string variant, string fileSystem)
        {
            string fullName = GetResourceFullName(name, variant);
            ResourceData resourceData = resourceDatas[fullName];
            string workingName = PathUtils.GetRegularPath(Path.Combine(workingPath, fullName.ToLower()));

            byte[] bytes = File.ReadAllBytes(workingName);
            int length = bytes.Length;
            int hashCode = Crc32Utils.GetCrc32(bytes);
            int zipLength = length;
            int zipHashCode = hashCode;

            byte[] hashBytes = ConverterUtils.GetBytes(hashCode);
            if (resourceData.loadType == LoadType.LoadFromMemoryAndQuickDecrypt)
            {
                bytes = EncryptionUtils.GetQuickXorBytes(bytes, hashBytes);
            }
            else if (resourceData.loadType == LoadType.LoadFromMemoryAndDecrypt)
            {
                bytes = EncryptionUtils.GetXorBytes(bytes, hashBytes);
            }

            return ProcessOutput(platform, outputPackagePath, outputFullPath, outputPackedPath, zip, name, variant, fileSystem, resourceData, bytes, length, hashCode, zipLength, zipHashCode);
        }

        private bool ProcessBinary(Platform platform, string workingPath, string outputPackagePath, string outputFullPath, string outputPackedPath, bool zip, string name, string variant, string fileSystem)
        {
            string fullName = GetResourceFullName(name, variant);
            ResourceData resourceData = resourceDatas[fullName];
            string assetName = resourceData.GetAssetNames()[0];
            string assetPath = PathUtils.GetRegularPath(Application.dataPath.Substring(0, Application.dataPath.Length - AssetsStringLength) + assetName);

            byte[] bytes = File.ReadAllBytes(assetPath);
            int length = bytes.Length;
            int hashCode = Crc32Utils.GetCrc32(bytes);
            int zipLength = length;
            int zipHashCode = hashCode;

            byte[] hashBytes = ConverterUtils.GetBytes(hashCode);
            if (resourceData.loadType == LoadType.LoadFromBinaryAndQuickDecrypt)
            {
                bytes = EncryptionUtils.GetQuickXorBytes(bytes, hashBytes);
            }
            else if (resourceData.loadType == LoadType.LoadFromBinaryAndDecrypt)
            {
                bytes = EncryptionUtils.GetXorBytes(bytes, hashBytes);
            }

            return ProcessOutput(platform, outputPackagePath, outputFullPath, outputPackedPath, zip, name, variant, fileSystem, resourceData, bytes, length, hashCode, zipLength, zipHashCode);
        }

        private void ProcessPackageVersionList(string outputPackagePath, Platform platform)
        {
            Asset[] originalAssets = resourceCollection.GetAssets();
            PackageVersionAsset[] assets = new PackageVersionAsset[originalAssets.Length];
            for (int i = 0; i < assets.Length; i++)
            {
                Asset originalAsset = originalAssets[i];
                assets[i] = new PackageVersionAsset(originalAsset.Name, GetDependencyAssetIndexes(originalAsset.Name));
            }

            SortedDictionary<string, ResourceData>.ValueCollection resourceDatas = this.resourceDatas.Values;

            int index = 0;
            PackageVersionResource[] resources = new PackageVersionResource[resourceCollection.ResourceCount];
            foreach (ResourceData resourceData in resourceDatas)
            {
                ResourceCode resourceCode = resourceData.GetCode(platform);
                resources[index++] = new PackageVersionResource(resourceData.name, resourceData.variant, GetExtension(resourceData), (byte) resourceData.loadType, resourceCode.length, resourceCode.hashCode, GetAssetIndexes(resourceData));
            }

            string[] fileSystemNames = GetFileSystemNames(resourceDatas);
            PackageVersionFileSystem[] fileSystems = new PackageVersionFileSystem[fileSystemNames.Length];
            for (int i = 0; i < fileSystems.Length; i++)
            {
                fileSystems[i] = new PackageVersionFileSystem(fileSystemNames[i], GetResourceIndexesFromFileSystem(resourceDatas, fileSystemNames[i]));
            }

            string[] resourceGroupNames = GetResourceGroupNames(resourceDatas);
            PackageVersionResourceGroup[] resourceGroups = new PackageVersionResourceGroup[resourceGroupNames.Length];
            for (int i = 0; i < resourceGroups.Length; i++)
            {
                resourceGroups[i] = new PackageVersionResourceGroup(resourceGroupNames[i], GetResourceIndexesFromResourceGroup(resourceDatas, resourceGroupNames[i]));
            }

            PackageVersionList versionList = new PackageVersionList(ApplicableGameVersion, InternalResourceVersion, assets, resources, fileSystems, resourceGroups);
            PackageVersionListSerializer serializer = new PackageVersionListSerializer();
            string packageVersionListPath = PathUtils.GetRegularPath(Path.Combine(outputPackagePath, RemoteVersionListFileName));
            using (FileStream fileStream = new FileStream(packageVersionListPath, FileMode.Create, FileAccess.Write))
            {
                if (!serializer.Serialize(fileStream, versionList))
                {
                    throw new GameFrameworkException("Serialize package version list failure.");
                }
            }
        }

        private VersionListData ProcessUpdatableVersionList(string outputFullPath, Platform platform)
        {
            Asset[] originalAssets = resourceCollection.GetAssets();
            UpdatableVersionAsset[] assets = new UpdatableVersionAsset[originalAssets.Length];
            for (int i = 0; i < assets.Length; i++)
            {
                Asset originalAsset = originalAssets[i];
                assets[i] = new UpdatableVersionAsset(originalAsset.Name, GetDependencyAssetIndexes(originalAsset.Name));
            }

            SortedDictionary<string, ResourceData>.ValueCollection resourceDatas = this.resourceDatas.Values;

            int index = 0;
            UpdatableVersionResource[] resources = new UpdatableVersionResource[resourceCollection.ResourceCount];
            foreach (ResourceData resourceData in resourceDatas)
            {
                ResourceCode resourceCode = resourceData.GetCode(platform);
                resources[index++] = new UpdatableVersionResource(resourceData.name, resourceData.variant, GetExtension(resourceData), (byte) resourceData.loadType, resourceCode.length, resourceCode.hashCode, resourceCode.zipLength,
                    resourceCode.zipHashCode, GetAssetIndexes(resourceData));
            }

            string[] fileSystemNames = GetFileSystemNames(resourceDatas);
            UpdatableVersionFileSystem[] fileSystems = new UpdatableVersionFileSystem[fileSystemNames.Length];
            for (int i = 0; i < fileSystems.Length; i++)
            {
                fileSystems[i] = new UpdatableVersionFileSystem(fileSystemNames[i], GetResourceIndexesFromFileSystem(resourceDatas, fileSystemNames[i]));
            }

            string[] resourceGroupNames = GetResourceGroupNames(resourceDatas);
            UpdatableVersionResourceGroup[] resourceGroups = new UpdatableVersionResourceGroup[resourceGroupNames.Length];
            for (int i = 0; i < resourceGroups.Length; i++)
            {
                resourceGroups[i] = new UpdatableVersionResourceGroup(resourceGroupNames[i], GetResourceIndexesFromResourceGroup(resourceDatas, resourceGroupNames[i]));
            }

            UpdatableVersionList versionList = new UpdatableVersionList(ApplicableGameVersion, InternalResourceVersion, assets, resources, fileSystems, resourceGroups);
            UpdatableVersionListSerializer serializer = new UpdatableVersionListSerializer();
            string updatableVersionListPath = PathUtils.GetRegularPath(Path.Combine(outputFullPath, RemoteVersionListFileName));
            using (FileStream fileStream = new FileStream(updatableVersionListPath, FileMode.Create, FileAccess.Write))
            {
                if (!serializer.Serialize(fileStream, versionList))
                {
                    throw new GameFrameworkException("Serialize updatable version list failure.");
                }
            }

            byte[] bytes = File.ReadAllBytes(updatableVersionListPath);
            int length = bytes.Length;
            int hashCode = Crc32Utils.GetCrc32(bytes);
            bytes = ZipUtils.Compress(bytes);
            int zipLength = bytes.Length;
            File.WriteAllBytes(updatableVersionListPath, bytes);
            int zipHashCode = Crc32Utils.GetCrc32(bytes);
            int dotPosition = RemoteVersionListFileName.LastIndexOf('.');
            string versionListFullNameWithCrc32 = StringUtils.Format("{}.{}.{}", RemoteVersionListFileName.Substring(0, dotPosition), NumberUtils.ToHex(hashCode), RemoteVersionListFileName.Substring(dotPosition + 1));
            string updatableVersionListPathWithCrc32 = PathUtils.GetRegularPath(Path.Combine(outputFullPath, versionListFullNameWithCrc32));
            File.Move(updatableVersionListPath, updatableVersionListPathWithCrc32);

            return VersionListData.ValueOf(updatableVersionListPathWithCrc32, length, hashCode, zipLength, zipHashCode);
        }

        private void ProcessReadOnlyVersionList(string outputPackedPath, Platform platform)
        {
            ResourceData[] packedResourceDatas = GetPackedResourceDatas();

            LocalVersionResource[] resources = new LocalVersionResource[packedResourceDatas.Length];
            for (int i = 0; i < resources.Length; i++)
            {
                ResourceData resourceData = packedResourceDatas[i];
                ResourceCode resourceCode = resourceData.GetCode(platform);
                resources[i] = new LocalVersionResource(resourceData.name, resourceData.variant, GetExtension(resourceData), (byte) resourceData.loadType, resourceCode.length, resourceCode.hashCode);
            }

            string[] packedFileSystemNames = GetFileSystemNames(packedResourceDatas);
            LocalVersionFileSystem[] fileSystems = new LocalVersionFileSystem[packedFileSystemNames.Length];
            for (int i = 0; i < fileSystems.Length; i++)
            {
                fileSystems[i] = new LocalVersionFileSystem(packedFileSystemNames[i], GetResourceIndexesFromFileSystem(packedResourceDatas, packedFileSystemNames[i]));
            }

            LocalVersionList versionList = new LocalVersionList(resources, fileSystems);
            LocalVersionListSerializer serializer = new LocalVersionListSerializer();
            string localVersionListPath = PathUtils.GetRegularPath(Path.Combine(outputPackedPath, LocalVersionListFileName));
            using (FileStream fileStream = new FileStream(localVersionListPath, FileMode.Create, FileAccess.Write))
            {
                if (!serializer.Serialize(fileStream, versionList))
                {
                    throw new GameFrameworkException("Serialize read only version list failure.");
                }
            }
        }

        private int[] GetDependencyAssetIndexes(string assetName)
        {
            List<int> dependencyAssetIndexes = new List<int>();
            Asset[] assets = resourceCollection.GetAssets();
            DependencyData dependencyData = resourceAnalyzerController.GetDependencyData(assetName);
            foreach (Asset dependencyAsset in dependencyData.GetDependencyAssets())
            {
                for (int i = 0; i < assets.Length; i++)
                {
                    if (assets[i] == dependencyAsset)
                    {
                        dependencyAssetIndexes.Add(i);
                        break;
                    }
                }
            }

            dependencyAssetIndexes.Sort();
            return dependencyAssetIndexes.ToArray();
        }

        private int[] GetAssetIndexes(ResourceData resourceData)
        {
            Asset[] assets = resourceCollection.GetAssets();
            string[] assetGuids = resourceData.GetAssetGuids();
            int[] assetIndexes = new int[assetGuids.Length];
            for (int i = 0; i < assetGuids.Length; i++)
            {
                assetIndexes[i] = Array.BinarySearch(assets, resourceCollection.GetAsset(assetGuids[i]));
                if (assetIndexes[i] < 0)
                {
                    throw new GameFrameworkException("Asset is invalid.");
                }
            }

            return assetIndexes;
        }

        private ResourceData[] GetPackedResourceDatas()
        {
            List<ResourceData> packedResourceDatas = new List<ResourceData>();
            foreach (ResourceData resourceData in resourceDatas.Values)
            {
                if (!resourceData.packed)
                {
                    continue;
                }

                packedResourceDatas.Add(resourceData);
            }

            return packedResourceDatas.ToArray();
        }

        private string[] GetFileSystemNames(IEnumerable<ResourceData> resourceDatas)
        {
            HashSet<string> fileSystemNames = new HashSet<string>();
            foreach (ResourceData resourceData in resourceDatas)
            {
                if (resourceData.fileSystem == null)
                {
                    continue;
                }

                fileSystemNames.Add(resourceData.fileSystem);
            }

            return fileSystemNames.OrderBy(x => x).ToArray();
        }

        private int[] GetResourceIndexesFromFileSystem(IEnumerable<ResourceData> resourceDatas, string fileSystemName)
        {
            int index = 0;
            List<int> resourceIndexes = new List<int>();
            foreach (ResourceData resourceData in resourceDatas)
            {
                if (resourceData.fileSystem == fileSystemName)
                {
                    resourceIndexes.Add(index);
                }

                index++;
            }

            resourceIndexes.Sort();
            return resourceIndexes.ToArray();
        }

        private string[] GetResourceGroupNames(IEnumerable<ResourceData> resourceDatas)
        {
            HashSet<string> resourceGroupNames = new HashSet<string>();
            foreach (ResourceData resourceData in resourceDatas)
            {
                foreach (string resourceGroup in resourceData.GetResourceGroups())
                {
                    resourceGroupNames.Add(resourceGroup);
                }
            }

            return resourceGroupNames.OrderBy(x => x).ToArray();
        }

        private int[] GetResourceIndexesFromResourceGroup(IEnumerable<ResourceData> resourceDatas, string resourceGroupName)
        {
            int index = 0;
            List<int> resourceIndexes = new List<int>();
            foreach (ResourceData resourceData in resourceDatas)
            {
                foreach (string resourceGroup in resourceData.GetResourceGroups())
                {
                    if (resourceGroup == resourceGroupName)
                    {
                        resourceIndexes.Add(index);
                        break;
                    }
                }

                index++;
            }

            resourceIndexes.Sort();
            return resourceIndexes.ToArray();
        }

        private void CreateFileSystems(IEnumerable<ResourceData> resourceDatas, string outputPath, Dictionary<string, IFileSystem> outputFileSystem)
        {
            outputFileSystem.Clear();
            string[] fileSystemNames = GetFileSystemNames(resourceDatas);
            if (fileSystemNames.Length > 0 && fileSystemManager == null)
            {
                fileSystemManager = new FileSystemManager();
            }

            foreach (string fileSystemName in fileSystemNames)
            {
                int fileCount = GetResourceIndexesFromFileSystem(resourceDatas, fileSystemName).Length;
                string fullPath = PathUtils.GetRegularPath(Path.Combine(outputPath, StringUtils.Format("{}.{}", fileSystemName, DefaultExtension)));
                string directory = Path.GetDirectoryName(fullPath);
                if (!Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                IFileSystem fileSystem = fileSystemManager.CreateFileSystem(fullPath, FileSystemAccess.Write, fileCount, fileCount);
                outputFileSystem.Add(fileSystemName, fileSystem);
            }
        }

        private bool ProcessOutput(Platform platform, string outputPackagePath, string outputFullPath, string outputPackedPath, bool zip, string name, string variant, string fileSystem, ResourceData resourceData, byte[] bytes, int length,
            int hashCode, int zipLength, int zipHashCode)
        {
            string fullNameWithExtension = StringUtils.Format("{}.{}", GetResourceFullName(name, variant), GetExtension(resourceData));

            if (OutputPackageSelected)
            {
                if (string.IsNullOrEmpty(fileSystem))
                {
                    string packagePath = PathUtils.GetRegularPath(Path.Combine(outputPackagePath, fullNameWithExtension));
                    string packageDirectoryName = Path.GetDirectoryName(packagePath);
                    if (!Directory.Exists(packageDirectoryName))
                    {
                        Directory.CreateDirectory(packageDirectoryName);
                    }

                    File.WriteAllBytes(packagePath, bytes);
                }
                else
                {
                    if (!outputPackageFileSystems[fileSystem].WriteFile(fullNameWithExtension, bytes))
                    {
                        return false;
                    }
                }
            }

            if (OutputPackedSelected && resourceData.packed)
            {
                if (string.IsNullOrEmpty(fileSystem))
                {
                    string packedPath = PathUtils.GetRegularPath(Path.Combine(outputPackedPath, fullNameWithExtension));
                    string packedDirectoryName = Path.GetDirectoryName(packedPath);
                    if (!Directory.Exists(packedDirectoryName))
                    {
                        Directory.CreateDirectory(packedDirectoryName);
                    }

                    File.WriteAllBytes(packedPath, bytes);
                }
                else
                {
                    if (!outputPackedFileSystems[fileSystem].WriteFile(fullNameWithExtension, bytes))
                    {
                        return false;
                    }
                }
            }

            if (OutputFullSelected)
            {
                string fullNameWithCrc32AndExtension = variant != null
                    ? StringUtils.Format("{}.{}.{}.{}", name, variant, NumberUtils.ToHex(hashCode), DefaultExtension)
                    : StringUtils.Format("{}.{}.{}", name, NumberUtils.ToHex(hashCode), DefaultExtension);
                string fullPath = PathUtils.GetRegularPath(Path.Combine(outputFullPath, fullNameWithCrc32AndExtension));
                string fullDirectoryName = Path.GetDirectoryName(fullPath);
                if (!Directory.Exists(fullDirectoryName))
                {
                    Directory.CreateDirectory(fullDirectoryName);
                }

                if (zip)
                {
                    byte[] zipBytes = ZipUtils.Compress(bytes);
                    zipLength = zipBytes.Length;
                    zipHashCode = Crc32Utils.GetCrc32(zipBytes);
                    File.WriteAllBytes(fullPath, zipBytes);
                }
                else
                {
                    File.WriteAllBytes(fullPath, bytes);
                }
            }

            resourceData.AddCode(platform, length, hashCode, zipLength, zipHashCode);
            return true;
        }

        private BuildAssetBundleOptions GetBuildAssetBundleOptions()
        {
            BuildAssetBundleOptions buildOptions = BuildAssetBundleOptions.None;

            if (UncompressedAssetBundleSelected)
            {
                buildOptions |= BuildAssetBundleOptions.UncompressedAssetBundle;
            }

            if (DisableWriteTypeTreeSelected)
            {
                buildOptions |= BuildAssetBundleOptions.DisableWriteTypeTree;
            }

            if (DeterministicAssetBundleSelected)
            {
                buildOptions |= BuildAssetBundleOptions.DeterministicAssetBundle;
            }

            if (ForceRebuildAssetBundleSelected)
            {
                buildOptions |= BuildAssetBundleOptions.ForceRebuildAssetBundle;
            }

            if (IgnoreTypeTreeChangesSelected)
            {
                buildOptions |= BuildAssetBundleOptions.IgnoreTypeTreeChanges;
            }

            if (AppendHashToAssetBundleNameSelected)
            {
                buildOptions |= BuildAssetBundleOptions.AppendHashToAssetBundleName;
            }

            if (ChunkBasedCompressionSelected)
            {
                buildOptions |= BuildAssetBundleOptions.ChunkBasedCompression;
            }

            return buildOptions;
        }

        private bool PrepareBuildData(out AssetBundleBuild[] assetBundleBuildDatas, out ResourceData[] assetBundleResourceDatas, out ResourceData[] binaryResourceDatas)
        {
            assetBundleBuildDatas = null;
            assetBundleResourceDatas = null;
            binaryResourceDatas = null;
            resourceDatas.Clear();

            ResourceCollection.Resource[] resources = resourceCollection.GetResources();
            foreach (ResourceCollection.Resource resource in resources)
            {
                resourceDatas.Add(resource.FullName, new ResourceData(resource.Name, resource.Variant, resource.FileSystem, resource.LoadType, resource.Packed, resource.GetResourceGroups()));
            }

            Asset[] assets = resourceCollection.GetAssets();
            foreach (Asset asset in assets)
            {
                string assetName = asset.Name;
                if (string.IsNullOrEmpty(assetName))
                {
                    buildReport.LogError("Can not find asset by guid '{}'.", asset.guid);
                    return false;
                }

                string assetFileFullName = Application.dataPath.Substring(0, Application.dataPath.Length - AssetsStringLength) + assetName;
                if (!File.Exists(assetFileFullName))
                {
                    buildReport.LogError("Can not find asset '{}'.", assetFileFullName);
                    return false;
                }

                byte[] assetBytes = File.ReadAllBytes(assetFileFullName);
                int assetHashCode = Crc32Utils.GetCrc32(assetBytes);

                List<string> dependencyAssetNames = new List<string>();
                DependencyData dependencyData = resourceAnalyzerController.GetDependencyData(assetName);
                Asset[] dependencyAssets = dependencyData.GetDependencyAssets();
                foreach (Asset dependencyAsset in dependencyAssets)
                {
                    dependencyAssetNames.Add(dependencyAsset.Name);
                }

                dependencyAssetNames.Sort();

                resourceDatas[asset.resource.FullName].AddAssetData(asset.guid, assetName, assetBytes.Length, assetHashCode, dependencyAssetNames.ToArray());
            }

            List<AssetBundleBuild> assetBundleBuildDataList = new List<AssetBundleBuild>();
            List<ResourceData> assetBundleResourceDataList = new List<ResourceData>();
            List<ResourceData> binaryResourceDataList = new List<ResourceData>();
            foreach (ResourceData resourceData in resourceDatas.Values)
            {
                if (resourceData.AssetCount <= 0)
                {
                    buildReport.LogError("Resource '{}' has no asset.", GetResourceFullName(resourceData.name, resourceData.variant));
                    return false;
                }

                if (resourceData.IsLoadFromBinary)
                {
                    binaryResourceDataList.Add(resourceData);
                }
                else
                {
                    assetBundleResourceDataList.Add(resourceData);

                    AssetBundleBuild build = new AssetBundleBuild();
                    build.assetBundleName = resourceData.name;
                    build.assetBundleVariant = resourceData.variant;
                    build.assetNames = resourceData.GetAssetNames();
                    assetBundleBuildDataList.Add(build);
                }
            }

            assetBundleBuildDatas = assetBundleBuildDataList.ToArray();
            assetBundleResourceDatas = assetBundleResourceDataList.ToArray();
            binaryResourceDatas = binaryResourceDataList.ToArray();
            return true;
        }

        private static string GetResourceFullName(string name, string variant)
        {
            return !string.IsNullOrEmpty(variant) ? StringUtils.Format("{}.{}", name, variant) : name;
        }

        private static BuildTarget GetBuildTarget(Platform platform)
        {
            switch (platform)
            {
                case Platform.Windows:
                    return BuildTarget.StandaloneWindows;

                case Platform.Windows64:
                    return BuildTarget.StandaloneWindows64;

                case Platform.MacOS:
#if UNITY_2017_3_OR_NEWER
                    return BuildTarget.StandaloneOSX;
#else
                    return BuildTarget.StandaloneOSXUniversal;
#endif
                case Platform.Linux:
                    return BuildTarget.StandaloneLinux64;

                case Platform.IOS:
                    return BuildTarget.iOS;

                case Platform.Android:
                    return BuildTarget.Android;

                case Platform.WindowsStore:
                    return BuildTarget.WSAPlayer;

                case Platform.WebGL:
                    return BuildTarget.WebGL;

                default:
                    throw new GameFrameworkException("Platform is invalid.");
            }
        }

        public static string GetExtension(ResourceData data)
        {
            if (data.IsLoadFromBinary)
            {
                string assetName = data.GetAssetNames()[0];
                int position = assetName.LastIndexOf('.');
                if (position >= 0)
                {
                    return assetName.Substring(position + 1);
                }
            }

            return DefaultExtension;
        }
    }
}