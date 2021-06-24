using DG.Tweening;
using MiniKing.Script.Common;
using MiniKing.Script.Module.Login.Service;
using Spring.Core;
using Spring.Logger;
using Spring.Util.Property;
using UnityEngine;

namespace MiniKing.Script.UI.Login
{
    public class ButtonLogin : MonoBehaviour
    {

        public Transform mobile;

        public Transform guest;
        

        public void Show()
        {
            gameObject.SetActive(true);

            mobile.DOKill();
            mobile.DOScale(1f, 1f).SetEase(Ease.OutBack).SetDelay(0.2f * 0);
            
            guest.DOKill();
            guest.DOScale(1f, 1f).SetEase(Ease.OutBack).SetDelay(0.2f * 1);
        }

        public void Hide()
        {
            mobile.DOKill();
            mobile.DOScale(0f, 0f);
            
            guest.DOKill();
            guest.DOScale(0f, 0f);

            gameObject.SetActive(false);
        }

        public void ClickLoginByPhone()
        {
            Hide();
            LoginController.GetInstance().panelLogin.Show();
        }

        public void ClickLoginByGuest()
        {
            Hide();
            var uid = SystemInfo.deviceUniqueIdentifier;
            
            Log.Info("游客登录[account:{}][password:{}]", uid, uid);
            PlayerData.tempPairSs = new Pair<string, string>(uid, uid);
            SpringContext.GetBean<ILoginService>().ConnectToGateway();
        }
    }
}