using System.IO;
using Spring.Util;
using Summer.Editor.ResourceBuilder.Model;
using UnityEditor;
using UnityEngine;

namespace Summer.Editor.ResourceBuilder
{
    /// <summary>
    /// 资源生成器。
    /// </summary>
    public sealed class ResourceBuilderWindow : EditorWindow
    {
        private ResourceBuilderController controller;
        private bool orderBuildResources;
        private int buildEventHandlerTypeNameIndex;

        [MenuItem("Summer/Resource Tools/Resource Builder", false, 41)]
        private static void Open()
        {
            ResourceBuilderWindow window = GetWindow<ResourceBuilderWindow>("Resource Builder", true);
            window.minSize = new Vector2(800f, 645f);
        }

        private void OnEnable()
        {
            controller = new ResourceBuilderController();
            controller.OnLoadingResource += OnLoadingResource;
            controller.OnLoadingAsset += OnLoadingAsset;
            controller.OnLoadCompleted += OnLoadCompleted;
            controller.OnAnalyzingAsset += OnAnalyzingAsset;
            controller.OnAnalyzeCompleted += OnAnalyzeCompleted;
            controller.ProcessingAssetBundle += OnProcessingAssetBundle;
            controller.ProcessingBinary += OnProcessingBinary;
            controller.ProcessResourceComplete += OnProcessResourceComplete;
            controller.BuildResourceError += OnBuildResourceError;

            orderBuildResources = false;

            if (controller.Load())
            {
                Debug.Log("Load configuration success.");
                buildEventHandlerTypeNameIndex = 0;
                string[] buildEventHandlerTypeNames = controller.GetBuildEventHandlerTypeNames();
                for (int i = 0; i < buildEventHandlerTypeNames.Length; i++)
                {
                    if (controller.BuildEventHandlerTypeName == buildEventHandlerTypeNames[i])
                    {
                        buildEventHandlerTypeNameIndex = i;
                        break;
                    }
                }

                controller.RefreshBuildEventHandler();
            }
            else
            {
                Debug.LogWarning("Load configuration failure.");
            }
        }

        private void Update()
        {
            if (orderBuildResources)
            {
                orderBuildResources = false;
                BuildResources();
            }
        }

