using System;
using Summer.Download.Model.Eve;
using Spring.Collection.Reference;
using Spring.Logger;
using Spring.Util;
using UnityEngine.Networking;

namespace Summer.Download
{
    /// <summary>
    /// 使用 UnityWebRequest 实现的下载代理辅助器。
    /// </summary>
    public class DownloadAgentMono : DownloadAgentHelperBase, IDisposable
    {
        private const int CachedBytesLength = 0x1000;
        private readonly byte[] cachedBytes = new byte[CachedBytesLength];

        private UnityWebRequest unityWebRequest;
        private bool disposed;

        private EventHandler<DownloadAgentHelperUpdateBytesEventArgs> m_DownloadAgentHelperUpdateBytesEventHandler = null;
        private EventHandler<DownloadAgentHelperUpdateLengthEventArgs> m_DownloadAgentHelperUpdateLengthEventHandler = null;
        private EventHandler<DownloadAgentHelperCompleteEventArgs> m_DownloadAgentHelperCompleteEventHandler = null;
        private EventHandler<DownloadAgentHelperErrorEventArgs> m_DownloadAgentHelperErrorEventHandler = null;

        /// <summary>
        /// 下载代理辅助器更新数据流事件。
        /// </summary>
        public override event EventHandler<DownloadAgentHelperUpdateBytesEventArgs> DownloadAgentHelperUpdateBytes
        {
            add { m_DownloadAgentHelperUpdateBytesEventHandler += value; }
            remove { m_DownloadAgentHelperUpdateBytesEventHandler -= value; }
        }

        /// <summary>
        /// 下载代理辅助器更新数据大小事件。
        /// </summary>
        public override event EventHandler<DownloadAgentHelperUpdateLengthEventArgs> DownloadAgentHelperUpdateLength
        {
            add { m_DownloadAgentHelperUpdateLengthEventHandler += value; }
            remove { m_DownloadAgentHelperUpdateLengthEventHandler -= value; }
        }

        /// <summary>
        /// 下载代理辅助器完成事件。
        /// </summary>
        public override event EventHandler<DownloadAgentHelperCompleteEventArgs> DownloadAgentHelperComplete
        {
            add { m_DownloadAgentHelperCompleteEventHandler += value; }
            remove { m_DownloadAgentHelperCompleteEventHandler -= value; }
        }

        /// <summary>
        /// 下载代理辅助器错误事件。
        /// </summary>
        public override event EventHandler<DownloadAgentHelperErrorEventArgs> DownloadAgentHelperError
        {
            add { m_DownloadAgentHelperErrorEventHandler += value; }
            remove { m_DownloadAgentHelperErrorEventHandler -= value; }
        }

        /// <summary>
        /// 通过下载代理辅助器下载指定地址的数据。
        /// </summary>
        /// <param name="downloadUri">下载地址。</param>
        /// <param name="userData">用户自定义数据。</param>
        public override void Download(string downloadUri, object userData)
        {
            if (m_DownloadAgentHelperUpdateBytesEventHandler == null || m_DownloadAgentHelperUpdateLengthEventHandler == null || m_DownloadAgentHelperCompleteEventHandler == null || m_DownloadAgentHelperErrorEventHandler == null)
            {
                Log.Fatal("Download agent helper handler is invalid.");
                return;
            }

            unityWebRequest = new UnityWebRequest(downloadUri);
            unityWebRequest.downloadHandler = new DownloadHandler(this);
            unityWebRequest.SendWebRequest();
        }

        /// <summary>
        /// 通过下载代理辅助器下载指定地址的数据。
        /// </summary>
        /// <param name="downloadUri">下载地址。</param>
        /// <param name="fromPosition">下载数据起始位置。</param>
        /// <param name="userData">用户自定义数据。</param>
        public override void Download(string downloadUri, long fromPosition, object userData)
        {
            if (m_DownloadAgentHelperUpdateBytesEventHandler == null || m_DownloadAgentHelperUpdateLengthEventHandler == null || m_DownloadAgentHelperCompleteEventHandler == null || m_DownloadAgentHelperErrorEventHandler == null)
            {
                Log.Fatal("Download agent helper handler is invalid.");
                return;
            }

            unityWebRequest = new UnityWebRequest(downloadUri);
            unityWebRequest.SetRequestHeader("Range", StringUtils.Format("bytes={}-", fromPosition.ToString()));
            unityWebRequest.downloadHandler = new DownloadHandler(this);
            unityWebRequest.SendWebRequest();
        }

