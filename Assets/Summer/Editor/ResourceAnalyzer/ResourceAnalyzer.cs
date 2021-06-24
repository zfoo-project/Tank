using System.Collections.Generic;
using Summer.Editor.ResourceCollection;
using Spring.Util;
using Summer.Editor.ResourceAnalyzer.Model;
using UnityEditor;
using UnityEngine;

namespace Summer.Editor.ResourceAnalyzer
{
    /// <summary>
    /// 资源分析器。
    /// </summary>
    public sealed class ResourceAnalyzer : EditorWindow
    {
        private ResourceAnalyzerController controller;
        private bool analyzed;
        private int toolbarIndex;

        private int assetCount;
        private string[] cachedAssetNames;
        private int selectedAssetIndex = -1;
        private string selectedAssetName;
        private DependencyData selectedDependencyData;
        private AssetsOrder assetsOrder = AssetsOrder.AssetNameAsc;
        private string assetsFilter;
        private Vector2 assetsScroll = Vector2.zero;
        private Vector2 dependencyResourcesScroll = Vector2.zero;
        private Vector2 dependencyAssetsScroll = Vector2.zero;
        private Vector2 scatteredDependencyAssetsScroll = Vector2.zero;

        private int scatteredAssetCount;
        private string[] cachedScatteredAssetNames;
        private int selectedScatteredAssetIndex = -1;
        private string selectedScatteredAssetName;
        private Asset[] selectedHostAssets;
        private ScatteredAssetsOrder scatteredAssetsOrder = ScatteredAssetsOrder.AssetNameAsc;
        private string scatteredAssetsFilter;
        private Vector2 scatteredAssetsScroll = Vector2.zero;
        private Vector2 hostAssetsScroll = Vector2.zero;

        private int circularDependencyCount;
        private string[][] cachedCircularDependencyDatas;
        private Vector2 circularDependencyScroll = Vector2.zero;

        [MenuItem("Summer/Resource Tools/Resource Analyzer", false, 43)]
        private static void Open()
        {
            ResourceAnalyzer window = GetWindow<ResourceAnalyzer>("Resource Analyzer", true);
            window.minSize = new Vector2(800f, 600f);
        }

        private void OnEnable()
        {
            controller = new ResourceAnalyzerController();
            controller.OnLoadingResource += OnLoadingResource;
            controller.OnLoadingAsset += OnLoadingAsset;
            controller.OnLoadCompleted += OnLoadCompleted;
            controller.OnAnalyzingAsset += OnAnalyzingAsset;
            controller.OnAnalyzeCompleted += OnAnalyzeCompleted;

            analyzed = false;
            toolbarIndex = 0;

            assetCount = 0;
            cachedAssetNames = null;
            selectedAssetIndex = -1;
            selectedAssetName = null;
            selectedDependencyData = new DependencyData();
            assetsOrder = AssetsOrder.ScatteredDependencyAssetCountDesc;
            assetsFilter = null;
            assetsScroll = Vector2.zero;
            dependencyResourcesScroll = Vector2.zero;
            dependencyAssetsScroll = Vector2.zero;
            scatteredDependencyAssetsScroll = Vector2.zero;

            scatteredAssetCount = 0;
            cachedScatteredAssetNames = null;
            selectedScatteredAssetIndex = -1;
            selectedScatteredAssetName = null;
            selectedHostAssets = new Asset[] { };
            scatteredAssetsOrder = ScatteredAssetsOrder.HostAssetCountDesc;
            scatteredAssetsFilter = null;
            scatteredAssetsScroll = Vector2.zero;
            hostAssetsScroll = Vector2.zero;

            circularDependencyCount = 0;
            cachedCircularDependencyDatas = null;
            circularDependencyScroll = Vector2.zero;
        }

