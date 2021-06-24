using Summer.Base;
using Summer.WebRequest.Model.Agent;
using Spring.Core;
using Spring.Util;
using UnityEngine;
using SpringComponent = Summer.Base.SpringComponent;

namespace Summer.WebRequest
{
    /// <summary>
    /// Web 请求组件。
    /// </summary>
    [DisallowMultipleComponent]
    [AddComponentMenu("Summer/Web Request")]
    public sealed class WebRequestComponent : SpringComponent
    {

        [Autowired]
        private IWebRequestManager webRequestManager;


        [SerializeField]
        private int webRequestAgentHelperCount = 1;

        [SerializeField]
        private float timeout = 30f;


        /// <summary>
        /// 获取或设置 Web 请求超时时长，以秒为单位。
        /// </summary>
        public float Timeout
        {
            get
            {
                return webRequestManager.Timeout;
            }
            set
            {
                webRequestManager.Timeout = timeout = value;
            }
        }

        [BeforePostConstruct]
        private void Init()
        {
            webRequestManager.Timeout = timeout;

            for (int i = 0; i < webRequestAgentHelperCount; i++)
            {
                AddWebRequestAgentHelper(i);
            }
        }


        /// <summary>
        /// 增加 Web 请求代理辅助器。
        /// </summary>
        /// <param name="index">Web 请求代理辅助器索引。</param>
        private void AddWebRequestAgentHelper(int index)
        {
            var agentObject = new GameObject();
            var webAgentMono = agentObject.AddComponent<WebRequestAgentMono>();
            agentObject.name = StringUtils.Format("Web Request Agent Helper - {}", index.ToString());
            agentObject.transform.SetParent(transform);
            agentObject.transform.localScale = Vector3.one;
            webRequestManager.AddWebRequestAgentMono(webAgentMono);
        }

    }
}
