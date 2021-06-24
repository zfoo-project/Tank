using DG.Tweening;
using MiniKing.Script.Config;
using MiniKing.Script.Constant;
using MiniKing.Script.UI.Common;
using Spring.Core;
using Spring.Logger;
using Summer.I18n;
using Summer.Procedure;
using Summer.Resource;
using Summer.Resource.Model.UpdatableVersion;

namespace MiniKing.Script.Procedure.Updater
{
    [Bean]
    public class ProcedureUpdateVersion : FsmState<IProcedureFsmManager>
    {
        private bool updateVersionComplete;
        private UpdateVersionListCallbacks updateVersionListCallbacks;

        [Autowired]
        private IResourceManager resourceManager;

        [Autowired]
        private II18nManager i18nManager;

        public override void OnInit(IFsm<IProcedureFsmManager> fsm)
        {
            base.OnInit(fsm);

            updateVersionListCallbacks = new UpdateVersionListCallbacks(OnUpdateVersionListSuccess, OnUpdateVersionListFailure);
        }

        public override void OnEnter(IFsm<IProcedureFsmManager> fsm)
        {
            base.OnEnter(fsm);

            updateVersionComplete = false;

            var versionInfo = fsm.GetData<VersionInfo>(GameConstant.VERSION_INFO);
            resourceManager.UpdateVersionList(versionInfo.versionListLength, versionInfo.versionListHashCode, versionInfo.versionListZipLength, versionInfo.versionListZipHashCode, updateVersionListCallbacks);
            fsm.CleadData();

            CommonController.GetInstance().loadingRotate.Show();
        }

        public override void OnUpdate(IFsm<IProcedureFsmManager> fsm, float elapseSeconds, float realElapseSeconds)
        {
            base.OnUpdate(fsm, elapseSeconds, realElapseSeconds);

            if (!updateVersionComplete)
            {
                return;
            }

            fsm.ChangeState<ProcedureCheckResources>();
        }

        private void OnUpdateVersionListSuccess(string downloadPath, string downloadUri)
        {
            updateVersionComplete = true;
            Log.Info("Update version list from '{}' success.", downloadUri);
            CommonController.GetInstance().loadingRotate.Hide();
        }

        private void OnUpdateVersionListFailure(string downloadUri, string errorMessage)
        {
            Log.Warning("Update version list from '{}' failure, error message is '{}'.", downloadUri, errorMessage);
            CommonController.GetInstance().snackbar.Error(i18nManager.GetString(I18nEnum.update_version_error.ToString()));
            CommonController.GetInstance().snackbar.Info(errorMessage);


            var sequence = DOTween.Sequence();
            sequence.AppendInterval(3f);
            sequence.AppendCallback(() =>
            {
                var fsm = SpringContext.GetBean<IProcedureFsmManager>().ProcedureFsm;
                fsm.ChangeState<ProcedureCheckVersion>();
            });
        }
    }
}