        private void OnGUI()
        {
            EditorGUILayout.BeginVertical(GUILayout.Width(position.width), GUILayout.Height(position.height));
            {
                GUILayout.Space(5f);
                EditorGUILayout.LabelField("Environment Information", EditorStyles.boldLabel);
                EditorGUILayout.BeginVertical("box");
                {
                    EditorGUILayout.BeginHorizontal();
                    {
                        EditorGUILayout.LabelField("Product Name", GUILayout.Width(160f));
                        EditorGUILayout.LabelField(controller.ProductName);
                    }
                    EditorGUILayout.EndHorizontal();
                    EditorGUILayout.BeginHorizontal();
                    {
                        EditorGUILayout.LabelField("Company Name", GUILayout.Width(160f));
                        EditorGUILayout.LabelField(controller.CompanyName);
                    }
                    EditorGUILayout.EndHorizontal();
                    EditorGUILayout.BeginHorizontal();
                    {
                        EditorGUILayout.LabelField("Game Identifier", GUILayout.Width(160f));
                        EditorGUILayout.LabelField(controller.GameIdentifier);
                    }
                    EditorGUILayout.EndHorizontal();
                    EditorGUILayout.BeginHorizontal();
                    {
                        EditorGUILayout.LabelField("Unity Version", GUILayout.Width(160f));
                        EditorGUILayout.LabelField(controller.UnityVersion);
                    }
                    EditorGUILayout.EndHorizontal();
                    EditorGUILayout.BeginHorizontal();
                    {
                        EditorGUILayout.LabelField("Applicable Game Version", GUILayout.Width(160f));
                        EditorGUILayout.LabelField(controller.ApplicableGameVersion);
                    }
                    EditorGUILayout.EndHorizontal();
                }
                EditorGUILayout.EndVertical();
                GUILayout.Space(5f);
                EditorGUILayout.BeginHorizontal();
                {
                    EditorGUILayout.BeginVertical();
                    {
                        EditorGUILayout.LabelField("Platforms", EditorStyles.boldLabel);
                        EditorGUILayout.BeginHorizontal("box");
                        {
                            EditorGUILayout.BeginVertical();
                            {
                                DrawPlatform(Platform.Windows, "Windows");
                                DrawPlatform(Platform.Windows64, "Windows x64");
                                DrawPlatform(Platform.MacOS, "macOS");
                                DrawPlatform(Platform.Linux, "Linux");
                                DrawPlatform(Platform.IOS, "iOS");
                                DrawPlatform(Platform.Android, "Android");
                            }
                            EditorGUILayout.EndVertical();
                            EditorGUILayout.BeginVertical();
                            {
                                DrawPlatform(Platform.WindowsStore, "Windows Store");
                                DrawPlatform(Platform.WebGL, "WebGL");
                            }
                            EditorGUILayout.EndVertical();
                        }
                        EditorGUILayout.EndHorizontal();
                        EditorGUILayout.BeginVertical();
                        {
                            controller.ZipSelected = EditorGUILayout.ToggleLeft("Zip All Resources", controller.ZipSelected);
                        }
                        EditorGUILayout.EndVertical();
                    }
                    EditorGUILayout.EndVertical();
                    EditorGUILayout.BeginVertical();
                    {
                        EditorGUILayout.LabelField("AssetBundle Options", EditorStyles.boldLabel);
                        EditorGUILayout.BeginVertical("box");
                        {
                            bool uncompressedAssetBundleSelected = EditorGUILayout.ToggleLeft("Uncompressed AssetBundle", controller.UncompressedAssetBundleSelected);
                            if (controller.UncompressedAssetBundleSelected != uncompressedAssetBundleSelected)
                            {
                                controller.UncompressedAssetBundleSelected = uncompressedAssetBundleSelected;
                                if (controller.UncompressedAssetBundleSelected)
                                {
                                    controller.ChunkBasedCompressionSelected = false;
                                }
                            }

                            bool disableWriteTypeTreeSelected = EditorGUILayout.ToggleLeft("Disable Write TypeTree", controller.DisableWriteTypeTreeSelected);
                            if (controller.DisableWriteTypeTreeSelected != disableWriteTypeTreeSelected)
                            {
                                controller.DisableWriteTypeTreeSelected = disableWriteTypeTreeSelected;
                                if (controller.DisableWriteTypeTreeSelected)
                                {
                                    controller.IgnoreTypeTreeChangesSelected = false;
                                }
                            }

                            controller.DeterministicAssetBundleSelected = EditorGUILayout.ToggleLeft("Deterministic AssetBundle", controller.DeterministicAssetBundleSelected);
                            controller.ForceRebuildAssetBundleSelected = EditorGUILayout.ToggleLeft("Force Rebuild AssetBundle", controller.ForceRebuildAssetBundleSelected);

                            bool ignoreTypeTreeChangesSelected = EditorGUILayout.ToggleLeft("Ignore TypeTree Changes", controller.IgnoreTypeTreeChangesSelected);
                            if (controller.IgnoreTypeTreeChangesSelected != ignoreTypeTreeChangesSelected)
                            {
                                controller.IgnoreTypeTreeChangesSelected = ignoreTypeTreeChangesSelected;
                                if (controller.IgnoreTypeTreeChangesSelected)
                                {
                                    controller.DisableWriteTypeTreeSelected = false;
                                }
                            }

                            EditorGUI.BeginDisabledGroup(true);
                            {
                                controller.AppendHashToAssetBundleNameSelected = EditorGUILayout.ToggleLeft("Append Hash To AssetBundle Name", controller.AppendHashToAssetBundleNameSelected);
                            }
                            EditorGUI.EndDisabledGroup();

                            bool chunkBasedCompressionSelected = EditorGUILayout.ToggleLeft("Chunk Based Compression", controller.ChunkBasedCompressionSelected);
                            if (controller.ChunkBasedCompressionSelected != chunkBasedCompressionSelected)
                            {
                                controller.ChunkBasedCompressionSelected = chunkBasedCompressionSelected;
                                if (controller.ChunkBasedCompressionSelected)
                                {
                                    controller.UncompressedAssetBundleSelected = false;
                                }
                            }
                        }
                        EditorGUILayout.EndVertical();
                    }
                    EditorGUILayout.EndVertical();
                }
                EditorGUILayout.EndHorizontal();
                string compressMessage = string.Empty;
                MessageType compressMessageType = MessageType.None;
                GetCompressMessage(out compressMessage, out compressMessageType);
                EditorGUILayout.HelpBox(compressMessage, compressMessageType);
                GUILayout.Space(5f);
                EditorGUILayout.LabelField("Build", EditorStyles.boldLabel);
                EditorGUILayout.BeginVertical("box");
                {
                    EditorGUILayout.BeginHorizontal();
                    {
                        EditorGUILayout.LabelField("Build Event Handler", GUILayout.Width(160f));
                        string[] names = controller.GetBuildEventHandlerTypeNames();
                        int selectedIndex = EditorGUILayout.Popup(buildEventHandlerTypeNameIndex, names);
                        if (selectedIndex != buildEventHandlerTypeNameIndex)
                        {
                            buildEventHandlerTypeNameIndex = selectedIndex;
                            controller.BuildEventHandlerTypeName = selectedIndex <= 0 ? string.Empty : names[selectedIndex];
                            if (controller.RefreshBuildEventHandler())
                            {
                                Debug.Log("Set build event success.");
                            }
                            else
                            {
                                Debug.LogWarning("Set build event failure.");
                            }
                        }
                    }
                    EditorGUILayout.EndHorizontal();
                    EditorGUILayout.BeginHorizontal();
                    {
                        EditorGUILayout.LabelField("Resource Version", GUILayout.Width(160f));
                        controller.InternalResourceVersion = EditorGUILayout.IntField(controller.InternalResourceVersion);
                    }
                    EditorGUILayout.EndHorizontal();
                    EditorGUILayout.BeginHorizontal();
                    {
                        EditorGUILayout.LabelField("Resource Version", GUILayout.Width(160f));
                        GUILayout.Label(StringUtils.Format("{} ({})", controller.ApplicableGameVersion, controller.InternalResourceVersion.ToString()));
                    }
                    EditorGUILayout.EndHorizontal();
                    EditorGUILayout.BeginHorizontal();
                    {
                        EditorGUILayout.LabelField("Output Directory", GUILayout.Width(160f));
                        controller.OutputDirectory = EditorGUILayout.TextField(controller.OutputDirectory);
                        if (GUILayout.Button("Browse...", GUILayout.Width(80f)))
                        {
                            BrowseOutputDirectory();
                        }
                    }
                    EditorGUILayout.EndHorizontal();
                    EditorGUILayout.BeginHorizontal();
                    {
                        EditorGUILayout.LabelField("Working Path", GUILayout.Width(160f));
                        GUILayout.Label(controller.WorkingPath);
                    }
                    EditorGUILayout.EndHorizontal();
                    EditorGUILayout.BeginHorizontal();
                    {
                        EditorGUI.BeginDisabledGroup(!controller.OutputPackageSelected);
                        EditorGUILayout.LabelField("Output Package Path", GUILayout.Width(160f));
                        GUILayout.Label(controller.OutputPackagePath);
                        EditorGUI.EndDisabledGroup();
                        controller.OutputPackageSelected = EditorGUILayout.ToggleLeft("Generate", controller.OutputPackageSelected, GUILayout.Width(70f));
                    }
                    EditorGUILayout.EndHorizontal();
                    EditorGUILayout.BeginHorizontal();
                    {
                        EditorGUI.BeginDisabledGroup(!controller.OutputFullSelected);
                        EditorGUILayout.LabelField("Output Full Path", GUILayout.Width(160f));
                        GUILayout.Label(controller.OutputFullPath);
                        EditorGUI.EndDisabledGroup();
                        controller.OutputFullSelected = EditorGUILayout.ToggleLeft("Generate", controller.OutputFullSelected, GUILayout.Width(70f));
                    }
                    EditorGUILayout.EndHorizontal();
                    EditorGUILayout.BeginHorizontal();
                    {
                        EditorGUI.BeginDisabledGroup(!controller.OutputPackedSelected);
                        EditorGUILayout.LabelField("Output Packed Path", GUILayout.Width(160f));
                        GUILayout.Label(controller.OutputPackedPath);
                        EditorGUI.EndDisabledGroup();
                        controller.OutputPackedSelected = EditorGUILayout.ToggleLeft("Generate", controller.OutputPackedSelected, GUILayout.Width(70f));
                    }
                    EditorGUILayout.EndHorizontal();
                    EditorGUILayout.BeginHorizontal();
                    {
                        EditorGUILayout.LabelField("Build Report Path", GUILayout.Width(160f));
                        GUILayout.Label(controller.BuildReportPath);
                    }
                    EditorGUILayout.EndHorizontal();
                }
                EditorGUILayout.EndVertical();
                string buildMessage = string.Empty;
                MessageType buildMessageType = MessageType.None;
                GetBuildMessage(out buildMessage, out buildMessageType);
                EditorGUILayout.HelpBox(buildMessage, buildMessageType);
                GUILayout.Space(2f);
                EditorGUILayout.BeginHorizontal();
                {
                    EditorGUI.BeginDisabledGroup(controller.Platforms == Platform.Undefined || !controller.IsValidOutputDirectory);
                    {
                        if (GUILayout.Button("Start Build Resources"))
                        {
                            orderBuildResources = true;
                        }
                    }
                    EditorGUI.EndDisabledGroup();
                    if (GUILayout.Button("Save", GUILayout.Width(80f)))
                    {
                        SaveConfiguration();
                    }
                }
                EditorGUILayout.EndHorizontal();
            }
            EditorGUILayout.EndVertical();
        }

