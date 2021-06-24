using System.Collections.Generic;
using Summer.Base.Model;
using Spring.Collection;
using Spring.Collection.Reference;

namespace Summer.Base.TaskPool
{
    /// <summary>
    /// 任务池。
    /// </summary>
    /// <typeparam name="T">任务类型。</typeparam>
    sealed class TaskPool<T> where T : TaskBase
    {
        private readonly Stack<ITaskAgent<T>> freeAgents = new Stack<ITaskAgent<T>>();
        private readonly CachedLinkedList<ITaskAgent<T>> workingAgents = new CachedLinkedList<ITaskAgent<T>>();
        private readonly CachedLinkedList<T> waitingTasks = new CachedLinkedList<T>();


        /// <summary>
        /// 获取任务代理总数量。
        /// </summary>
        public int TotalAgentCount
        {
            get { return FreeAgentCount + WorkingAgentCount; }
        }

        /// <summary>
        /// 获取可用任务代理数量。
        /// </summary>
        public int FreeAgentCount
        {
            get { return freeAgents.Count; }
        }

        /// <summary>
        /// 获取工作中任务代理数量。
        /// </summary>
        public int WorkingAgentCount
        {
            get { return workingAgents.Count; }
        }

        /// <summary>
        /// 获取等待任务数量。
        /// </summary>
        public int WaitingTaskCount
        {
            get { return waitingTasks.Count; }
        }

        /// <summary>
        /// 任务池轮询。
        /// </summary>
        /// <param name="elapseSeconds">逻辑流逝时间，以秒为单位。</param>
        /// <param name="realElapseSeconds">真实流逝时间，以秒为单位。</param>
        public void Update(float elapseSeconds, float realElapseSeconds)
        {
            ProcessRunningTasks(elapseSeconds, realElapseSeconds);
            ProcessWaitingTasks(elapseSeconds, realElapseSeconds);
        }

        /// <summary>
        /// 关闭并清理任务池。
        /// </summary>
        public void Shutdown()
        {
            RemoveAllTasks();

            while (FreeAgentCount > 0)
            {
                freeAgents.Pop().Shutdown();
            }
        }

        /// <summary>
        /// 增加任务代理。
        /// </summary>
        /// <param name="agent">要增加的任务代理。</param>
        public void AddAgent(ITaskAgent<T> agent)
        {
            if (agent == null)
            {
                throw new GameFrameworkException("Task agent is invalid.");
            }

            agent.Initialize();
            freeAgents.Push(agent);
        }

        /// <summary>
        /// 增加任务。
        /// </summary>
        /// <param name="task">要增加的任务。</param>
        public void AddTask(T task)
        {
            LinkedListNode<T> current = waitingTasks.Last;
            while (current != null)
            {
                if (task.Priority <= current.Value.Priority)
                {
                    break;
                }

                current = current.Previous;
            }

            if (current != null)
            {
                waitingTasks.AddAfter(current, task);
            }
            else
            {
                waitingTasks.AddFirst(task);
            }
        }

        /// <summary>
        /// 移除任务。
        /// </summary>
        /// <param name="serialId">要移除任务的序列编号。</param>
        /// <returns>是否移除任务成功。</returns>
        public bool RemoveTask(int serialId)
        {
            foreach (T task in waitingTasks)
            {
                if (task.SerialId == serialId)
                {
                    waitingTasks.Remove(task);
                    ReferenceCache.Release(task);
                    return true;
                }
            }

            foreach (ITaskAgent<T> workingAgent in workingAgents)
            {
                if (workingAgent.Task.SerialId == serialId)
                {
                    T task = workingAgent.Task;
                    workingAgent.Reset();
                    freeAgents.Push(workingAgent);
                    workingAgents.Remove(workingAgent);
                    ReferenceCache.Release(task);
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// 移除所有任务。
        /// </summary>
        public void RemoveAllTasks()
        {
            foreach (T task in waitingTasks)
            {
                ReferenceCache.Release(task);
            }

            waitingTasks.Clear();

            foreach (ITaskAgent<T> workingAgent in workingAgents)
            {
                T task = workingAgent.Task;
                workingAgent.Reset();
                freeAgents.Push(workingAgent);
                ReferenceCache.Release(task);
            }

            workingAgents.Clear();
        }

        public TaskInfo[] GetAllTaskInfos()
        {
            List<TaskInfo> results = new List<TaskInfo>();
            foreach (ITaskAgent<T> workingAgent in workingAgents)
            {
                T workingTask = workingAgent.Task;
                results.Add(new TaskInfo(workingTask.SerialId, workingTask.Priority, workingTask.Done ? TaskStatus.Done : TaskStatus.Doing, workingTask.Description));
            }

            foreach (T waitingTask in waitingTasks)
            {
                results.Add(new TaskInfo(waitingTask.SerialId, waitingTask.Priority, TaskStatus.Todo, waitingTask.Description));
            }

            return results.ToArray();
        }

        private void ProcessRunningTasks(float elapseSeconds, float realElapseSeconds)
        {
            LinkedListNode<ITaskAgent<T>> current = workingAgents.First;
            while (current != null)
            {
                T task = current.Value.Task;
                if (!task.Done)
                {
                    current.Value.Update(elapseSeconds, realElapseSeconds);
                    current = current.Next;
                    continue;
                }

                LinkedListNode<ITaskAgent<T>> next = current.Next;
                current.Value.Reset();
                freeAgents.Push(current.Value);
                workingAgents.Remove(current);
                ReferenceCache.Release(task);
                current = next;
            }
        }

        private void ProcessWaitingTasks(float elapseSeconds, float realElapseSeconds)
        {
            LinkedListNode<T> current = waitingTasks.First;
            while (current != null && FreeAgentCount > 0)
            {
                ITaskAgent<T> agent = freeAgents.Pop();
                LinkedListNode<ITaskAgent<T>> agentNode = workingAgents.AddLast(agent);
                T task = current.Value;
                LinkedListNode<T> next = current.Next;
                StartTaskStatus status = agent.Start(task);
                if (status == StartTaskStatus.Done || status == StartTaskStatus.HasToWait || status == StartTaskStatus.UnknownError)
                {
                    agent.Reset();
                    freeAgents.Push(agent);
                    workingAgents.Remove(agentNode);
                }

                if (status == StartTaskStatus.Done || status == StartTaskStatus.CanResume || status == StartTaskStatus.UnknownError)
                {
                    waitingTasks.Remove(current);
                }

                if (status == StartTaskStatus.Done || status == StartTaskStatus.UnknownError)
                {
                    ReferenceCache.Release(task);
                }

                current = next;
            }
        }
    }
}