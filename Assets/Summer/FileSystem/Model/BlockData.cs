using System.Runtime.InteropServices;

namespace Summer.FileSystem.Model
{
    /// <summary>
    /// 块数据。
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct BlockData
    {
        public static readonly BlockData Empty = new BlockData(0, 0);

        private readonly int stringIndex;
        private readonly int clusterIndex;
        private readonly int length;

        public BlockData(int clusterIndex, int length)
            : this(-1, clusterIndex, length)
        {
        }

        public BlockData(int stringIndex, int clusterIndex, int length)
        {
            this.stringIndex = stringIndex;
            this.clusterIndex = clusterIndex;
            this.length = length;
        }

        public bool Using
        {
            get { return stringIndex >= 0; }
        }

        public int StringIndex
        {
            get { return stringIndex; }
        }

        public int ClusterIndex
        {
            get { return clusterIndex; }
        }

        public int Length
        {
            get { return length; }
        }

        public BlockData Free()
        {
            return new BlockData(clusterIndex, (int) FileSystem.GetUpBoundClusterOffset(length));
        }
    }
}