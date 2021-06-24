using DG.Tweening;
using MiniKing.Script.Constant;
using MiniKing.Script.Procedure.Scene;
using MiniKing.Script.UI.Common;
using Spring.Core;
using UnityEngine;

namespace MiniKing.Script.UI.Login
{
    public class ButtonTapToStart : MonoBehaviour
    {
        public void Hide()
        {
            gameObject.SetActive(false);
        }

        public void Show()
        {
            var sequence = DOTween.Sequence();
            sequence.AppendCallback(() => { CommonController.GetInstance().loadingRotate.Show(); });
            sequence.AppendInterval(1.5f);
            sequence.AppendCallback(() =>
            {
                CommonController.GetInstance().loadingRotate.Hide();
                LoginController.GetInstance().title.transform.DOLocalMoveY(43f, 0.5f).SetEase(Ease.OutCubic).SetRelative(true);
                LoginController.GetInstance().title.transform.DOScale(0.791f, 0.5f).SetEase(Ease.OutCubic);
            });
            sequence.AppendInterval(0.5f);
            sequence.AppendCallback(() =>
            {
                gameObject.SetActive(true);
                var buttonTapToStartCanvasGroup = GetComponentInChildren<CanvasGroup>();
                buttonTapToStartCanvasGroup.DOFade(1f, 1.2f).SetEase(Ease.Linear).SetLoops(-1, LoopType.Yoyo);
                LoginController.GetInstance().buttonLogout.Show();
            });
        }


        public void StartGame()
        {
            var buttonTapToStartCanvasGroup = GetComponentInChildren<CanvasGroup>();
            buttonTapToStartCanvasGroup.DOKill();
            buttonTapToStartCanvasGroup.DOFade(0f, 0f);
            buttonTapToStartCanvasGroup.DOFade(1f, 0.2f).SetEase(Ease.Linear).SetLoops(3, LoopType.Yoyo)
                .OnComplete(() =>
                {
                    // PlayManager.Instance.LoadScene(Data.scene_home);
                    SpringContext.GetBean<ProcedureChangeScene>().ChangeScene(SceneEnum.Menu);
                    LoginController.GetInstance().DestroyThis();
                });
        }
    }
}