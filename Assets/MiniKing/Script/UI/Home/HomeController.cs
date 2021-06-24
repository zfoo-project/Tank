using UnityEngine;

namespace MiniKing.Script.UI.Home
{
    public class HomeController : MonoBehaviour
    {
        public StatusBar statusBar;

        public ButtonPlay buttonPlay;

        public PanelHome panelHome;

        public PanelRanking panelRanking;
        
        private static HomeController INSTANCE = null;

        public static HomeController GetInstance()
        {
            return INSTANCE;
        }

        private void Awake()
        {
            INSTANCE = this;
        }

        private void OnDestroy()
        {
            INSTANCE = null;
        }

        public static void refreshStatusBar()
        {
            if (INSTANCE == null)
            {
                return;
            }

            if (INSTANCE.statusBar == null)
            {
                return;
            }

            INSTANCE.statusBar.refresh();
        }
        
        public static void refreshPanelHome()
        {
            if (INSTANCE == null)
            {
                return;
            }

            if (INSTANCE.panelHome == null)
            {
                return;
            }

            INSTANCE.panelHome.refresh();
        }
    }
}