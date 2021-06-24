using System;
using System.Collections.Generic;

namespace Summer.Procedure
{
    /// <summary>
    /// 有限状态机管理器。
    /// </summary>
    public interface IProcedureFsmManager
    {
        Fsm<IProcedureFsmManager> ProcedureFsm
        {
            get;
        }
        
    }
}
