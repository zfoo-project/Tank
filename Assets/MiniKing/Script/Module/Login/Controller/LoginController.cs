using CsProtocol;
using DG.Tweening;
using MiniKing.Script.Common;
using MiniKing.Script.Constant;
using MiniKing.Script.Module.Login.Service;
using MiniKing.Script.UI.Common;
using MiniKing.Script.UI.Home;
using Spring.Core;
using Spring.Event;
using Spring.Logger;
using Spring.Util;
using Spring.Util.Json;
using Summer.I18n;
using Summer.Net;
using Summer.Net.Core.Model;
using Summer.Net.Dispatcher;
using Summer.Scheduler.Model;
using Summer.Setting;

namespace MiniKing.Script.Module.Login.Controller
{
    [Bean]
    public class LoginController
    {
        [Autowired]
        private ISettingManager settingManager;

        [Autowired]
        private INetManager netManager;

        [Autowired]
        private II18nManager i18nManager;

        [Autowired]
        private ILoginService loginService;

        private int reconnectCount = 0;

        [PacketReceiver]
        public void AtLoginResponse(LoginResponse response)
        {
            var token = response.token;
            Log.Info("登录返回[token:{}]", token);


            if (UI.Login.LoginController.GetInstance() != null)
            {
                var remember = UI.Login.LoginController.GetInstance().panelLogin.remember.isOn;
                if (remember)
                {
                    settingManager.SetString(GameConstant.SETTING_LOGIN_TOKEN, token);
                }

                UI.Login.LoginController.GetInstance().panelLogin.Hide();
                UI.Login.LoginController.GetInstance().buttonTapToStart.Show();
            }

            CommonController.GetInstance().loadingRotate.Hide();
        }

        [PacketReceiver]
        public void AtGetPlayerInfoResponse(GetPlayerInfoResponse response)
        {
            Log.Info("玩家的基础信息返回[playerInfo:{}]", JsonUtils.object2String(response.playerInfo));

            var playerInfo = response.playerInfo;
            var currencyVo = response.currencyVo;

            PlayerData.loginFlag = true;
            PlayerData.playerInfo = response.playerInfo;
            PlayerData.currencyVo = response.currencyVo;

            HomeController.refreshStatusBar();
            HomeController.refreshPanelHome();
        }

        [PacketReceiver]
        public void AtPong(Pong pong)
        {
            // 设置一下服务器的最新时间
            TimeUtils.SetNow(pong.time);
        }

        [EventReceiver]
        public void OnNetOpenEvent(NetOpenEvent eve)
        {
            reconnectCount = 0;
            PlayerData.loginFlag = false;

            if (settingManager.HasSetting(GameConstant.SETTING_LOGIN_TOKEN))
            {
                Log.Info("连接成功事件，通过Token登录服务器");
                loginService.LoginByToken();
            }
            else
            {
                Log.Info("连接成功事件，通过账号密码登录服务器");
                loginService.LoginByAccount();
            }
        }

        [EventReceiver]
        public void OnNetErrorEvent(NetErrorEvent eve)
        {
            reconnectCount++;

            var sequence = DOTween.Sequence();
            sequence.OnComplete(() =>
            {
                var errorMessage = StringUtils.Format(i18nManager.GetString(I18nEnum.connection_error_and_reconnect.ToString(), reconnectCount));
                CommonController.GetInstance().snackbar.Error(errorMessage);
            });

            loginService.ConnectToGateway();
        }

        [EventReceiver]
        public void OnMinuteSchedulerAsyncEvent(MinuteSchedulerAsyncEvent eve)
        {
            netManager.Send(Ping.ValueOf());
        }
    }
}