namespace Summer.Editor.ResourceBuilder.Model
{
    public sealed class VersionListData
    {
        public string path;

        public int length;

        public int hashCode;

        public int zipLength;

        public int zipHashCode;

        public static VersionListData ValueOf(string path, int length, int hashCode, int zipLength, int zipHashCode)
        {
            var data = new VersionListData();
            data.path = path;
            data.length = length;
            data.hashCode = hashCode;
            data.zipLength = zipLength;
            data.zipHashCode = zipHashCode;
            return data;
        }
    }
}