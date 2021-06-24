using MiniKing.Script.Procedure.Updater;
using Spring.Core;
using Summer.Base;
using UnityEngine;

namespace MiniKing.Script.UI.Login
{
    public class PanelVersionUpdate : MonoBehaviour
    {
        public void ClickShutdownGame()
        {
            SpringContext.GetBean<BaseComponent>().Shutdown(ShutdownType.Quit);
        }

        public void ClickUpdateGame()
        {
            SpringContext.GetBean<ProcedureCheckVersion>().confirmUpdateVersion = true;
            Hide();
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