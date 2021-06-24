namespace Summer.Editor.ResourceBuilder.Model
{
    public sealed class ResourceCode
    {
        public readonly Platform platform;
        public readonly int length;
        public readonly int hashCode;
        public readonly int zipLength;
        public readonly int zipHashCode;

        public ResourceCode(Platform platform, int length, int hashCode, int zipLength, int zipHashCode)
        {
            this.platform = platform;
            this.length = length;
            this.hashCode = hashCode;
            this.zipLength = zipLength;
            this.zipHashCode = zipHashCode;
        }
    }
}