using System.Net.Http;
using Summer.Base.Model;
using Summer.Base.TaskPool;
using Summer.WebRequest.Model.Agent;
using Spring.Core;

namespace Summer.WebRequest
{
    /// <summary>
    /// Web 请求管理器。
    /// </summary>
    [Bean]
    public sealed class WebRequestManager : AbstractManager, IWebRequestManager
    {
        /// <summary>
        /// 默认 Web 请求任务优先级。
        /// </summary>
        public const int DefaultPriority = 0;

        private readonly TaskPool<WebRequestTask> m_TaskPool;
        private float m_Timeout;

        /// <summary>
        /// 初始化 Web 请求管理器的新实例。
        /// </summary>
        public WebRequestManager()
        {
            m_TaskPool = new TaskPool<WebRequestTask>();
            m_Timeout = 30f;
        }

        /// <summary>
        /// 获取 Web 请求代理总数量。
        /// </summary>
        public int TotalAgentCount
        {
            get { return m_TaskPool.TotalAgentCount; }
        }

        /// <summary>
        /// 获取可用 Web 请求代理数量。
        /// </summary>
        public int FreeAgentCount
        {
            get { return m_TaskPool.FreeAgentCount; }
        }

        /// <summary>
        /// 获取工作中 Web 请求代理数量。
        /// </summary>
        public int WorkingAgentCount
        {
            get { return m_TaskPool.WorkingAgentCount; }
        }

        /// <summary>
        /// 获取等待 Web 请求数量。
        /// </summary>
        public int WaitingTaskCount
        {
            get { return m_TaskPool.WaitingTaskCount; }
        }

        /// <summary>
        /// 获取或设置 Web 请求超时时长，以秒为单位。
        /// </summary>
        public float Timeout
        {
            get { return m_Timeout; }
            set { m_Timeout = value; }
        }


        /// <summary>
        /// Web 请求管理器轮询。
        /// </summary>
        /// <param name="elapseSeconds">逻辑流逝时间，以秒为单位。</param>
        /// <param name="realElapseSeconds">真实流逝时间，以秒为单位。</param>
        public override void Update(float elapseSeconds, float realElapseSeconds)
        {
            m_TaskPool.Update(elapseSeconds, realElapseSeconds);
        }

        /// <summary>
        /// 关闭并清理 Web 请求管理器。
        /// </summary>
        public override void Shutdown()
        {
            m_TaskPool.Shutdown();
        }

        /// <summary>
        /// 增加 Web 请求代理辅助器。
        /// </summary>
        /// <param name="webRequestAgentMono">要增加的 Web 请求代理辅助器。</param>
        public void AddWebRequestAgentMono(WebRequestAgentMono webRequestAgentMono)
        {
            WebRequestAgent agent = new WebRequestAgent(webRequestAgentMono);
            m_TaskPool.AddAgent(agent);
        }

        /// <summary>
        /// 增加 Web 请求任务。
        /// </summary>
        /// <param name="webRequestUri">Web 请求地址。</param>
        /// <returns>新增 Web 请求任务的序列编号。</returns>
        public int AddGetWebRequest(string webRequestUri)
        {
            return AddWebRequest(webRequestUri, HttpMethod.Get, null, DefaultPriority, null);
        }

        /// <summary>
        /// 增加 Web 请求任务。
        /// </summary>
        /// <param name="webRequestUri">Web 请求地址。</param>
        /// <param name="postData">要发送的数据流。</param>
        /// <returns>新增 Web 请求任务的序列编号。</returns>
        public int AddPostWebRequest(string webRequestUri, byte[] postData)
        {
            return AddWebRequest(webRequestUri, HttpMethod.Post, postData, DefaultPriority, null);
        }

        /// <summary>
        /// 增加 Web 请求任务。
        /// </summary>
        /// <param name="webRequestUri">Web 请求地址。</param>
        /// <param name="priority">Web 请求任务的优先级。</param>
        /// <returns>新增 Web 请求任务的序列编号。</returns>
        public int AddGetWebRequest(string webRequestUri, int priority)
        {
            return AddWebRequest(webRequestUri, HttpMethod.Get, null, priority, null);
        }

