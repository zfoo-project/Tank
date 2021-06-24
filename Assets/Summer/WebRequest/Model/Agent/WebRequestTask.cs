using System.Net.Http;
using Summer.Base.TaskPool;
using Spring.Collection.Reference;

namespace Summer.WebRequest.Model.Agent
{
    /// <summary>
    /// Web 请求任务。
    /// </summary>
    public sealed class WebRequestTask : TaskBase
    {
        private static int serial = 0;

        private WebRequestTaskStatus status;
        private string webRequestUri;
        private HttpMethod httpMethod;
        private byte[] postData;
        private float timeout;
        private object userData;

        public WebRequestTask()
        {
            status = WebRequestTaskStatus.Todo;
            webRequestUri = null;
            postData = null;
            timeout = 0f;
            userData = null;
        }

        /// <summary>
        /// 获取或设置 Web 请求任务的状态。
        /// </summary>
        public WebRequestTaskStatus Status
        {
            get { return status; }
            set { status = value; }
        }

        /// <summary>
        /// 获取要发送的远程地址。
        /// </summary>
        public string WebRequestUri
        {
            get { return webRequestUri; }
        }
        
        public HttpMethod HttpMethod
        {
            get { return httpMethod; }
        }

        /// <summary>
        /// 获取 Web 请求超时时长，以秒为单位。
        /// </summary>
        public float Timeout
        {
            get { return timeout; }
        }

        /// <summary>
        /// 获取用户自定义数据。
        /// </summary>
        public object UserData
        {
            get { return userData; }
        }

        /// <summary>
        /// 获取 Web 请求任务的描述。
        /// </summary>
        public override string Description
        {
            get { return webRequestUri; }
        }
        

        /// <summary>
        /// 创建 Web 请求任务。
        /// </summary>
        /// <param name="webRequestUri">要发送的远程地址。</param>
        /// <param name="postData">要发送的数据流。</param>
        /// <param name="priority">Web 请求任务的优先级。</param>
        /// <param name="timeout">下载超时时长，以秒为单位。</param>
        /// <param name="userData">用户自定义数据。</param>
        /// <returns>创建的 Web 请求任务。</returns>
        public static WebRequestTask Create(string webRequestUri, HttpMethod httpMethod, byte[] postData, int priority, float timeout, object userData)
        {
            WebRequestTask webRequestTask = ReferenceCache.Acquire<WebRequestTask>();
            webRequestTask.Initialize(++serial, priority);
            webRequestTask.webRequestUri = webRequestUri;
            webRequestTask.httpMethod = httpMethod;
            webRequestTask.postData = postData;
            webRequestTask.timeout = timeout;
            webRequestTask.userData = userData;
            return webRequestTask;
        }

        /// <summary>
        /// 清理 Web 请求任务。
        /// </summary>
        public override void Clear()
        {
            base.Clear();
            status = WebRequestTaskStatus.Todo;
            webRequestUri = null;
            postData = null;
            timeout = 0f;
            userData = null;
        }

        /// <summary>
        /// 获取要发送的数据流。
        /// </summary>
        public byte[] GetPostData()
        {
            return postData;
        }
    }
}