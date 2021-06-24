using System;

namespace Summer.Procedure
{
    /// <summary>
    /// 有限状态机接口。
    /// </summary>
    /// <typeparam name="T">有限状态机持有者类型。</typeparam>
    public interface IFsm<T> where T : class
    {
        /// <summary>
        /// 获取当前有限状态机状态。
        /// </summary>
        FsmState<T> CurrentState { get; }


        /// <summary>
        /// 开始有限状态机。
        /// </summary>
        /// <param name="stateType">要开始的有限状态机状态类型。</param>
        void Start(Type stateType);


        /// <summary>
        /// 获取有限状态机数据。
        /// </summary>
        /// <typeparam name="TData">要获取的有限状态机数据的类型。</typeparam>
        /// <param name="name">有限状态机数据名称。</param>
        /// <returns>要获取的有限状态机数据。</returns>
        TData GetData<TData>(string name);


        /// <summary>
        /// 设置有限状态机数据。
        /// </summary>
        /// <typeparam name="TData">要设置的有限状态机数据的类型。</typeparam>
        /// <param name="name">有限状态机数据名称。</param>
        /// <param name="data">要设置的有限状态机数据。</param>
        void SetData(string name, object data);

        void CleadData();

        /// <summary>
        /// 移除有限状态机数据。
        /// </summary>
        /// <param name="name">有限状态机数据名称。</param>
        /// <returns>是否移除有限状态机数据成功。</returns>
        bool RemoveData(string name);

        void ChangeState<TNextState>() where TNextState : FsmState<T>;
        
        void ChangeState(Type stateType);
    }
}