using System.Runtime.InteropServices;

namespace Summer.Base.TaskPool
{
    /// <summary>
    /// 任务信息。
    /// </summary>
    [StructLayout(LayoutKind.Auto)]
    public struct TaskInfo
    {
        private readonly int serialId;
        private readonly int priority;
        private readonly TaskStatus status;
        private readonly string description;

        /// <summary>
        /// 初始化任务信息的新实例。
        /// </summary>
        /// <param name="serialId">任务的序列编号。</param>
        /// <param name="priority">任务的优先级。</param>
        /// <param name="status">任务状态。</param>
        /// <param name="description">任务描述。</param>
        public TaskInfo(int serialId, int priority, TaskStatus status, string description)
        {
            this.serialId = serialId;
            this.priority = priority;
            this.status = status;
            this.description = description;
        }

        /// <summary>
        /// 获取任务的序列编号。
        /// </summary>
        public int SerialId
        {
            get
            {
                return serialId;
            }
        }

        /// <summary>
        /// 获取任务的优先级。
        /// </summary>
        public int Priority
        {
            get
            {
                return priority;
            }
        }

        /// <summary>
        /// 获取任务状态。
        /// </summary>
        public TaskStatus Status
        {
            get
            {
                return status;
            }
        }

        /// <summary>
        /// 获取任务描述。
        /// </summary>
        public string Description
        {
            get
            {
                return description;
            }
        }
    }
}
