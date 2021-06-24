﻿namespace MiniKing.Script.Config
{
    public class VersionInfo
    {
        // 最新的游戏版本号
        public string gameVersion;
        // 是否需要强制更新游戏应用
        public bool forceUpdateGame;
        // 资源更新下载地址
        public string updatePrefixUrl;
        
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
    }
}
