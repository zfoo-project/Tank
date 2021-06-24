using Summer.Base;
using Spring.Core;
using Spring.Util;
using UnityEngine;
using SpringComponent = Summer.Base.SpringComponent;

namespace Summer.Procedure
{
    /// <summary>
    /// 流程组件。
    /// </summary>
    [DisallowMultipleComponent]
    [AddComponentMenu("Summer/Procedure")]
    public sealed class ProcedureComponent : SpringComponent
    {
        [SerializeField]
        private string[] availableProcedureTypeNames;

        [SerializeField]
        private string entranceProcedureTypeName;

        [Autowired]
        private IProcedureFsmManager procedureFsmManager;


        public void StartProcedure()
        {
            var entranceProcedureType = AssemblyUtils.GetTypeByName(entranceProcedureTypeName);
            // 初始化流程状态机
            SpringContext.GetBean<IProcedureFsmManager>().ProcedureFsm.Start(entranceProcedureType);
        }
    }
}