        private void BrowseOutputDirectory()
        {
            string directory = EditorUtility.OpenFolderPanel("Select Output Directory", controller.OutputDirectory, string.Empty);
            if (!string.IsNullOrEmpty(directory))
            {
                controller.OutputDirectory = directory;
            }
        }

        private void GetCompressMessage(out string message, out MessageType messageType)
        {
            if (controller.ZipSelected)
            {
                if (controller.UncompressedAssetBundleSelected)
                {
                    message = "Compresses AssetBundles with ZIP only. It uses more storage but it's faster when loading the AssetBundles.";
                    messageType = MessageType.Info;
                }
                else if (controller.ChunkBasedCompressionSelected)
                {
                    message = "Compresses AssetBundles with both chunk-based compression and ZIP. Recommended when you use 'AssetBundle.LoadFromFile'.";
                    messageType = MessageType.Info;
                }
                else
                {
                    message = "Compresses AssetBundles with both LZMA and ZIP. Not recommended.";
                    messageType = MessageType.Warning;
                }
            }
            else
            {
                if (controller.UncompressedAssetBundleSelected)
                {
                    message = "Doesn't compress AssetBundles at all. Not recommended.";
                    messageType = MessageType.Warning;
                }
                else if (controller.ChunkBasedCompressionSelected)
                {
                    message = "Compresses AssetBundles with chunk-based compression only. Recommended when you use 'AssetBundle.LoadFromFile'.";
                    messageType = MessageType.Info;
                }
                else
                {
                    message = "Compresses AssetBundles with LZMA only. Recommended when you use 'AssetBundle.LoadFromMemory'.";
                    messageType = MessageType.Info;
                }
            }
        }