        private void OnGUI()
        {
            EditorGUILayout.BeginVertical(GUILayout.Width(position.width), GUILayout.Height(position.height));
            {
                GUILayout.Space(5f);
                int toolbarIndex = GUILayout.Toolbar(this.toolbarIndex, new string[] { "Summary", "Asset Dependency Viewer", "Scattered Asset Viewer", "Circular Dependency Viewer" }, GUILayout.Height(30f));
                if (toolbarIndex != this.toolbarIndex)
                {
                    this.toolbarIndex = toolbarIndex;
                    GUI.FocusControl(null);
                }

                switch (this.toolbarIndex)
                {
                    case 0:
                        DrawSummary();
                        break;

                    case 1:
                        DrawAssetDependencyViewer();
                        break;

                    case 2:
                        DrawScatteredAssetViewer();
                        break;

                    case 3:
                        DrawCircularDependencyViewer();
                        break;
                }
            }
            EditorGUILayout.EndVertical();
        }

        private void DrawAnalyzeButton()
        {
            if (!analyzed)
            {
                EditorGUILayout.HelpBox("Please analyze first.", MessageType.Info);
            }

            if (GUILayout.Button("Analyze", GUILayout.Height(30f)))
            {
                controller.Clear();

                selectedAssetIndex = -1;
                selectedAssetName = null;
                selectedDependencyData = new DependencyData();

                selectedScatteredAssetIndex = -1;
                selectedScatteredAssetName = null;
                selectedHostAssets = new Asset[] { };

                if (controller.Prepare())
                {
                    controller.Analyze();
                    analyzed = true;
                    assetCount = controller.GetAssetNames().Length;
                    scatteredAssetCount = controller.GetScatteredAssetNames().Length;
                    cachedCircularDependencyDatas = controller.GetCircularDependencyDatas();
                    circularDependencyCount = cachedCircularDependencyDatas.Length;
                    OnAssetsOrderOrFilterChanged();
                    OnScatteredAssetsOrderOrFilterChanged();
                }
                else
                {
                    EditorUtility.DisplayDialog("Resource Analyze", "Can not parse 'ResourceCollection.xml', please use 'Resource Editor' tool first.", "OK");
                }
            }
        }

        private void DrawSummary()
        {
            DrawAnalyzeButton();
        }

