using Spring.Core;
using Summer.Base;
using UnityEngine;

namespace MiniKing.Script.UI.Login
{
    public class PanelNetworkError : MonoBehaviour
    {
        public void ClickShutdownGame()
        {
            SpringContext.GetBean<BaseComponent>().Shutdown(ShutdownType.Quit);
        }

        public void Hide()
        {
            gameObject.SetActive(false);
        }

        public void Show()
        {
            gameObject.SetActive(true);
        }
    }
}