using MiniKing.Script.Constant;
using Spring.Core;
using Summer.Net;
using Summer.Setting;
using UnityEngine;

namespace MiniKing.Script.UI.Login
{
    public class ButtonLogout : MonoBehaviour
    {
        public void ClickLogout()
        {
            SpringContext.GetBean<INetManager>().Close();
            SpringContext.GetBean<ISettingManager>().RemoveSetting(GameConstant.SETTING_LOGIN_TOKEN);
            LoginController.GetInstance().InitLogin();
            LoginController.GetInstance().buttonLogin.Show();
        }

        public void Show()
        {
            gameObject.SetActive(true);
        }

        public void Hide()
        {
            gameObject.SetActive(false);
        }
    }
}