        private void DrawAssetDependencyViewer()
        {
            if (!analyzed)
            {
                DrawAnalyzeButton();
                return;
            }

            EditorGUILayout.BeginHorizontal();
            {
                GUILayout.Space(5f);
                EditorGUILayout.BeginVertical(GUILayout.Width(position.width * 0.4f));
                {
                    GUILayout.Space(5f);
                    string title = null;
                    if (string.IsNullOrEmpty(assetsFilter))
                    {
                        title = StringUtils.Format("Assets In Resources ({})", assetCount.ToString());
                    }
                    else
                    {
                        title = StringUtils.Format("Assets In Resources ({}/{})", cachedAssetNames.Length.ToString(), assetCount.ToString());
                    }
                    EditorGUILayout.LabelField(title, EditorStyles.boldLabel);
                    EditorGUILayout.BeginVertical("box", GUILayout.Height(position.height - 150f));
                    {
                        assetsScroll = EditorGUILayout.BeginScrollView(assetsScroll);
                        {
                            int selectedIndex = GUILayout.SelectionGrid(selectedAssetIndex, cachedAssetNames, 1, "toggle");
                            if (selectedIndex != selectedAssetIndex)
                            {
                                selectedAssetIndex = selectedIndex;
                                selectedAssetName = cachedAssetNames[selectedIndex];
                                selectedDependencyData = controller.GetDependencyData(selectedAssetName);
                            }
                        }
                        EditorGUILayout.EndScrollView();
                    }
                    EditorGUILayout.EndVertical();
                    EditorGUILayout.BeginVertical("box");
                    {
                        EditorGUILayout.LabelField("Asset Name", selectedAssetName ?? "<None>");
                        EditorGUILayout.LabelField("Resource Name", selectedAssetName == null ? "<None>" : controller.GetAsset(selectedAssetName).resource.FullName);
                        EditorGUILayout.BeginHorizontal();
                        {
                            AssetsOrder assetsOrder = (AssetsOrder)EditorGUILayout.EnumPopup("Order by", this.assetsOrder);
                            if (assetsOrder != this.assetsOrder)
                            {
                                this.assetsOrder = assetsOrder;
                                OnAssetsOrderOrFilterChanged();
                            }
                        }
                        EditorGUILayout.EndHorizontal();
                        EditorGUILayout.BeginHorizontal();
                        {
                            string assetsFilter = EditorGUILayout.TextField("Assets Filter", this.assetsFilter);
                            if (assetsFilter != this.assetsFilter)
                            {
                                this.assetsFilter = assetsFilter;
                                OnAssetsOrderOrFilterChanged();
                            }
                            EditorGUI.BeginDisabledGroup(string.IsNullOrEmpty(this.assetsFilter));
                            {
                                if (GUILayout.Button("x", GUILayout.Width(20f)))
                                {
                                    this.assetsFilter = null;
                                    GUI.FocusControl(null);
                                    OnAssetsOrderOrFilterChanged();
                                }
                            }
                            EditorGUI.EndDisabledGroup();
                        }
                        EditorGUILayout.EndHorizontal();
                    }
                    EditorGUILayout.EndVertical();
                }
                EditorGUILayout.EndVertical();
                EditorGUILayout.BeginVertical(GUILayout.Width(position.width * 0.6f - 14f));
                {
                    GUILayout.Space(5f);
                    EditorGUILayout.LabelField(StringUtils.Format("Dependency Resources ({})", selectedDependencyData.dependencyResources.Count.ToString()), EditorStyles.boldLabel);
                    EditorGUILayout.BeginVertical("box", GUILayout.Height(position.height * 0.2f));
                    {
                        dependencyResourcesScroll = EditorGUILayout.BeginScrollView(dependencyResourcesScroll);
                        {
                            ResourceCollection.Resource[] dependencyResources = selectedDependencyData.GetDependencyResources();
                            foreach (ResourceCollection.Resource dependencyResource in dependencyResources)
                            {
                                GUILayout.Label(dependencyResource.FullName);
                            }
                        }
                        EditorGUILayout.EndScrollView();
                    }
                    EditorGUILayout.EndVertical();
                    EditorGUILayout.LabelField(StringUtils.Format("Dependency Assets ({})", selectedDependencyData.dependencyResources.Count.ToString()), EditorStyles.boldLabel);
                    EditorGUILayout.BeginVertical("box", GUILayout.Height(position.height * 0.3f));
                    {
                        dependencyAssetsScroll = EditorGUILayout.BeginScrollView(dependencyAssetsScroll);
                        {
                            Asset[] dependencyAssets = selectedDependencyData.GetDependencyAssets();
                            foreach (Asset dependencyAsset in dependencyAssets)
                            {
                                EditorGUILayout.BeginHorizontal();
                                {
                                    if (GUILayout.Button("GO", GUILayout.Width(30f)))
                                    {
                                        selectedAssetName = dependencyAsset.Name;
                                        selectedAssetIndex = new List<string>(cachedAssetNames).IndexOf(selectedAssetName);
                                        selectedDependencyData = controller.GetDependencyData(selectedAssetName);
                                    }

                                    GUILayout.Label(dependencyAsset.Name);
                                }
                                EditorGUILayout.EndHorizontal();
                            }
                        }
                        EditorGUILayout.EndScrollView();
                    }
                    EditorGUILayout.EndVertical();
                    EditorGUILayout.LabelField(StringUtils.Format("Scattered Dependency Assets ({})", selectedDependencyData.scatteredDependencyAssetNames.Count.ToString()), EditorStyles.boldLabel);
                    EditorGUILayout.BeginVertical("box", GUILayout.Height(position.height * 0.5f - 116f));
                    {
                        scatteredDependencyAssetsScroll = EditorGUILayout.BeginScrollView(scatteredDependencyAssetsScroll);
                        {
                            string[] scatteredDependencyAssetNames = selectedDependencyData.GetScatteredDependencyAssetNames();
                            foreach (string scatteredDependencyAssetName in scatteredDependencyAssetNames)
                            {
                                EditorGUILayout.BeginHorizontal();
                                {
                                    int count = controller.GetHostAssets(scatteredDependencyAssetName).Length;
                                    EditorGUI.BeginDisabledGroup(count < 2);
                                    {
                                        if (GUILayout.Button("GO", GUILayout.Width(30f)))
                                        {
                                            selectedScatteredAssetName = scatteredDependencyAssetName;
                                            selectedScatteredAssetIndex = new List<string>(cachedScatteredAssetNames).IndexOf(selectedScatteredAssetName);
                                            selectedHostAssets = controller.GetHostAssets(selectedScatteredAssetName);
                                            toolbarIndex = 2;
                                            GUI.FocusControl(null);
                                        }
                                    }
                                    EditorGUI.EndDisabledGroup();
                                    GUILayout.Label(count > 1 ? StringUtils.Format("{} ({})", scatteredDependencyAssetName, count.ToString()) : scatteredDependencyAssetName);
                                }
                                EditorGUILayout.EndHorizontal();
                            }
                        }
                        EditorGUILayout.EndScrollView();
                    }
                    EditorGUILayout.EndVertical();
                }
                EditorGUILayout.EndVertical();
            }
            EditorGUILayout.EndHorizontal();
        }

