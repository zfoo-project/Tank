using System;
using System.Net.Http;
using Spring.Util;
using UnityEngine;
using UnityEngine.Networking;

namespace Summer.WebRequest.Model.Agent
{
    /// <summary>
    /// 使用 UnityWebRequest 实现的 Web 请求代理辅助器。
    /// </summary>
    public class WebRequestAgentMono : MonoBehaviour, IDisposable
    {
        private UnityWebRequest unityWebRequest;
        private bool disposed;
        private WebRequestAgent webRequestAgent;

        public void SetWebRequestAgent(WebRequestAgent webRequestAgent)
        {
            this.webRequestAgent = webRequestAgent;
        }


        /// <summary>
        /// 通过 Web 请求代理辅助器发送请求。
        /// </summary>
        /// <param name="webRequestUri">要发送的远程地址。</param>
        /// <param name="postData">要发送的数据流。</param>
        /// <param name="userData">用户自定义数据。</param>
        public void Request(string webRequestUri, HttpMethod httpMethod, byte[] postData, object userData)
        {
            if (HttpMethod.Get == httpMethod)
            {
                unityWebRequest = UnityWebRequest.Get(webRequestUri);
            }
            else if (HttpMethod.Post == httpMethod)
            {
                unityWebRequest = UnityWebRequest.Post(webRequestUri, ConverterUtils.GetString(postData));
            }
            else if (HttpMethod.Put == httpMethod)
            {
                unityWebRequest = UnityWebRequest.Put(webRequestUri, postData);
            }
            else
            {
                throw new Exception(StringUtils.Format("请求[{}]不支持[{}]操作", webRequestUri, httpMethod.Method));
            }

            unityWebRequest.SendWebRequest();
        }

        /// <summary>
        /// 重置 Web 请求代理辅助器。
        /// </summary>
        public void Reset()
        {
            if (unityWebRequest != null)
            {
                unityWebRequest.Dispose();
                unityWebRequest = null;
            }
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
                webRequestAgent.OnWebRequestAgentError(unityWebRequest.error);
            }
            else if (unityWebRequest.downloadHandler.isDone)
            {
                webRequestAgent.OnWebRequestAgentComplete(unityWebRequest.downloadHandler.data);
            }
        }
    }
}