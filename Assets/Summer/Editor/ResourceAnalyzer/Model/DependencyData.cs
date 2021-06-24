using System.Collections.Generic;
using Summer.Editor.ResourceCollection;

namespace Summer.Editor.ResourceAnalyzer.Model
{
    public sealed class DependencyData
    {
        public List<ResourceCollection.Resource> dependencyResources = new List<ResourceCollection.Resource>();
        public List<Asset> dependencyAssets = new List<Asset>();
        public List<string> scatteredDependencyAssetNames = new List<string>();

        public void AddDependencyAsset(Asset asset)
        {
            if (!dependencyResources.Contains(asset.resource))
            {
                dependencyResources.Add(asset.resource);
            }

            dependencyAssets.Add(asset);
        }

        public void AddScatteredDependencyAsset(string dependencyAssetName)
        {
            scatteredDependencyAssetNames.Add(dependencyAssetName);
        }

        public ResourceCollection.Resource[] GetDependencyResources()
        {
            return dependencyResources.ToArray();
        }

        public Asset[] GetDependencyAssets()
        {
            return dependencyAssets.ToArray();
        }

        public string[] GetScatteredDependencyAssetNames()
        {
            return scatteredDependencyAssetNames.ToArray();
        }

        public void RefreshData()
        {
            dependencyResources.Sort(DependencyResourcesComparer);
            dependencyAssets.Sort(DependencyAssetsComparer);
            scatteredDependencyAssetNames.Sort();
        }

        private int DependencyResourcesComparer(ResourceCollection.Resource a, ResourceCollection.Resource b)
        {
            return a.FullName.CompareTo(b.FullName);
        }

        private int DependencyAssetsComparer(Asset a, Asset b)
        {
            return a.Name.CompareTo(b.Name);
        }
    }
}