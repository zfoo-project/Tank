namespace Summer.Editor.ResourceAnalyzer.Model
{
    public enum AssetsOrder : byte
    {
        AssetNameAsc,
        AssetNameDesc,
        DependencyResourceCountAsc,
        DependencyResourceCountDesc,
        DependencyAssetCountAsc,
        DependencyAssetCountDesc,
        ScatteredDependencyAssetCountAsc,
        ScatteredDependencyAssetCountDesc,
    }
}
