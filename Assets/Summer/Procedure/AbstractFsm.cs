using System;

namespace Summer.Procedure
{
    /// <summary>
    /// 有限状态机基类。
    /// </summary>
    public abstract class AbstractFsm
    {


        /// <summary>
        /// 获取有限状态机名称。
        /// </summary>
        public string Name => OwnerType.FullName;


        /// <summary>
        /// 获取有限状态机持有者类型。
        /// </summary>
        public abstract Type OwnerType { get; }


        /// <summary>
        /// 获取有限状态机是否被销毁。
        /// </summary>
        public abstract bool IsDestroyed { get; }


        /// <summary>
        /// 有限状态机轮询。
        /// </summary>
        /// <param name="elapseSeconds">逻辑流逝时间，以秒为单位。</param>
        /// <param name="realElapseSeconds">当前已流逝时间，以秒为单位。</param>
        public abstract void Update(float elapseSeconds, float realElapseSeconds);

        /// <summary>
        /// 关闭并清理有限状态机。
        /// </summary>
        public abstract void Shutdown();
    }
}