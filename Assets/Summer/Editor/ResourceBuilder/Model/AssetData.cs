namespace Summer.Editor.ResourceBuilder.Model
{
    public sealed class AssetData
    {
        public readonly string guid;
        public readonly string name;
        public readonly int length;
        public readonly int hashCode;
        public readonly string[] dependencyAssetNames;

        public AssetData(string guid, string name, int length, int hashCode, string[] dependencyAssetNames)
        {
            this.guid = guid;
            this.name = name;
            this.length = length;
            this.hashCode = hashCode;
            this.dependencyAssetNames = dependencyAssetNames;
        }

    }
}