using CsProtocol;
using Spring.Core;
using Spring.Logger;
using Summer.Net;
using Summer.Procedure;

namespace MiniKing.Script.Procedure.Scene
{
    [Bean]
    public class ProcedureLevel1 : FsmState<IProcedureFsmManager>
    {
        [Autowired]
        private INetManager netManager;

        private int score;

        public void addScore(int addScore)
        {
            this.score += addScore;
        }

        public override void OnEnter(IFsm<IProcedureFsmManager> fsm)
        {
            base.OnEnter(fsm);

            score = 0;
        }

        public override void OnUpdate(IFsm<IProcedureFsmManager> fsm, float elapseSeconds, float realElapseSeconds)
        {
            base.OnUpdate(fsm, elapseSeconds, realElapseSeconds);
        }

        public override void OnLeave(IFsm<IProcedureFsmManager> fsm, bool isShutdown)
        {
            base.OnLeave(fsm, isShutdown);

            Log.Info("score:{}", score);
            netManager.Send(BattleResultRequest.ValueOf(score));
        }
    }
}