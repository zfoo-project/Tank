using UnityEngine;

namespace MiniKing.Script.UI.Common
{
    public class CommonController : MonoBehaviour
    {
        public LoadingRotate loadingRotate;

        public Snackbar snackbar;
        
        public ProgressBar progressBar;

        public PanelChangeScene panelChangeScene;
        
        private static CommonController INSTANCE = null;
        public static CommonController GetInstance()
        {
            return INSTANCE;
        }
        private void Awake()
        {
            INSTANCE = this;
        }
        
    }
}