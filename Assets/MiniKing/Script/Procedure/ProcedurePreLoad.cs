using System.Collections.Generic;
using System.Linq;
using MiniKing.Script.Constant;
using MiniKing.Script.Procedure.Scene;
using MiniKing.Script.UI.Common;
using MiniKing.Script.UI.Login;
using Spring.Core;
using Spring.Event;
using Spring.Logger;
using Spring.Storage;
using Spring.Storage.Helper;
using Spring.Storage.Model.Anno;
using Spring.Util;
using Summer.I18n;
using Summer.Procedure;
using ProcedureOwner = Summer.Procedure.IFsm<Summer.Procedure.IProcedureFsmManager>;

namespace MiniKing.Script.Procedure
{
    [Bean]
    public class ProcedurePreLoad : FsmState<IProcedureFsmManager>
    {
        [Autowired]
        private I18nManager i18NManager;

        private Dictionary<string, bool> loadedFlagMap = new Dictionary<string, bool>();

        private int progressCount;
        private int totalProgress;

        public override void OnEnter(IFsm<IProcedureFsmManager> fsm)
        {
            base.OnEnter(fsm);

            progressCount = 0;
            totalProgress = 0;
            loadedFlagMap.Clear();

            // Preload Excel
            LoadStorage();

            CommonController.GetInstance().progressBar.Show(totalProgress);
            RefreshProgressBar();
        }

        public override void OnLeave(ProcedureOwner fsm, bool isShutdown)
        {
            base.OnLeave(fsm, isShutdown);
            loadedFlagMap.Clear();
            CommonController.GetInstance().progressBar.Hide();
        }

        public override void OnUpdate(IFsm<IProcedureFsmManager> fsm, float elapseSeconds, float realElapseSeconds)
        {
            base.OnUpdate(fsm, elapseSeconds, realElapseSeconds);

            foreach (var loadedFlag in loadedFlagMap)
            {
                if (!loadedFlag.Value)
                {
                    return;
                }
            }

            // 注入storage
            var storageManager = StorageContext.GetStorageManager();
            storageManager.InitAfter();

            // fsm.SetData(SceneConstant.NEXT_SCENE_ENUM, SceneEnum.Login);
            fsm.ChangeState<ProcedureLogin>();
        }

        private void LoadStorage()
        {
            var classTypes = AssemblyUtils.GetAllClassTypes();
            classTypes.Where(it => it.IsDefined(typeof(Resource), false))
                .ToList()
                .ForEach(it => loadedFlagMap.Add(it.FullName, false));

            totalProgress += loadedFlagMap.Count;
            StorageContext.GetStorageHelper().InitStorage();
        }

        private void RefreshProgressBar()
        {
            var message = i18NManager.GetString(I18nEnum.load_progress_tip.ToString(), progressCount, totalProgress);
            CommonController.GetInstance().progressBar.SetBar(progressCount, message);
        }


        [EventReceiver]
        public void OnLoadStorageSuccessEvent(LoadStorageSuccessEvent eve)
        {
            loadedFlagMap[eve.resourceType.FullName] = true;
            Log.Info("Load excel '{}' OK.", eve.resourceType.Name);

            progressCount++;
            RefreshProgressBar();
        }
    }
}