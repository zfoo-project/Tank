using System.Collections;
using DG.Tweening;
using TMPro;
using UnityEngine;

namespace MiniKing.Script.UI.Common
{
    public class Snackbar : MonoBehaviour
    {
        public CanvasGroup serverErrorCanvasGroup;
        public TextMeshProUGUI serverError;

        public CanvasGroup errorCanvasGroup;
        public TextMeshProUGUI error;

        public CanvasGroup infoCanvasGroup;
        public TextMeshProUGUI info;

        public CanvasGroup serverInfoCanvasGroup;
        public TextMeshProUGUI serverInfo;

        public void ServerError(string message)
        {
            if (string.IsNullOrEmpty(message))
            {
                return;
            }

            serverError.text = message;
            serverErrorCanvasGroup.gameObject.SetActive(true);
            serverErrorCanvasGroup.alpha = 0f;
            serverErrorCanvasGroup.DOFade(1f, 0.8f)
                .SetEase(Ease.Linear)
                .SetLoops(1, LoopType.Yoyo);
            StartCoroutine(HideServerError(message));
        }

        private IEnumerator HideServerError(string message)
        {
            yield return new WaitForSeconds(3f);
            if (message.Equals(serverError.text))
            {
                serverErrorCanvasGroup.DOFade(0f, 1.2f)
                    .SetEase(Ease.Linear)
                    .SetLoops(1, LoopType.Yoyo)
                    .OnComplete(() =>
                    {
                        if (message.Equals(serverError.text))
                        {
                            serverError.text = null;
                            serverErrorCanvasGroup.gameObject.SetActive(false);
                        }
                    });
            }
        }


        public void Error(string message)
        {
            if (string.IsNullOrEmpty(message))
            {
                return;
            }

            error.text = message;
            errorCanvasGroup.gameObject.SetActive(true);
            errorCanvasGroup.alpha = 0f;
            errorCanvasGroup.DOFade(1f, 0.8f)
                .SetEase(Ease.Linear)
                .SetLoops(1, LoopType.Yoyo);
            StartCoroutine(HideError(message));
        }

        private IEnumerator HideError(string message)
        {
            yield return new WaitForSeconds(3f);
            if (message.Equals(error.text))
            {
                errorCanvasGroup.DOFade(0f, 1.2f)
                    .SetEase(Ease.Linear)
                    .SetLoops(1, LoopType.Yoyo)
                    .OnComplete(() =>
                    {
                        if (message.Equals(error.text))
                        {
                            error.text = null;
                            errorCanvasGroup.gameObject.SetActive(false);
                        }
                    });
            }
        }


        public void Info(string message)
        {
            if (string.IsNullOrEmpty(message))
            {
                return;
            }

            info.text = message;
            infoCanvasGroup.gameObject.SetActive(true);
            infoCanvasGroup.alpha = 0f;
            infoCanvasGroup.DOFade(1f, 0.8f)
                .SetEase(Ease.Linear)
                .SetLoops(1, LoopType.Yoyo);
            StartCoroutine(HideInfo(message));
        }

        private IEnumerator HideInfo(string message)
        {
            yield return new WaitForSeconds(3f);

            if (message.Equals(info.text))
            {
                infoCanvasGroup.DOFade(0f, 1.2f)
                    .SetEase(Ease.Linear)
                    .SetLoops(1, LoopType.Yoyo)
                    .OnComplete(() =>
                    {
                        if (message.Equals(info.text))
                        {
                            info.text = null;
                            infoCanvasGroup.gameObject.SetActive(false);
                        }
                    });
            }
        }

        public void ServerInfo(string message)
        {
            if (string.IsNullOrEmpty(message))
            {
                return;
            }

            serverInfo.text = message;
            serverInfoCanvasGroup.gameObject.SetActive(true);
            serverInfoCanvasGroup.alpha = 0f;
            serverInfoCanvasGroup.DOFade(1f, 0.8f)
                .SetEase(Ease.Linear)
                .SetLoops(1, LoopType.Yoyo);
            StartCoroutine(HideServerInfo(message));
        }

        private IEnumerator HideServerInfo(string message)
        {
            yield return new WaitForSeconds(3f);
            if (message.Equals(serverInfo.text))
            {
                serverInfoCanvasGroup.DOFade(0f, 1.2f)
                    .SetEase(Ease.Linear)
                    .SetLoops(1, LoopType.Yoyo)
                    .OnComplete(() =>
                    {
                        if (message.Equals(serverInfo.text))
                        {
                            serverInfo.text = null;
                            serverInfoCanvasGroup.gameObject.SetActive(false);
                        }
                    });
            }
        }
    }
}