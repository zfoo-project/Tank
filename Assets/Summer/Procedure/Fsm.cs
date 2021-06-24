using System;
using System.Collections.Generic;
using Spring.Collection.Reference;
using Spring.Util;
using Summer.Base.Model;

namespace Summer.Procedure
{
    /// <summary>
    /// 有限状态机。
    /// </summary>
    /// <typeparam name="T">有限状态机持有者类型。</typeparam>
    public sealed class Fsm<T> : AbstractFsm, IReference, IFsm<T> where T : class
    {
        private T owner;
        private readonly Dictionary<Type, FsmState<T>> fsmStates;
        private readonly Dictionary<string, object> datas;
        private FsmState<T> currentState;
        private float currentStateTime;
        private bool isDestroyed;

        /// <summary>
        /// 初始化有限状态机的新实例。
        /// </summary>
        public Fsm()
        {
            owner = null;
            fsmStates = new Dictionary<Type, FsmState<T>>();
            datas = new Dictionary<string, object>(StringComparer.Ordinal);
            currentState = null;
            currentStateTime = 0f;
            isDestroyed = true;
        }


        /// <summary>
        /// 获取有限状态机持有者。
        /// </summary>
        public T Owner
        {
            get { return owner; }
        }

        /// <summary>
        /// 获取有限状态机持有者类型。
        /// </summary>
        public override Type OwnerType
        {
            get { return typeof(T); }
        }


        /// <summary>
        /// 获取有限状态机是否正在运行。
        /// </summary>
        public bool IsRunning
        {
            get { return currentState != null; }
        }

        /// <summary>
        /// 获取有限状态机是否被销毁。
        /// </summary>
        public override bool IsDestroyed
        {
            get { return isDestroyed; }
        }

        /// <summary>
        /// 获取当前有限状态机状态。
        /// </summary>
        public FsmState<T> CurrentState
        {
            get { return currentState; }
        }

        /// <summary>
        /// 获取当前有限状态机状态名称。
        /// </summary>
        public string CurrentStateName
        {
            get { return currentState != null ? currentState.GetType().FullName : null; }
        }

        /// <summary>
        /// 获取当前有限状态机状态持续时间。
        /// </summary>
        public float CurrentStateTime
        {
            get { return currentStateTime; }
        }

        /// <summary>
        /// 创建有限状态机。
        /// </summary>
        /// <param name="name">有限状态机名称。</param>
        /// <param name="owner">有限状态机持有者。</param>
        /// <param name="states">有限状态机状态集合。</param>
        /// <returns>创建的有限状态机。</returns>
        public static Fsm<T> Create(T owner, params FsmState<T>[] states)
        {
            if (owner == null)
            {
                throw new GameFrameworkException("FSM owner is invalid.");
            }

            if (CollectionUtils.IsEmpty((object[]) states))
            {
                throw new GameFrameworkException("FSM states is invalid.");
            }

            var abstractFsm = ReferenceCache.Acquire<Fsm<T>>();
            abstractFsm.owner = owner;
            abstractFsm.isDestroyed = false;
            foreach (var state in states)
            {
                if (state == null)
                {
                    throw new GameFrameworkException("FSM states is invalid.");
                }

                var stateType = state.GetType();
                if (abstractFsm.fsmStates.ContainsKey(stateType))
                {
                    throw new GameFrameworkException(StringUtils.Format("FSM '{}' state '{}' is already exist.",
                        typeof(T).FullName, stateType));
                }

                abstractFsm.fsmStates.Add(stateType, state);
                state.OnInit(abstractFsm);
            }

            return abstractFsm;
        }


        /// <summary>
        /// 清理有限状态机。
        /// </summary>
        public void Clear()
        {
            if (currentState != null)
            {
                currentState.OnLeave(this, true);
            }

            foreach (KeyValuePair<Type, FsmState<T>> state in fsmStates)
            {
                state.Value.OnDestroy(this);
            }

            owner = null;
            fsmStates.Clear();
            datas.Clear();
            currentState = null;
            currentStateTime = 0f;
            isDestroyed = true;
        }

        /// <summary>
        /// 开始有限状态机。
        /// </summary>
        /// <typeparam name="TState">要开始的有限状态机状态类型。</typeparam>
        public void Start<TState>() where TState : FsmState<T>
        {
            if (IsRunning)
            {
                throw new GameFrameworkException("FSM is running, can not start again.");
            }

            FsmState<T> state = GetState<TState>();
            if (state == null)
            {
                throw new GameFrameworkException(StringUtils.Format(
                    "FSM '{}' can not start state '{}' which is not exist.", typeof(T).Name,
                    typeof(TState).FullName));
            }

            currentStateTime = 0f;
            currentState = state;
            currentState.OnEnter(this);
        }

        /// <summary>
        /// 开始有限状态机。
        /// </summary>
        /// <param name="stateType">要开始的有限状态机状态类型。</param>
        public void Start(Type stateType)
        {
            if (IsRunning)
            {
                throw new GameFrameworkException("FSM is running, can not start again.");
            }

            if (stateType == null)
            {
                throw new GameFrameworkException("State type is invalid.");
            }

            if (!typeof(FsmState<T>).IsAssignableFrom(stateType))
            {
                throw new GameFrameworkException(
                    StringUtils.Format("State type '{}' is invalid.", stateType.FullName));
            }

            FsmState<T> state = GetState(stateType);
            if (state == null)
            {
                throw new GameFrameworkException(StringUtils.Format(
                    "FSM '{}' can not start state '{}' which is not exist.", typeof(T).Name, stateType.FullName));
            }

            currentStateTime = 0f;
            currentState = state;
            currentState.OnEnter(this);
        }

