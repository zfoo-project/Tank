using MiniKing.Script.UI.Home;
using Spring.Core;
using Summer.Procedure;

namespace MiniKing.Script.Procedure.Scene
{
    [Bean]
    public class ProcedureMenu : FsmState<IProcedureFsmManager>
    {
        public override void OnEnter(IFsm<IProcedureFsmManager> fsm)
        {
            base.OnEnter(fsm);
            HomeController.refreshStatusBar();
            HomeController.refreshPanelHome();
        }

        public override void OnUpdate(IFsm<IProcedureFsmManager> fsm, float elapseSeconds, float realElapseSeconds)
        {
            base.OnUpdate(fsm, elapseSeconds, realElapseSeconds);
        }
    }
}