        private void DrawScatteredAssetViewer()
        {
            if (!analyzed)
            {
                DrawAnalyzeButton();
                return;
            }

            EditorGUILayout.BeginHorizontal();
            {
                GUILayout.Space(5f);
                EditorGUILayout.BeginVertical(GUILayout.Width(position.width * 0.4f));
                {
                    GUILayout.Space(5f);
                    string title = null;
                    if (string.IsNullOrEmpty(scatteredAssetsFilter))
                    {
                        title = StringUtils.Format("Scattered Assets ({})", scatteredAssetCount.ToString());
                    }
                    else
                    {
                        title = StringUtils.Format("Scattered Assets ({}/{})", cachedScatteredAssetNames.Length.ToString(), scatteredAssetCount.ToString());
                    }
                    EditorGUILayout.LabelField(title, EditorStyles.boldLabel);
                    EditorGUILayout.BeginVertical("box", GUILayout.Height(position.height - 132f));
                    {
                        scatteredAssetsScroll = EditorGUILayout.BeginScrollView(scatteredAssetsScroll);
                        {
                            int selectedIndex = GUILayout.SelectionGrid(selectedScatteredAssetIndex, cachedScatteredAssetNames, 1, "toggle");
                            if (selectedIndex != selectedScatteredAssetIndex)
                            {
                                selectedScatteredAssetIndex = selectedIndex;
                                selectedScatteredAssetName = cachedScatteredAssetNames[selectedIndex];
                                selectedHostAssets = controller.GetHostAssets(selectedScatteredAssetName);
                            }
                        }
                        EditorGUILayout.EndScrollView();
                    }
                    EditorGUILayout.EndVertical();
                    EditorGUILayout.BeginVertical("box");
                    {
                        EditorGUILayout.LabelField("Scattered Asset Name", selectedScatteredAssetName ?? "<None>");
                        EditorGUILayout.BeginHorizontal();
                        {
                            ScatteredAssetsOrder scatteredAssetsOrder = (ScatteredAssetsOrder)EditorGUILayout.EnumPopup("Order by", this.scatteredAssetsOrder);
                            if (scatteredAssetsOrder != this.scatteredAssetsOrder)
                            {
                                this.scatteredAssetsOrder = scatteredAssetsOrder;
                                OnScatteredAssetsOrderOrFilterChanged();
                            }
                        }
                        EditorGUILayout.EndHorizontal();
                        EditorGUILayout.BeginHorizontal();
                        {
                            string scatteredAssetsFilter = EditorGUILayout.TextField("Assets Filter", this.scatteredAssetsFilter);
                            if (scatteredAssetsFilter != this.scatteredAssetsFilter)
                            {
                                this.scatteredAssetsFilter = scatteredAssetsFilter;
                                OnScatteredAssetsOrderOrFilterChanged();
                            }
                            EditorGUI.BeginDisabledGroup(string.IsNullOrEmpty(this.scatteredAssetsFilter));
                            {
                                if (GUILayout.Button("x", GUILayout.Width(20f)))
                                {
                                    this.scatteredAssetsFilter = null;
                                    GUI.FocusControl(null);
                                    OnScatteredAssetsOrderOrFilterChanged();
                                }
                            }
                            EditorGUI.EndDisabledGroup();
                        }
                        EditorGUILayout.EndHorizontal();
                    }
                    EditorGUILayout.EndVertical();
                }
                EditorGUILayout.EndVertical();
                EditorGUILayout.BeginVertical(GUILayout.Width(position.width * 0.6f - 14f));
                {
                    GUILayout.Space(5f);
                    EditorGUILayout.LabelField(StringUtils.Format("Host Assets ({})", selectedHostAssets.Length.ToString()), EditorStyles.boldLabel);
                    EditorGUILayout.BeginVertical("box", GUILayout.Height(position.height - 68f));
                    {
                        hostAssetsScroll = EditorGUILayout.BeginScrollView(hostAssetsScroll);
                        {
                            foreach (Asset hostAsset in selectedHostAssets)
                            {
                                EditorGUILayout.BeginHorizontal();
                                {
                                    if (GUILayout.Button("GO", GUILayout.Width(30f)))
                                    {
                                        selectedAssetName = hostAsset.Name;
                                        selectedAssetIndex = new List<string>(cachedAssetNames).IndexOf(selectedAssetName);
                                        selectedDependencyData = controller.GetDependencyData(selectedAssetName);
                                        toolbarIndex = 1;
                                        GUI.FocusControl(null);
                                    }

                                    GUILayout.Label(StringUtils.Format("{} [{}]", hostAsset.Name, hostAsset.resource.FullName));
                                }
                                EditorGUILayout.EndHorizontal();
                            }
                        }
                        EditorGUILayout.EndScrollView();
                    }
                    EditorGUILayout.EndVertical();
                }
                EditorGUILayout.EndVertical();
            }
            EditorGUILayout.EndHorizontal();
        }

