using System.Runtime.InteropServices;

namespace Summer.Editor.ResourceAnalyzer.Model
{
    [StructLayout(LayoutKind.Auto)]
    public struct Stamp
    {
        private readonly string hostAssetName;
        private readonly string dependencyAssetName;

        public Stamp(string hostAssetName, string dependencyAssetName)
        {
            this.hostAssetName = hostAssetName;
            this.dependencyAssetName = dependencyAssetName;
        }

        public string HostAssetName
        {
            get { return hostAssetName; }
        }

        public string DependencyAssetName
        {
            get { return dependencyAssetName; }
        }
    }
}