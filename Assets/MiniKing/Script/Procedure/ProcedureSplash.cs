using MiniKing.Script.Procedure.Updater;
using MiniKing.Script.UI.Login;
using Spring.Core;
using Spring.Logger;
using Summer.Base;
using Summer.Procedure;
using Summer.Resource;
using Summer.Resource.Model.Constant;
using UnityEngine;

namespace MiniKing.Script.Procedure
{
    [Bean]
    public class ProcedureSplash : FsmState<IProcedureFsmManager>
    {
        private bool checkNetworkComplete;

        public override void OnEnter(IFsm<IProcedureFsmManager> fsm)
        {
            base.OnEnter(fsm);

            // 如果没有网络，则直接跳出弹窗返回
            if (Application.internetReachability == NetworkReachability.NotReachable)
            {
                checkNetworkComplete = false;
                LoginController.GetInstance().panelNetworkError.Show();
            }
            else
            {
                checkNetworkComplete = true;
            }
        }

        public override void OnUpdate(IFsm<IProcedureFsmManager> fsm, float elapseSeconds, float realElapseSeconds)
        {
            base.OnUpdate(fsm, elapseSeconds, realElapseSeconds);

            if (!checkNetworkComplete)
            {
                return;
            }

            var baseComponent = SpringContext.GetBean<BaseComponent>();
            if (baseComponent.editorResourceMode)
            {
                // 编辑器模式
                Log.Info("Editor resource mode detected.");
                fsm.ChangeState<ProcedurePreLoad>();
            }
            else if (SpringContext.GetBean<IResourceManager>().ResourceMode == ResourceMode.Package)
            {
                // 单机模式
                Log.Info("Package resource mode detected.");
                fsm.ChangeState<ProcedureInitResources>();
            }
            else
            {
                // 可更新模式
                Log.Info("Updatable resource mode detected.");
                fsm.ChangeState<ProcedureCheckVersion>();
            }
        }
    }
}