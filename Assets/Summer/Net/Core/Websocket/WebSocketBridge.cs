#if UNITY_WEBGL && !UNITY_EDITOR
using System;
using System.Runtime.InteropServices;
using AOT;
using Spring.Logger;

namespace Summer.Net.Core.Websocket
{
    /// <summary>
    /// Class providing static access methods to work with JSLIB WebSocket
    /// </summary>
    internal static class WebSocketBridge
    {
        /* Delegates */
        public delegate void OnOpenCallback();

        public delegate void OnMessageCallback(IntPtr msgPtr, int msgSize);

        public delegate void OnErrorCallback();

        public delegate void OnCloseCallback();

        /* WebSocket JSLIB functions */
        [DllImport("__Internal")]
        public static extern void WebSocketConnect(string url);

        [DllImport("__Internal")]
        public static extern void WebSocketClose();

        [DllImport("__Internal")]
        public static extern void WebSocketSend(byte[] dataPtr, int dataLength);


        [DllImport("__Internal")]
        public static extern void WebSocketSetOnOpen(OnOpenCallback callback);

        [DllImport("__Internal")]
        public static extern void WebSocketSetOnMessage(OnMessageCallback callback);

        [DllImport("__Internal")]
        public static extern void WebSocketSetOnError(OnErrorCallback callback);

        [DllImport("__Internal")]
        public static extern void WebSocketSetOnClose(OnCloseCallback callback);

        public static WebsocketClient websocketClient;
        /* If callbacks was initialized and set */
        public static bool initialized = false;

        /* Initialize WebSocket callbacks to JSLIB */
        public static void Initialize()
        {
            WebSocketSetOnOpen(DelegateOnOpenEvent);
            WebSocketSetOnMessage(DelegateOnMessageEvent);
            WebSocketSetOnError(DelegateOnErrorEvent);
            WebSocketSetOnClose(DelegateOnCloseEvent);

            initialized = true;
        }


        [MonoPInvokeCallback(typeof(OnOpenCallback))]
        public static void DelegateOnOpenEvent()
        {
            websocketClient.HandleOnOpen();
        }

        [MonoPInvokeCallback(typeof(OnMessageCallback))]
        public static void DelegateOnMessageEvent(IntPtr msgPtr, int msgSize)
        {
            try
            {
                var bytes = new byte[msgSize];
                Marshal.Copy(msgPtr, bytes, 0, msgSize);
                websocketClient.HandleOnMessage(bytes);
            }
            catch (Exception e)
            {
                Log.Info(e);
                throw e;
            }
        }


        [MonoPInvokeCallback(typeof(OnErrorCallback))]
        public static void DelegateOnErrorEvent()
        {
            websocketClient.HandleOnError();
        }

        [MonoPInvokeCallback(typeof(OnCloseCallback))]
        public static void DelegateOnCloseEvent()
        {
            websocketClient.HandleOnClose();
        }

    }
}
#endif