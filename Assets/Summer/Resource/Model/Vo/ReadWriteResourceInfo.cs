using System.Runtime.InteropServices;
using Summer.Resource.Model.Constant;

namespace Summer.Resource.Model.Vo
{
    [StructLayout(LayoutKind.Auto)]
    public struct ReadWriteResourceInfo
    {
        private readonly string fileSystemName;
        private readonly LoadType loadType;
        private readonly int length;
        private readonly int hashCode;

        public ReadWriteResourceInfo(string fileSystemName, LoadType loadType, int length, int hashCode)
        {
            this.fileSystemName = fileSystemName;
            this.loadType = loadType;
            this.length = length;
            this.hashCode = hashCode;
        }

        public bool UseFileSystem
        {
            get { return !string.IsNullOrEmpty(fileSystemName); }
        }

        public string FileSystemName
        {
            get { return fileSystemName; }
        }

        public LoadType LoadType
        {
            get { return loadType; }
        }

        public int Length
        {
            get { return length; }
        }

        public int HashCode
        {
            get { return hashCode; }
        }
    }
}