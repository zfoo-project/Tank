using Summer.Base;
using Summer.Base.TaskPool;
using Spring.Core;
using Spring.Util;
using UnityEngine;
using SpringComponent = Summer.Base.SpringComponent;

namespace Summer.Download
{
    /// <summary>
    /// 下载组件。
    /// </summary>
    [DisallowMultipleComponent]
    [AddComponentMenu("Summer/Download")]
    public sealed class DownloadComponent : SpringComponent
    {
        private const int OneMegaBytes = 1024 * 1024;

        [Autowired]
        private IDownloadManager downloadManager = null;

        [SerializeField]
        private int downloadAgentHelperCount = 3;

        [SerializeField]
        private float timeout = 30f;

        [SerializeField]
        private int flushSize = OneMegaBytes;


        /// <summary>
        /// 获取或设置下载超时时长，以秒为单位。
        /// </summary>
        public float Timeout
        {
            get
            {
                return downloadManager.Timeout;
            }
            set
            {
                downloadManager.Timeout = timeout = value;
            }
        }

        /// <summary>
        /// 获取或设置将缓冲区写入磁盘的临界大小，仅当开启断点续传时有效。
        /// </summary>
        public int FlushSize
        {
            get
            {
                return downloadManager.FlushSize;
            }
            set
            {
                downloadManager.FlushSize = flushSize = value;
            }
        }

        [BeforePostConstruct]
        private void Init()
        {
            downloadManager.FlushSize = flushSize;
            downloadManager.Timeout = timeout;
            
            for (int i = 0; i < downloadAgentHelperCount; i++)
            {
                AddDownloadAgentHelper(i);
            }
        }


        /// <summary>
        /// 获取所有下载任务的信息。
        /// </summary>
        /// <returns>所有下载任务的信息。</returns>
        public TaskInfo[] GetAllDownloadInfos()
        {
            return downloadManager.GetAllDownloadInfos();
        }

        /// <summary>
        /// 增加下载代理辅助器。
        /// </summary>
        /// <param name="index">下载代理辅助器索引。</param>
        private void AddDownloadAgentHelper(int index)
        {
            var downloadAgentObject = new GameObject();
            var downloadAgentMono = downloadAgentObject.AddComponent<DownloadAgentMono>();
            downloadAgentObject.name = StringUtils.Format("Download Agent Helper - {}", index.ToString());
            downloadAgentObject.transform.SetParent(transform);
            downloadAgentObject.transform.localScale = Vector3.one;
            downloadManager.AddDownloadAgentHelper(downloadAgentMono);
        }

    }
}
