using System;
using System.Collections.Generic;
using System.Linq;
using Spring.Util;
using Summer.Base.Model;
using Summer.Editor.ResourceAnalyzer.Model;
using Summer.Editor.ResourceCollection;
using UnityEditor;
using UnityEngine;

namespace Summer.Editor.ResourceAnalyzer
{
    public sealed class ResourceAnalyzerController
    {
        private readonly ResourceCollection.ResourceCollection resourceCollection;

        private readonly Dictionary<string, DependencyData> dependencyDatas;
        private readonly Dictionary<string, List<Asset>> scatteredAssets;
        private readonly List<string[]> circularDependencyDatas;
        private readonly HashSet<Stamp> analyzedStamps;

        public ResourceAnalyzerController() : this(null)
        {
        }

        public ResourceAnalyzerController(ResourceCollection.ResourceCollection resourceCollection)
        {
            this.resourceCollection = resourceCollection != null ? resourceCollection : new ResourceCollection.ResourceCollection();

            this.resourceCollection.OnLoadingResource += delegate(int index, int count)
            {
                if (OnLoadingResource != null)
                {
                    OnLoadingResource(index, count);
                }
            };

            this.resourceCollection.OnLoadingAsset += delegate(int index, int count)
            {
                if (OnLoadingAsset != null)
                {
                    OnLoadingAsset(index, count);
                }
            };

            this.resourceCollection.OnLoadCompleted += delegate()
            {
                if (OnLoadCompleted != null)
                {
                    OnLoadCompleted();
                }
            };

            dependencyDatas = new Dictionary<string, DependencyData>(StringComparer.Ordinal);
            scatteredAssets = new Dictionary<string, List<Asset>>(StringComparer.Ordinal);
            analyzedStamps = new HashSet<Stamp>();
            circularDependencyDatas = new List<string[]>();
        }

        public event GameFrameworkAction<int, int> OnLoadingResource = null;

        public event GameFrameworkAction<int, int> OnLoadingAsset = null;

        public event GameFrameworkAction OnLoadCompleted = null;

        public event GameFrameworkAction<int, int> OnAnalyzingAsset = null;

        public event GameFrameworkAction OnAnalyzeCompleted = null;

        public void Clear()
        {
            resourceCollection.Clear();
            dependencyDatas.Clear();
            scatteredAssets.Clear();
            circularDependencyDatas.Clear();
            analyzedStamps.Clear();
        }

        public bool Prepare()
        {
            resourceCollection.Clear();
            return resourceCollection.Load();
        }

        public void Analyze()
        {
            dependencyDatas.Clear();
            scatteredAssets.Clear();
            circularDependencyDatas.Clear();
            analyzedStamps.Clear();

            HashSet<string> scriptAssetNames = GetFilteredAssetNames("t:Script");
            Asset[] assets = resourceCollection.GetAssets();
            int count = assets.Length;
            for (int i = 0; i < count; i++)
            {
                if (OnAnalyzingAsset != null)
                {
                    OnAnalyzingAsset(i, count);
                }

                string assetName = assets[i].Name;
                if (string.IsNullOrEmpty(assetName))
                {
                    Debug.LogWarning(StringUtils.Format("Can not find asset by guid '{}'.", assets[i].guid));
                    continue;
                }

                DependencyData dependencyData = new DependencyData();
                AnalyzeAsset(assetName, assets[i], dependencyData, scriptAssetNames);
                dependencyData.RefreshData();
                dependencyDatas.Add(assetName, dependencyData);
            }

            foreach (List<Asset> scatteredAsset in scatteredAssets.Values)
            {
                scatteredAsset.Sort((a, b) => a.Name.CompareTo(b.Name));
            }

            circularDependencyDatas.AddRange(new CircularDependencyChecker(analyzedStamps.ToArray()).Check());

            if (OnAnalyzeCompleted != null)
            {
                OnAnalyzeCompleted();
            }
        }

        private void AnalyzeAsset(string assetName, Asset hostAsset, DependencyData dependencyData, HashSet<string> scriptAssetNames)
        {
            string[] dependencyAssetNames = AssetDatabase.GetDependencies(assetName, false);
            foreach (string dependencyAssetName in dependencyAssetNames)
            {
                if (scriptAssetNames.Contains(dependencyAssetName))
                {
                    continue;
                }

                if (dependencyAssetName == assetName)
                {
                    continue;
                }

                if (dependencyAssetName.EndsWith(".unity", StringComparison.Ordinal))
                {
                    // 忽略对场景的依赖
                    continue;
                }

                Stamp stamp = new Stamp(hostAsset.Name, dependencyAssetName);
                if (analyzedStamps.Contains(stamp))
                {
                    continue;
                }

                analyzedStamps.Add(stamp);

                string guid = AssetDatabase.AssetPathToGUID(dependencyAssetName);
                if (string.IsNullOrEmpty(guid))
                {
                    Debug.LogWarning(StringUtils.Format("Can not find guid by asset '{}'.", dependencyAssetName));
                    continue;
                }

                Asset asset = resourceCollection.GetAsset(guid);
                if (asset != null)
                {
                    dependencyData.AddDependencyAsset(asset);
                }
                else
                {
                    dependencyData.AddScatteredDependencyAsset(dependencyAssetName);

                    List<Asset> scatteredAssets = null;
                    if (!this.scatteredAssets.TryGetValue(dependencyAssetName, out scatteredAssets))
                    {
                        scatteredAssets = new List<Asset>();
                        this.scatteredAssets.Add(dependencyAssetName, scatteredAssets);
                    }

                    scatteredAssets.Add(hostAsset);

                    AnalyzeAsset(dependencyAssetName, hostAsset, dependencyData, scriptAssetNames);
                }
            }
        }

