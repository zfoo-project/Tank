using MiniKing.Script.Constant;
using Spring.Core;
using Spring.Event;
using Spring.Logger;
using Spring.Util.Property;
using Summer.Procedure;
using Summer.Resource;
using Summer.Resource.Model.Eve;

namespace MiniKing.Script.Procedure.Updater
{
    [Bean]
    public class ProcedureCheckResources : FsmState<IProcedureFsmManager>
    {
        private bool checkResourcesComplete;
        private bool needUpdateResources;
        private int updateResourceCount;
        private long updateResourceTotalZipLength;

        public override void OnEnter(IFsm<IProcedureFsmManager> fsm)
        {
            base.OnEnter(fsm);

            checkResourcesComplete = false;
            needUpdateResources = false;
            updateResourceCount = 0;
            updateResourceTotalZipLength = 0L;

            SpringContext.GetBean<IResourceManager>().CheckResources();
        }

        public override void OnUpdate(IFsm<IProcedureFsmManager> fsm, float elapseSeconds, float realElapseSeconds)
        {
            base.OnUpdate(fsm, elapseSeconds, realElapseSeconds);

            if (!checkResourcesComplete)
            {
                return;
            }

            if (needUpdateResources)
            {
                fsm.SetData(GameConstant.UPDATE_RESOURCE_INFO, new Pair<int, long>(updateResourceCount, updateResourceTotalZipLength));
                fsm.ChangeState<ProcedureUpdateResources>();
            }
            else
            {
                fsm.ChangeState<ProcedurePreLoad>();
            }
        }

        [EventReceiver]
        public void OnResourceCheckCompleteEvent(ResourceCheckCompleteEvent eve)
        {
            checkResourcesComplete = true;
            needUpdateResources = eve.updateCount > 0;
            updateResourceCount = eve.updateCount;
            updateResourceTotalZipLength = eve.updateTotalZipLength;
            Log.Info("Check resources complete, '{}' resources need to update, zip length is '{}', unzip length is '{}'."
                , eve.updateCount, eve.updateTotalZipLength, eve.updateTotalLength);
        }
    }
}