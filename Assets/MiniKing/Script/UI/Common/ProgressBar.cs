using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MiniKing.Script.UI.Common
{
    public class ProgressBar : MonoBehaviour
    {
        private Slider sliderDownload;
        private CanvasGroup canvasGroup;
        public TextMeshProUGUI textDownload;

        private void Init(int maxSize)
        {
            sliderDownload = GetComponent<Slider>();
            canvasGroup = GetComponent<CanvasGroup>();

            sliderDownload.value = 0;
            sliderDownload.maxValue = maxSize;
        }

        public void Show(int maxSize)
        {
            Init(maxSize);
            
            gameObject.SetActive(true);
            canvasGroup.DOKill();
            canvasGroup.alpha = 0f;
            canvasGroup.DOFade(1f, 0.25f).SetEase(Ease.OutCubic);
        }

        public void ShowNow(int maxSize)
        {
            Init(maxSize);
            gameObject.SetActive(true);
            canvasGroup.alpha = 1f;
        }

        public void HideNow()
        {
            gameObject.SetActive(false);
            canvasGroup.DOKill();
        }
        

        public void Hide()
        {
            canvasGroup.DOKill();
            canvasGroup.DOFade(0f, 0.25f).SetEase(Ease.OutCubic).OnComplete(() => { gameObject.SetActive(false); });
        }

        public void SetBar(int value, string message)
        {
            if (sliderDownload != null)
            {
                sliderDownload.value = value;
            }

            if (textDownload != null)
            {
                textDownload.text = message;
            }
        }
    }
}