        /// <summary>
        /// 通过下载代理辅助器下载指定地址的数据。
        /// </summary>
        /// <param name="downloadUri">下载地址。</param>
        /// <param name="fromPosition">下载数据起始位置。</param>
        /// <param name="toPosition">下载数据结束位置。</param>
        /// <param name="userData">用户自定义数据。</param>
        public override void Download(string downloadUri, long fromPosition, long toPosition, object userData)
        {
            if (m_DownloadAgentHelperUpdateBytesEventHandler == null || m_DownloadAgentHelperUpdateLengthEventHandler == null || m_DownloadAgentHelperCompleteEventHandler == null || m_DownloadAgentHelperErrorEventHandler == null)
            {
                Log.Fatal("Download agent helper handler is invalid.");
                return;
            }

            unityWebRequest = new UnityWebRequest(downloadUri);
            unityWebRequest.SetRequestHeader("Range", StringUtils.Format("bytes={}-{}", fromPosition.ToString(), toPosition.ToString()));
            unityWebRequest.downloadHandler = new DownloadHandler(this);
            unityWebRequest.SendWebRequest();
        }

        /// <summary>
        /// 重置下载代理辅助器。
        /// </summary>
        public override void Reset()
        {
            if (unityWebRequest != null)
            {
                unityWebRequest.Abort();
                unityWebRequest.Dispose();
                unityWebRequest = null;
            }

            Array.Clear(cachedBytes, 0, CachedBytesLength);
        }

        /// <summary>
        /// 释放资源。
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// 释放资源。
        /// </summary>
        /// <param name="disposing">释放资源标记。</param>
        protected virtual void Dispose(bool disposing)
        {
            if (disposed)
            {
                return;
            }

            if (disposing)
            {
                if (unityWebRequest != null)
                {
                    unityWebRequest.Dispose();
                    unityWebRequest = null;
                }
            }

            disposed = true;
        }

        private void Update()
        {
            if (unityWebRequest == null)
            {
                return;
            }

            if (!unityWebRequest.isDone)
            {
                return;
            }

            bool isError = unityWebRequest.isNetworkError || unityWebRequest.isHttpError;
            if (isError)
            {
                DownloadAgentHelperErrorEventArgs downloadAgentHelperErrorEventArgs = DownloadAgentHelperErrorEventArgs.Create(unityWebRequest.responseCode == RangeNotSatisfiableErrorCode, unityWebRequest.error);
                m_DownloadAgentHelperErrorEventHandler(this, downloadAgentHelperErrorEventArgs);
                ReferenceCache.Release(downloadAgentHelperErrorEventArgs);
            }
            else
            {
                DownloadAgentHelperCompleteEventArgs downloadAgentHelperCompleteEventArgs = DownloadAgentHelperCompleteEventArgs.Create((int) unityWebRequest.downloadedBytes);
                m_DownloadAgentHelperCompleteEventHandler(this, downloadAgentHelperCompleteEventArgs);
                ReferenceCache.Release(downloadAgentHelperCompleteEventArgs);
            }
        }

        private class DownloadHandler : DownloadHandlerScript
        {
            private readonly DownloadAgentMono m_Owner;

            public DownloadHandler(DownloadAgentMono owner)
                : base(owner.cachedBytes)
            {
                m_Owner = owner;
            }

            protected override bool ReceiveData(byte[] data, int dataLength)
            {
                if (m_Owner != null && m_Owner.unityWebRequest != null && dataLength > 0)
                {
                    DownloadAgentHelperUpdateBytesEventArgs downloadAgentHelperUpdateBytesEventArgs = DownloadAgentHelperUpdateBytesEventArgs.Create(data, 0, dataLength);
                    m_Owner.m_DownloadAgentHelperUpdateBytesEventHandler(this, downloadAgentHelperUpdateBytesEventArgs);
                    ReferenceCache.Release(downloadAgentHelperUpdateBytesEventArgs);

                    DownloadAgentHelperUpdateLengthEventArgs downloadAgentHelperUpdateLengthEventArgs = DownloadAgentHelperUpdateLengthEventArgs.Create(dataLength);
                    m_Owner.m_DownloadAgentHelperUpdateLengthEventHandler(this, downloadAgentHelperUpdateLengthEventArgs);
                    ReferenceCache.Release(downloadAgentHelperUpdateLengthEventArgs);
                }

                return base.ReceiveData(data, dataLength);
            }
        }
    }
}