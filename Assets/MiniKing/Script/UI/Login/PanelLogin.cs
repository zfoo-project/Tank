using MiniKing.Script.Common;
using MiniKing.Script.Constant;
using MiniKing.Script.Module.Login.Service;
using Spring.Core;
using Spring.Logger;
using Spring.Util;
using Spring.Util.Property;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MiniKing.Script.UI.Login
{
    public class PanelLogin : MonoBehaviour
    {
        public TMP_InputField account;

        public TMP_InputField password;

        public Toggle remember;

        private long clickLoginTime;

        public void Show()
        {
            gameObject.SetActive(true);
        }

        public void Hide()
        {
            account.text = StringUtils.EMPTY;
            password.text = StringUtils.EMPTY;
            gameObject.SetActive(false);
        }

        public void ClickClosePanel()
        {
            Hide();
            LoginController.GetInstance().buttonLogin.Show();
        }

        public void ClickLogin()
        {
            if (TimeUtils.CurrentTimeMillis() - clickLoginTime < GameConstant.CLICK_INTERVAL)
            {
                return;
            }


            Log.Info("账号密码登录[account:{}][password:{}]", account.text, password.text);
            PlayerData.tempPairSs = new Pair<string, string>(account.text, password.text);
            SpringContext.GetBean<ILoginService>().ConnectToGateway();
        }
    }
}