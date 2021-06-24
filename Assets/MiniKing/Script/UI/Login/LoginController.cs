using System;
using System.Collections;
using DG.Tweening;
using MiniKing.Script.UI.Common;
using UnityEngine;

namespace MiniKing.Script.UI.Login
{
    /**
     * 此控制器，登录成功过到主界面后，立刻销毁，不要在接下来的环节调用。
     */
    public class LoginController : MonoBehaviour
    {

        public CanvasGroup title;
        public Transform backGround;
        
        
        public ButtonTapToStart buttonTapToStart;
        public ButtonLogin buttonLogin;
        public ButtonLogout buttonLogout;
        
        public PanelLogin panelLogin;
        public PanelNetworkError panelNetworkError;
        public PanelVersionUpdate panelVersionUpdate;
        public PanelForceUpdate panelForceUpdate;
        
        
        
        private static LoginController INSTANCE = null;

        public static LoginController GetInstance()
        {
            return INSTANCE;
        }
        
        public void DestroyThis()
        {
            Destroy(gameObject.transform.parent.gameObject);
        }

        private void Awake()
        {
            INSTANCE = this;
            InitLogin();
        }

        private void OnDestroy()
        {
            INSTANCE = null;
        }

        public void InitLogin()
        {
            StartCoroutine(DoInitLogin());
        }

        public IEnumerator DoInitLogin()
        {
            title.gameObject.SetActive(true);
            backGround.gameObject.SetActive(true);
            
            panelLogin.Hide();
            buttonLogin.Hide();
            buttonLogout.Hide();
            panelNetworkError.Hide();
            buttonTapToStart.Hide();

            title.DOFade(0f, 0f);
            backGround.DOScale(1.05f, 0f);
            
            yield return new WaitForSeconds(1f);

            // title animation
            title.DOFade(1f, 0.5f).SetEase(Ease.OutCubic);
            yield return new WaitForSeconds(0.3f);

            // background animation
            backGround.transform.DOKill();
            backGround.transform.DOScale(1f, 1.5f).SetEase(Ease.Linear);

            yield return new WaitForSeconds(0.5f);
        }

    }
}