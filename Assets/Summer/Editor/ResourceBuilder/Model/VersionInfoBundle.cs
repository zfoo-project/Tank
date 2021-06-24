namespace MiniKing.Script.Config
{
    public class VersionInfoBundle
    {
        // 更新url的后缀
        public string updateSuffixUrl;
        
        // 最新的资源内部版本号
        public int internalResourceVersion;

        // 资源版本列表长度
        public int versionListLength;

        // 资源版本列表哈希值
        public int versionListHashCode;

        // 资源版本列表压缩后长度
        public int versionListZipLength;

        // 资源版本列表压缩后哈希值
        public int versionListZipHashCode;

        public static VersionInfoBundle ValueOf(string updateSuffixUrl, int internalResourceVersion, int versionListLength, int versionListHashCode, int versionListZipLength, int versionListZipHashCode)
        {
            var info = new VersionInfoBundle();
            info.updateSuffixUrl = updateSuffixUrl;
            info.internalResourceVersion = internalResourceVersion;
            info.versionListLength = versionListLength;
            info.versionListHashCode = versionListHashCode;
            info.versionListZipLength = versionListZipLength;
            info.versionListZipHashCode = versionListZipHashCode;
            return info;
        }
    }
}