using UnityEngine;
using UnityEngine.UI;

namespace MiniKing.Script.UI.Common
{
    public class PanelChangeScene : MonoBehaviour
    {
        public Image image;

        public void Show(int maxSize)
        {
            CommonController.GetInstance().progressBar.ShowNow(maxSize);
            gameObject.SetActive(true);
        }

        public void Hide()
        {
            CommonController.GetInstance().progressBar.HideNow();
            gameObject.SetActive(false);
        }
    }
}