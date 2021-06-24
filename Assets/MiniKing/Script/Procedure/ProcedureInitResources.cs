using Spring.Core;
using Spring.Event;
using Spring.Logger;
using Summer.Procedure;
using Summer.Resource.Manager;
using Summer.Resource.Model.Eve;

namespace MiniKing.Script.Procedure
{
    [Bean]
    public class ProcedureInitResources : FsmState<IProcedureFsmManager>
    {
        private bool initResourcesComplete;

        public override void OnEnter(IFsm<IProcedureFsmManager> procedureOwner)
        {
            base.OnEnter(procedureOwner);

            initResourcesComplete = false;

            // 注意：使用单机模式并初始化资源前，需要先构建 AssetBundle 并复制到 StreamingAssets 中，否则会产生 HTTP 404 错误
            SpringContext.GetBean<ResourceIniter>().InitResources();
        }

        public override void OnUpdate(IFsm<IProcedureFsmManager> fsm, float elapseSeconds, float realElapseSeconds)
        {
            base.OnUpdate(fsm, elapseSeconds, realElapseSeconds);

            if (!initResourcesComplete)
            {
                // 初始化资源未完成则继续等待
                return;
            }

            fsm.ChangeState<ProcedurePreLoad>();
        }

        [EventReceiver]
        public void OnResourceInitCompleteEvent(ResourceInitCompleteEvent eve)
        {
            initResourcesComplete = true;
            Log.Info("Init resources complete.");
        }
    }
}