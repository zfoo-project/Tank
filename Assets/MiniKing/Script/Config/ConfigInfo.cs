using System.Collections.Generic;

namespace MiniKing.Script.Config
{
    public class ConfigInfo
    {
        // 仅仅用来标识服务器的大版本号，用来确定是否强制更新
        public string gameVersion;

        public string versionInfoUrl;

        public string windowsAppUrl;

        public string macOSAppUrl;

        public string iosAppUrl;

        public string androidAppUrl;

        public List<string> gatewayUrls;

        public List<string> websocketGatewayUrls;
    }
}