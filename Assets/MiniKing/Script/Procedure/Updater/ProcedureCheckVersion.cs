using System;
using DG.Tweening;
using MiniKing.Script.Config;
using MiniKing.Script.Constant;
using MiniKing.Script.UI.Common;
using MiniKing.Script.UI.Login;
using Spring.Core;
using Spring.Event;
using Spring.Logger;
using Spring.Util;
using Spring.Util.Json;
using Summer.I18n;
using Summer.Procedure;
using Summer.Resource;
using Summer.Resource.Model.Constant;
using Summer.WebRequest;
using Summer.WebRequest.Model.Eve;
using UnityEngine;

namespace MiniKing.Script.Procedure.Updater
{
    [Bean]
    public class ProcedureCheckVersion : FsmState<IProcedureFsmManager>
    {
        [Autowired]
        private II18nManager i18nManager;

        private int checkErrorTimes;
        private bool needUpdateVersion;
        private VersionInfo versionInfo;

        private bool checkVersionComplete;
        public bool confirmUpdateVersion;

        public override void OnEnter(IFsm<IProcedureFsmManager> fsm)
        {
            base.OnEnter(fsm);

            CommonController.GetInstance().loadingRotate.Show();
            
            CheckVersion();
        }

        public override void OnUpdate(IFsm<IProcedureFsmManager> fsm, float elapseSeconds, float realElapseSeconds)
        {
            base.OnUpdate(fsm, elapseSeconds, realElapseSeconds);

            if (!checkVersionComplete)
            {
                return;
            }

            if (needUpdateVersion)
            {
                if (confirmUpdateVersion)
                {
                    fsm.SetData(GameConstant.VERSION_INFO, versionInfo);
                    fsm.ChangeState<ProcedureUpdateVersion>();
                }
            }
            else
            {
                fsm.ChangeState<ProcedureCheckResources>();
            }
        }

        public void CheckVersion()
        {
            checkVersionComplete = false;
            confirmUpdateVersion = false;

            needUpdateVersion = false;
            versionInfo = null;

            // 向服务器请求版本信息
            var checkVersionUrl = StringUtils.Format(SpringContext.GetBean<ConfigComponent>().configInfo.versionInfoUrl, GetPlatformPath());
            SpringContext.GetBean<IWebRequestManager>().AddGetWebRequest(checkVersionUrl, this);
        }

        public void GotoUpdateApp()
        {
            string url = null;
#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
            url = SpringContext.GetBean<ConfigComponent>().configInfo.windowsAppUrl;
#elif UNITY_EDITOR_OSX || UNITY_STANDALONE_OSX
            url = SpringContext.GetBean<ConfigComponent>().configInfo.macOSAppUrl;
#elif UNITY_IOS
            url = SpringContext.GetBean<ConfigComponent>().configInfo.iOSAppUrl;
#elif UNITY_ANDROID
            url = SpringContext.GetBean<ConfigComponent>().configInfo.androidAppUrl;
#endif
            if (!string.IsNullOrEmpty(url))
            {
                Application.OpenURL(url);
            }
        }

        [EventReceiver]
        private void OnWebRequestSuccessEvent(WebRequestSuccessEvent eve)
        {
            if (eve.userData != this)
            {
                return;
            }

            // 解析版本信息
            byte[] versionInfoBytes = eve.GetWebResponseBytes();
            var versionInfoString = ConverterUtils.GetString(versionInfoBytes).Trim();
            Log.Info("VersionInfo.json文件下载成功");
            Log.Info(versionInfoString);
            versionInfo = JsonUtils.string2Object<VersionInfo>(versionInfoString);
            if (versionInfo == null)
            {
                Log.Error("Parse VersionInfo failure.");
                return;
            }

            Log.Info("Latest game version is '{}', local game version is '{}'.", versionInfo.gameVersion, Application.version);
            CommonController.GetInstance().loadingRotate.Hide();
            
            if (!SpringContext.GetBean<ConfigComponent>().configInfo.gameVersion.Equals(versionInfo.gameVersion) && versionInfo.forceUpdateGame)
            {
                // 需要强制更新游戏应用
                LoginController.GetInstance().panelForceUpdate.Show();
                return;
            }

            // 设置资源更新下载地址
            var resourceManager = SpringContext.GetBean<IResourceManager>();
            resourceManager.UpdatePrefixUri = PathUtils.GetRegularPath(StringUtils.Format("{}{}/{}", versionInfo.updatePrefixUrl, versionInfo.updateSuffixUrl, GetPlatformPath()));

            checkVersionComplete = true;
            needUpdateVersion = resourceManager.CheckVersionList(versionInfo.internalResourceVersion) == CheckVersionListResult.NeedUpdate;
            if (needUpdateVersion)
            {
                LoginController.GetInstance().panelVersionUpdate.Show();
            }
        }
        
        [EventReceiver]
        private void OnWebRequestFailureEvent(WebRequestFailureEvent eve)
        {
            CommonController.GetInstance().snackbar.Error(StringUtils.Format(i18nManager.GetString(I18nEnum.check_version_error.ToString()), checkErrorTimes));
            checkErrorTimes++;
            CheckVersion();
            var sequence = DOTween.Sequence();
            sequence.AppendInterval(1.5f);
            sequence.AppendCallback(() =>
            {
                CommonController.GetInstance().snackbar.Info(i18nManager.GetString(I18nEnum.check_version_notice.ToString()));
            });
        }

        private string GetPlatformPath()
        {
            switch (Application.platform)
            {
                case RuntimePlatform.WindowsEditor:
                case RuntimePlatform.WindowsPlayer:
                    return "Windows";

                case RuntimePlatform.OSXEditor:
                case RuntimePlatform.OSXPlayer:
                    return "MacOS";

                case RuntimePlatform.IPhonePlayer:
                    return "IOS";

                case RuntimePlatform.Android:
                    return "Android";

                default:
                    throw new NotSupportedException(StringUtils.Format("Platform '{}' is not supported.", Application.platform.ToString()));
            }
        }
    }
}