        private void GetBuildMessage(out string message, out MessageType messageType)
        {
            if (controller.Platforms == Platform.Undefined)
            {
                message = "Platform undefined.";
                messageType = MessageType.Error;
                return;
            }

            if (!controller.IsValidOutputDirectory)
            {
                message = "Output directory is invalid.";
                messageType = MessageType.Error;
                return;
            }

            message = string.Empty;
            messageType = MessageType.Info;
            if (Directory.Exists(controller.OutputPackagePath))
            {
                message += StringUtils.Format("{} will be overwritten.", controller.OutputPackagePath);
                messageType = MessageType.Warning;
            }

            if (Directory.Exists(controller.OutputFullPath))
            {
                if (message.Length > 0)
                {
                    message += " ";
                }

                message += StringUtils.Format("{} will be overwritten.", controller.OutputFullPath);
                messageType = MessageType.Warning;
            }

            if (Directory.Exists(controller.OutputPackedPath))
            {
                if (message.Length > 0)
                {
                    message += " ";
                }

                message += StringUtils.Format("{} will be overwritten.", controller.OutputPackedPath);
                messageType = MessageType.Warning;
            }

            if (messageType == MessageType.Warning)
            {
                return;
            }

            message = "Ready to build.";
        }

        private void BuildResources()
        {
            if (controller.BuildResources())
            {
                Debug.Log("Build resources success.");
                SaveConfiguration();
            }
            else
            {
                Debug.LogWarning("Build resources failure.");
            }
        }

