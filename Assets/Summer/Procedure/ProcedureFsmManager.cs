using System.Linq;
using Summer.Base;
using Summer.Base.Model;
using Spring.Core;

namespace Summer.Procedure
{
    /// <summary>
    /// 有限状态机管理器。
    /// </summary>
    [Bean]
    public sealed class ProcedureFsmManager : AbstractManager, IProcedureFsmManager
    {
        private Fsm<IProcedureFsmManager> procedureFsm;

        public Fsm<IProcedureFsmManager> ProcedureFsm => procedureFsm;

        /// <summary>
        /// 获取游戏框架模块优先级。
        /// </summary>
        /// <remarks>优先级较高的模块会优先轮询，并且关闭操作会后进行。</remarks>
        public override int Priority
        {
            get { return -10; }
        }

        [BeforePostConstruct]
        private void Init()
        {
            var fsmStates = SpringContext.GetBeans(typeof(FsmState<IProcedureFsmManager>))
                .Select(it => (FsmState<IProcedureFsmManager>) it)
                .ToArray();
            procedureFsm = Fsm<IProcedureFsmManager>.Create(this, fsmStates);
        }

        /// <summary>
        /// 有限状态机管理器轮询。
        /// </summary>
        /// <param name="elapseSeconds">逻辑流逝时间，以秒为单位。</param>
        /// <param name="realElapseSeconds">真实流逝时间，以秒为单位。</param>
        public override void Update(float elapseSeconds, float realElapseSeconds)
        {
            procedureFsm.Update(elapseSeconds, realElapseSeconds);
        }

        /// <summary>
        /// 关闭并清理有限状态机管理器。
        /// </summary>
        public override void Shutdown()
        {
            procedureFsm.Shutdown();
        }

    }
}