using Summer.Base.Model;
using Summer.Base.TaskPool;
using Summer.WebRequest.Model.Eve;
using Spring.Collection.Reference;
using Spring.Event;

namespace Summer.WebRequest.Model.Agent
{
    /// <summary>
    /// Web 请求代理。
    /// </summary>
    public sealed class WebRequestAgent : ITaskAgent<WebRequestTask>
    {
        private readonly WebRequestAgentMono webRequestAgentMono;
        private WebRequestTask task;
        private float waitTime;


        /// <summary>
        /// 初始化 Web 请求代理的新实例。
        /// </summary>
        /// <param name="webRequestAgentMono">Web 请求代理辅助器。</param>
        public WebRequestAgent(WebRequestAgentMono webRequestAgentMono)
        {
            if (webRequestAgentMono == null)
            {
                throw new GameFrameworkException("Web request agent helper is invalid.");
            }

            this.webRequestAgentMono = webRequestAgentMono;
            task = null;
            waitTime = 0f;
        }

        /// <summary>
        /// 获取 Web 请求任务。
        /// </summary>
        public WebRequestTask Task
        {
            get { return task; }
        }

        /// <summary>
        /// 获取已经等待时间。
        /// </summary>
        public float WaitTime
        {
            get { return waitTime; }
        }

        /// <summary>
        /// 初始化 Web 请求代理。
        /// </summary>
        public void Initialize()
        {
            webRequestAgentMono.SetWebRequestAgent(this);
        }

        /// <summary>
        /// Web 请求代理轮询。
        /// </summary>
        /// <param name="elapseSeconds">逻辑流逝时间，以秒为单位。</param>
        /// <param name="realElapseSeconds">真实流逝时间，以秒为单位。</param>
        public void Update(float elapseSeconds, float realElapseSeconds)
        {
            if (task.Status == WebRequestTaskStatus.Doing)
            {
                waitTime += realElapseSeconds;
                if (waitTime >= task.Timeout)
                {
                    OnWebRequestAgentError("Timeout");
                }
            }
        }

        /// <summary>
        /// 关闭并清理 Web 请求代理。
        /// </summary>
        public void Shutdown()
        {
            Reset();
        }

        /// <summary>
        /// 开始处理 Web 请求任务。
        /// </summary>
        /// <param name="task">要处理的 Web 请求任务。</param>
        /// <returns>开始处理任务的状态。</returns>
        public StartTaskStatus Start(WebRequestTask task)
        {
            if (task == null)
            {
                throw new GameFrameworkException("Task is invalid.");
            }

            this.task = task;
            this.task.Status = WebRequestTaskStatus.Doing;

            var eve = WebRequestStartEvent.ValueOf(this.task.SerialId, this.task.WebRequestUri, this.task.UserData);
            EventBus.SyncSubmit(eve);
            ReferenceCache.Release(eve);

            byte[] postData = this.task.GetPostData();
            webRequestAgentMono.Request(this.task.WebRequestUri, this.task.HttpMethod, postData, this.task.UserData);
            waitTime = 0f;
            return StartTaskStatus.CanResume;
        }

        /// <summary>
        /// 重置 Web 请求代理。
        /// </summary>
        public void Reset()
        {
            webRequestAgentMono.Reset();
            task = null;
            waitTime = 0f;
        }

        public void OnWebRequestAgentComplete(byte[] webResponseBytes)
        {
            webRequestAgentMono.Reset();
            task.Status = WebRequestTaskStatus.Done;

            var eve = WebRequestSuccessEvent.ValueOf(task.SerialId, task.WebRequestUri, webResponseBytes, task.UserData);
            EventBus.SyncSubmit(eve);
            ReferenceCache.Release(eve);

            task.Done = true;
        }

        public void OnWebRequestAgentError(string error)
        {
            webRequestAgentMono.Reset();
            task.Status = WebRequestTaskStatus.Error;

            var eve = WebRequestFailureEvent.ValueOf(task.SerialId, task.WebRequestUri, error, task.UserData);
            EventBus.SyncSubmit(eve);
            ReferenceCache.Release(eve);

            task.Done = true;
        }
    }
}