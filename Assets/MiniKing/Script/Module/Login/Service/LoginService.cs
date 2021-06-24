using System;
using CsProtocol;
using DG.Tweening;
using MiniKing.Script.Common;
using MiniKing.Script.Config;
using MiniKing.Script.Constant;
using Spring.Core;
using Spring.Event;
using Spring.Logger;
using Spring.Util;
using Summer.Net;
using Summer.Net.Core.Model;
using Summer.Setting;
using UnityEngine;

namespace MiniKing.Script.Module.Login.Service
{
    [Bean]
    public class LoginService : ILoginService
    {
        [Autowired]
        private INetManager netManager;

        [Autowired]
        private ISettingManager settingManager;


        public void ConnectToGateway()
        {
            EventBus.AsyncExecute(() =>
            {
                try
                {
                    if (Application.platform == RuntimePlatform.WebGLPlayer)
                    {
                        var websocketGatewayUrl = RandomUtils.RandomEle(SpringContext.GetBean<ConfigComponent>().configInfo.websocketGatewayUrls);
                        netManager.Connect(websocketGatewayUrl);
                    }
                    else
                    {
                        var gatewayUrl = RandomUtils.RandomEle(SpringContext.GetBean<ConfigComponent>().configInfo.gatewayUrls);
                        netManager.Connect(gatewayUrl);
                    }
                }
                catch (Exception e)
                {
                    Log.Info("连接网络发生未知异常", e);
                    var sequence = DOTween.Sequence();
                    sequence.AppendInterval(3f);
                    sequence.AppendCallback(() => { EventBus.SyncSubmit(NetErrorEvent.ValueOf()); });
                }
            });
        }

        public void LoginByToken()
        {
            var sequence = DOTween.Sequence();
            sequence.AppendCallback(() => { netManager.Send(GetPlayerInfoRequest.ValueOf(settingManager.GetString(GameConstant.SETTING_LOGIN_TOKEN))); });
            sequence.AppendInterval(8f);
            sequence.AppendCallback(() =>
            {
                if (!PlayerData.loginFlag)
                {
                    LoginByToken();
                }
            });
        }

        public void LoginByAccount()
        {
            var sequence = DOTween.Sequence();
            sequence.AppendCallback(() =>
            {
                var account = PlayerData.tempPairSs.key;
                var password = PlayerData.tempPairSs.value;
                netManager.Send(LoginRequest.ValueOf(account, password));
            });
            sequence.AppendInterval(3f);
            sequence.AppendCallback(() =>
            {
                if (!PlayerData.loginFlag)
                {
                    LoginByAccount();
                }
            });
        }

        public void Logout()
        {
            netManager.Close();
            settingManager.RemoveSetting(GameConstant.SETTING_LOGIN_TOKEN);
        }
    }
}