        private void SaveConfiguration()
        {
            if (controller.Save())
            {
                Debug.Log("Save configuration success.");
            }
            else
            {
                Debug.LogWarning("Save configuration failure.");
            }
        }

        private void DrawPlatform(Platform platform, string platformName)
        {
            controller.SelectPlatform(platform, EditorGUILayout.ToggleLeft(platformName, controller.IsPlatformSelected(platform)));
        }

        private void OnLoadingResource(int index, int count)
        {
            EditorUtility.DisplayProgressBar("Loading Resources", StringUtils.Format("Loading resources, {}/{} loaded.", index.ToString(), count.ToString()), (float)index / count);
        }

        private void OnLoadingAsset(int index, int count)
        {
            EditorUtility.DisplayProgressBar("Loading Assets", StringUtils.Format("Loading assets, {}/{} loaded.", index.ToString(), count.ToString()), (float)index / count);
        }

        private void OnLoadCompleted()
        {
            EditorUtility.ClearProgressBar();
        }

        private void OnAnalyzingAsset(int index, int count)
        {
            EditorUtility.DisplayProgressBar("Analyzing Assets", StringUtils.Format("Analyzing assets, {}/{} analyzed.", index.ToString(), count.ToString()), (float)index / count);
        }

        private void OnAnalyzeCompleted()
        {
            EditorUtility.ClearProgressBar();
        }

        private bool OnProcessingAssetBundle(string assetBundleName, float progress)
        {
            if (EditorUtility.DisplayCancelableProgressBar("Processing AssetBundle", StringUtils.Format("Processing '{}'...", assetBundleName), progress))
            {
                EditorUtility.ClearProgressBar();
                return true;
            }
            else
            {
                Repaint();
                return false;
            }
        }

        private bool OnProcessingBinary(string binaryName, float progress)
        {
            if (EditorUtility.DisplayCancelableProgressBar("Processing Binary", StringUtils.Format("Processing '{}'...", binaryName), progress))
            {
                EditorUtility.ClearProgressBar();
                return true;
            }
            else
            {
                Repaint();
                return false;
            }
        }

        private void OnProcessResourceComplete(Platform platform)
        {
            EditorUtility.ClearProgressBar();
            Debug.Log(StringUtils.Format("Build resources for '{}' complete.", platform.ToString()));
        }

        private void OnBuildResourceError(string errorMessage)
        {
            EditorUtility.ClearProgressBar();
            Debug.LogWarning(StringUtils.Format("Build resources error with error message '{}'.", errorMessage));
        }
    }
}
