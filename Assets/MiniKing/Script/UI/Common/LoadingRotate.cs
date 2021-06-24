using System.Collections;
using DG.Tweening;
using UnityEngine;

namespace MiniKing.Script.UI.Common
{
    public class LoadingRotate : MonoBehaviour
    {
        public Transform[] loadingDot;
        private int loadingCount = 0;

        public float dotAnimTime = 1f;
        public float dotAnimDistanceTime = 0.2f;

        public void Show()
        {
            for (int i = 0; i < loadingDot.Length; i++)
            {
                loadingDot[i].DOKill();
                loadingDot[i].DOScale(0.2f + 0.8f / loadingDot.Length * i, 0f).SetUpdate(true);
                loadingDot[i].transform.DOScale(0.2f, dotAnimTime / loadingDot.Length * i).SetEase(Ease.InSine)
                    .SetUpdate(true);
            }

            gameObject.SetActive(true);
            StartCoroutine(LoadingCo());
        }


        public void Hide()
        {
            gameObject.SetActive(false);
        }

        IEnumerator LoadingCo()
        {
            while (true)
            {
                loadingDot[loadingCount].DOKill();
                loadingDot[loadingCount].DOScale(1f, 0f);
                yield return new WaitForSeconds(dotAnimDistanceTime);
            
                loadingDot[loadingCount].transform.DOScale(0.2f, dotAnimTime).SetEase(Ease.InSine);
                loadingCount += 1;

                if (loadingCount > loadingDot.Length - 1)
                {
                    loadingCount = 0;
                }
            }
        }


        private void OnDisable()
        {
            StopAllCoroutines();
        }
    }
}