        public Asset GetAsset(string assetName)
        {
            return resourceCollection.GetAsset(AssetDatabase.AssetPathToGUID(assetName));
        }

        public string[] GetAssetNames()
        {
            return GetAssetNames(AssetsOrder.AssetNameAsc, null);
        }

        public string[] GetAssetNames(AssetsOrder order, string filter)
        {
            HashSet<string> filteredAssetNames = GetFilteredAssetNames(filter);
            IEnumerable<KeyValuePair<string, DependencyData>> filteredResult = dependencyDatas.Where(pair => filteredAssetNames.Contains(pair.Key));
            IEnumerable<KeyValuePair<string, DependencyData>> orderedResult = null;
            switch (order)
            {
                case AssetsOrder.AssetNameAsc:
                    orderedResult = filteredResult.OrderBy(pair => pair.Key);
                    break;

                case AssetsOrder.AssetNameDesc:
                    orderedResult = filteredResult.OrderByDescending(pair => pair.Key);
                    break;

                case AssetsOrder.DependencyResourceCountAsc:
                    orderedResult = filteredResult.OrderBy(pair => pair.Value.dependencyResources.Count);
                    break;

                case AssetsOrder.DependencyResourceCountDesc:
                    orderedResult = filteredResult.OrderByDescending(pair => pair.Value.dependencyResources.Count);
                    break;

                case AssetsOrder.DependencyAssetCountAsc:
                    orderedResult = filteredResult.OrderBy(pair => pair.Value.dependencyAssets.Count);
                    break;

                case AssetsOrder.DependencyAssetCountDesc:
                    orderedResult = filteredResult.OrderByDescending(pair => pair.Value.dependencyAssets.Count);
                    break;

                case AssetsOrder.ScatteredDependencyAssetCountAsc:
                    orderedResult = filteredResult.OrderBy(pair => pair.Value.scatteredDependencyAssetNames.Count);
                    break;

                case AssetsOrder.ScatteredDependencyAssetCountDesc:
                    orderedResult = filteredResult.OrderByDescending(pair => pair.Value.scatteredDependencyAssetNames.Count);
                    break;

                default:
                    orderedResult = filteredResult;
                    break;
            }

            return orderedResult.Select(pair => pair.Key).ToArray();
        }

        public DependencyData GetDependencyData(string assetName)
        {
            DependencyData dependencyData = null;
            if (dependencyDatas.TryGetValue(assetName, out dependencyData))
            {
                return dependencyData;
            }

            return dependencyData;
        }

        public string[] GetScatteredAssetNames()
        {
            return GetScatteredAssetNames(ScatteredAssetsOrder.HostAssetCountDesc, null);
        }

        public string[] GetScatteredAssetNames(ScatteredAssetsOrder order, string filter)
        {
            HashSet<string> filterAssetNames = GetFilteredAssetNames(filter);
            IEnumerable<KeyValuePair<string, List<Asset>>> filteredResult = scatteredAssets.Where(pair => filterAssetNames.Contains(pair.Key) && pair.Value.Count > 1);
            IEnumerable<KeyValuePair<string, List<Asset>>> orderedResult = null;
            switch (order)
            {
                case ScatteredAssetsOrder.AssetNameAsc:
                    orderedResult = filteredResult.OrderBy(pair => pair.Key);
                    break;

                case ScatteredAssetsOrder.AssetNameDesc:
                    orderedResult = filteredResult.OrderByDescending(pair => pair.Key);
                    break;

                case ScatteredAssetsOrder.HostAssetCountAsc:
                    orderedResult = filteredResult.OrderBy(pair => pair.Value.Count);
                    break;

                case ScatteredAssetsOrder.HostAssetCountDesc:
                    orderedResult = filteredResult.OrderByDescending(pair => pair.Value.Count);
                    break;

                default:
                    orderedResult = filteredResult;
                    break;
            }

            return orderedResult.Select(pair => pair.Key).ToArray();
        }

        public Asset[] GetHostAssets(string scatteredAssetName)
        {
            List<Asset> assets = null;
            if (scatteredAssets.TryGetValue(scatteredAssetName, out assets))
            {
                return assets.ToArray();
            }

            return null;
        }

        public string[][] GetCircularDependencyDatas()
        {
            return circularDependencyDatas.ToArray();
        }

        private HashSet<string> GetFilteredAssetNames(string filter)
        {
            string[] filterAssetGuids = AssetDatabase.FindAssets(filter);
            HashSet<string> filterAssetNames = new HashSet<string>();
            foreach (string filterAssetGuid in filterAssetGuids)
            {
                filterAssetNames.Add(AssetDatabase.GUIDToAssetPath(filterAssetGuid));
            }

            return filterAssetNames;
        }
    }
}