        private void DrawCircularDependencyViewer()
        {
            if (!analyzed)
            {
                DrawAnalyzeButton();
                return;
            }

            EditorGUILayout.BeginHorizontal();
            {
                GUILayout.Space(5f);
                EditorGUILayout.BeginVertical();
                {
                    GUILayout.Space(5f);
                    EditorGUILayout.LabelField(StringUtils.Format("Circular Dependency ({})", circularDependencyCount.ToString()), EditorStyles.boldLabel);
                    circularDependencyScroll = EditorGUILayout.BeginScrollView(circularDependencyScroll);
                    {
                        int count = 0;
                        foreach (string[] circularDependencyData in cachedCircularDependencyDatas)
                        {
                            GUILayout.Label(StringUtils.Format("{}) {}", (++count).ToString(), circularDependencyData[circularDependencyData.Length - 1]), EditorStyles.boldLabel);
                            EditorGUILayout.BeginVertical("box");
                            {
                                foreach (string circularDependency in circularDependencyData)
                                {
                                    EditorGUILayout.BeginHorizontal();
                                    {
                                        GUILayout.Label(circularDependency);
                                        if (GUILayout.Button("GO", GUILayout.Width(30f)))
                                        {
                                            selectedAssetName = circularDependency;
                                            selectedAssetIndex = new List<string>(cachedAssetNames).IndexOf(selectedAssetName);
                                            selectedDependencyData = controller.GetDependencyData(selectedAssetName);
                                            toolbarIndex = 1;
                                            GUI.FocusControl(null);
                                        }
                                    }
                                    EditorGUILayout.EndHorizontal();
                                }
                            }
                            EditorGUILayout.EndVertical();
                            GUILayout.Space(5f);
                        }
                    }
                    EditorGUILayout.EndScrollView();
                }
                EditorGUILayout.EndVertical();
            }
            EditorGUILayout.EndHorizontal();
        }

        private void OnAssetsOrderOrFilterChanged()
        {
            cachedAssetNames = controller.GetAssetNames(assetsOrder, assetsFilter);
            if (!string.IsNullOrEmpty(selectedAssetName))
            {
                selectedAssetIndex = new List<string>(cachedAssetNames).IndexOf(selectedAssetName);
            }
        }

        private void OnScatteredAssetsOrderOrFilterChanged()
        {
            cachedScatteredAssetNames = controller.GetScatteredAssetNames(scatteredAssetsOrder, scatteredAssetsFilter);
            if (!string.IsNullOrEmpty(selectedScatteredAssetName))
            {
                selectedScatteredAssetIndex = new List<string>(cachedScatteredAssetNames).IndexOf(selectedScatteredAssetName);
            }
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
    }
}