        /// <summary>
        /// 是否存在有限状态机状态。
        /// </summary>
        /// <typeparam name="TState">要检查的有限状态机状态类型。</typeparam>
        /// <returns>是否存在有限状态机状态。</returns>
        public bool HasState<TState>() where TState : FsmState<T>
        {
            return fsmStates.ContainsKey(typeof(TState));
        }

        /// <summary>
        /// 是否存在有限状态机状态。
        /// </summary>
        /// <param name="stateType">要检查的有限状态机状态类型。</param>
        /// <returns>是否存在有限状态机状态。</returns>
        public bool HasState(Type stateType)
        {
            if (stateType == null)
            {
                throw new GameFrameworkException("State type is invalid.");
            }

            if (!typeof(FsmState<T>).IsAssignableFrom(stateType))
            {
                throw new GameFrameworkException(
                    StringUtils.Format("State type '{}' is invalid.", stateType.FullName));
            }

            return fsmStates.ContainsKey(stateType);
        }

        /// <summary>
        /// 获取有限状态机状态。
        /// </summary>
        /// <typeparam name="TState">要获取的有限状态机状态类型。</typeparam>
        /// <returns>要获取的有限状态机状态。</returns>
        public TState GetState<TState>() where TState : FsmState<T>
        {
            FsmState<T> state = null;
            if (fsmStates.TryGetValue(typeof(TState), out state))
            {
                return (TState) state;
            }

            return null;
        }

        /// <summary>
        /// 获取有限状态机状态。
        /// </summary>
        /// <param name="stateType">要获取的有限状态机状态类型。</param>
        /// <returns>要获取的有限状态机状态。</returns>
        public FsmState<T> GetState(Type stateType)
        {
            if (stateType == null)
            {
                throw new GameFrameworkException("State type is invalid.");
            }

            if (!typeof(FsmState<T>).IsAssignableFrom(stateType))
            {
                throw new GameFrameworkException(
                    StringUtils.Format("State type '{}' is invalid.", stateType.FullName));
            }

            FsmState<T> state = null;
            if (fsmStates.TryGetValue(stateType, out state))
            {
                return state;
            }

            return null;
        }

        /// <summary>
        /// 获取有限状态机的所有状态。
        /// </summary>
        /// <param name="results">有限状态机的所有状态。</param>
        public void GetAllStates(List<FsmState<T>> results)
        {
            results.Clear();
            foreach (KeyValuePair<Type, FsmState<T>> state in fsmStates)
            {
                results.Add(state.Value);
            }
        }

        /// <summary>
        /// 是否存在有限状态机数据。
        /// </summary>
        /// <param name="name">有限状态机数据名称。</param>
        /// <returns>有限状态机数据是否存在。</returns>
        public bool HasData(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new GameFrameworkException("Data name is invalid.");
            }

            return datas.ContainsKey(name);
        }

        /// <summary>
        /// 获取有限状态机数据。
        /// </summary>
        /// <typeparam name="TData">要获取的有限状态机数据的类型。</typeparam>
        /// <param name="name">有限状态机数据名称。</param>
        /// <returns>要获取的有限状态机数据。</returns>
        public TData GetData<TData>(string name)
        {
            return (TData) GetData(name);
        }

        /// <summary>
        /// 获取有限状态机数据。
        /// </summary>
        /// <param name="name">有限状态机数据名称。</param>
        /// <returns>要获取的有限状态机数据。</returns>
        public object GetData(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new GameFrameworkException("Data name is invalid.");
            }

            object data = null;
            if (datas.TryGetValue(name, out data))
            {
                return data;
            }

            return null;
        }

        /// <summary>
        /// 设置有限状态机数据。
        /// </summary>
        /// <typeparam name="TData">要设置的有限状态机数据的类型。</typeparam>
        /// <param name="name">有限状态机数据名称。</param>
        /// <param name="data">要设置的有限状态机数据。</param>
        public void SetData(string name, object data)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new GameFrameworkException("Data name is invalid.");
            }

            datas[name] = data;
        }

        public void CleadData()
        {
            datas.Clear();
        }

        /// <summary>
        /// 移除有限状态机数据。
        /// </summary>
        /// <param name="name">有限状态机数据名称。</param>
        /// <returns>是否移除有限状态机数据成功。</returns>
        public bool RemoveData(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new GameFrameworkException("Data name is invalid.");
            }

            return datas.Remove(name);
        }

        /// <summary>
        /// 有限状态机轮询。
        /// </summary>
        /// <param name="elapseSeconds">逻辑流逝时间，以秒为单位。</param>
        /// <param name="realElapseSeconds">真实流逝时间，以秒为单位。</param>
        public override void Update(float elapseSeconds, float realElapseSeconds)
        {
            if (currentState == null)
            {
                return;
            }

            currentStateTime += elapseSeconds;
            currentState.OnUpdate(this, elapseSeconds, realElapseSeconds);
        }

        /// <summary>
        /// 关闭并清理有限状态机。
        /// </summary>
        public override void Shutdown()
        {
            ReferenceCache.Release(this);
        }

        public void ChangeState<TNextState>() where TNextState : FsmState<T>
        {
            var stateType = typeof(TNextState);
            ChangeState(stateType);
        }

        public void ChangeState(Type stateType)
        {
            if (currentState == null)
            {
                throw new GameFrameworkException("Current state is invalid.");
            }

            FsmState<T> state = GetState(stateType);
            if (state == null)
            {
                throw new GameFrameworkException(StringUtils.Format("FSM '{}' can not change state to '{}' which is not exist.", typeof(T).Name, stateType.FullName));
            }

            currentState.OnLeave(this, false);
            currentStateTime = 0f;
            currentState = state;
            currentState.OnEnter(this);
        }
    }
}