        /// <summary>
        /// 增加 Web 请求任务。
        /// </summary>
        /// <param name="webRequestUri">Web 请求地址。</param>
        /// <param name="userData">用户自定义数据。</param>
        /// <returns>新增 Web 请求任务的序列编号。</returns>
        public int AddGetWebRequest(string webRequestUri, object userData)
        {
            return AddWebRequest(webRequestUri, HttpMethod.Get, null, DefaultPriority, userData);
        }

        /// <summary>
        /// 增加 Web 请求任务。
        /// </summary>
        /// <param name="webRequestUri">Web 请求地址。</param>
        /// <param name="postData">要发送的数据流。</param>
        /// <param name="priority">Web 请求任务的优先级。</param>
        /// <returns>新增 Web 请求任务的序列编号。</returns>
        public int AddPostWebRequest(string webRequestUri, byte[] postData, int priority)
        {
            return AddWebRequest(webRequestUri, HttpMethod.Post, postData, priority, null);
        }

        /// <summary>
        /// 增加 Web 请求任务。
        /// </summary>
        /// <param name="webRequestUri">Web 请求地址。</param>
        /// <param name="postData">要发送的数据流。</param>
        /// <param name="userData">用户自定义数据。</param>
        /// <returns>新增 Web 请求任务的序列编号。</returns>
        public int AddPostWebRequest(string webRequestUri, byte[] postData, object userData)
        {
            return AddWebRequest(webRequestUri, HttpMethod.Post, postData, DefaultPriority, userData);
        }

        /// <summary>
        /// 增加 Web 请求任务。
        /// </summary>
        /// <param name="webRequestUri">Web 请求地址。</param>
        /// <param name="priority">Web 请求任务的优先级。</param>
        /// <param name="userData">用户自定义数据。</param>
        /// <returns>新增 Web 请求任务的序列编号。</returns>
        public int AddGetWebRequest(string webRequestUri, int priority, object userData)
        {
            return AddWebRequest(webRequestUri, HttpMethod.Get, null, priority, userData);
        }

        /// <summary>
        /// 增加 Web 请求任务。
        /// </summary>
        /// <param name="webRequestUri">Web 请求地址。</param>
        /// <param name="postData">要发送的数据流。</param>
        /// <param name="priority">Web 请求任务的优先级。</param>
        /// <param name="userData">用户自定义数据。</param>
        /// <returns>新增 Web 请求任务的序列编号。</returns>
        public int AddWebRequest(string webRequestUri, HttpMethod httpMethod, byte[] postData, int priority, object userData)
        {
            if (string.IsNullOrEmpty(webRequestUri))
            {
                throw new GameFrameworkException("Web request uri is invalid.");
            }

            if (TotalAgentCount <= 0)
            {
                throw new GameFrameworkException("You must add web request agent first.");
            }

            WebRequestTask webRequestTask = WebRequestTask.Create(webRequestUri, httpMethod, postData, priority, m_Timeout, userData);
            m_TaskPool.AddTask(webRequestTask);
            return webRequestTask.SerialId;
        }

        /// <summary>
        /// 移除 Web 请求任务。
        /// </summary>
        /// <param name="serialId">要移除 Web 请求任务的序列编号。</param>
        /// <returns>是否移除 Web 请求任务成功。</returns>
        public bool RemoveWebRequest(int serialId)
        {
            return m_TaskPool.RemoveTask(serialId);
        }

        /// <summary>
        /// 移除所有 Web 请求任务。
        /// </summary>
        public void RemoveAllWebRequests()
        {
            m_TaskPool.RemoveAllTasks();
        }

        /// <summary>
        /// 获取所有 Web 请求任务的信息。
        /// </summary>
        /// <returns>所有 Web 请求任务的信息。</returns>
        public TaskInfo[] GetAllWebRequestInfos()
        {
            return m_TaskPool.GetAllTaskInfos();
        }
    }
}