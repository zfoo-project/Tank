using System;
using System.Collections;
using Spring.Logger;
using Summer.Resource.Model.Callback;
using Summer.Scene;
using UnityEngine;
using UnityEngine.Networking;
using SceneManager = UnityEngine.SceneManagement.SceneManager;

namespace Summer.Resource.Model.Loader
{
    /// <summary>
    /// 默认资源辅助器。
    /// </summary>
    public class SimpleLoadResourceMono : MonoBehaviour
    {
        /// <summary>
        /// 直接从指定文件路径加载数据流。
        /// </summary>
        /// <param name="fileUri">文件路径。</param>
        /// <param name="loadBytesCallbacks">加载数据流回调函数集。</param>
        /// <param name="userData">用户自定义数据。</param>
        public void LoadBytes(string fileUri, LoadBytesCallbacks loadBytesCallbacks, object userData)
        {
            StartCoroutine(LoadBytesCo(fileUri, loadBytesCallbacks, userData));
        }

        /// <summary>
        /// 卸载场景。
        /// </summary>
        /// <param name="sceneAssetName">场景资源名称。</param>
        /// <param name="unloadSceneCallbacks">卸载场景回调函数集。</param>
        /// <param name="userData">用户自定义数据。</param>
        public void UnloadScene(string sceneAssetName, UnloadSceneCallbacks unloadSceneCallbacks, object userData)
        {
            if (gameObject.activeInHierarchy)
            {
                StartCoroutine(UnloadSceneCo(sceneAssetName, unloadSceneCallbacks, userData));
            }
            else
            {
                SceneManager.UnloadSceneAsync(SceneComponent.GetSceneName(sceneAssetName));
            }
        }

        /// <summary>
        /// 释放资源。
        /// </summary>
        /// <param name="objectToRelease">要释放的资源。</param>
        public void Release(object objectToRelease)
        {
            AssetBundle assetBundle = objectToRelease as AssetBundle;
            if (assetBundle != null)
            {
                assetBundle.Unload(true);
                return;
            }

            /* Unity 当前 Resources.UnloadAsset 在 iOS 设备上会导致一些诡异问题，先不用这部分
            SceneAsset sceneAsset = objectToRelease as SceneAsset;
            if (sceneAsset != null)
            {
                return;
            }

            Object unityObject = objectToRelease as Object;
            if (unityObject == null)
            {
                Log.Warning("Asset is invalid.");
                return;
            }

            if (unityObject is GameObject || unityObject is MonoBehaviour)
            {
                // UnloadAsset may only be used on individual assets and can not be used on GameObject's / Components or AssetBundles.
                return;
            }

            Resources.UnloadAsset(unityObject);
            */
        }


        private IEnumerator LoadBytesCo(string fileUri, LoadBytesCallbacks loadBytesCallbacks, object userData)
        {
            Log.Info("加载资源[{}]，数据[{}]", fileUri, userData);

            bool isError = false;
            byte[] bytes = null;
            string errorMessage = null;
            DateTime startTime = DateTime.UtcNow;

            UnityWebRequest unityWebRequest = UnityWebRequest.Get(fileUri);
            yield return unityWebRequest.SendWebRequest();

            isError = unityWebRequest.isNetworkError || unityWebRequest.isHttpError;
            bytes = unityWebRequest.downloadHandler.data;
            errorMessage = isError ? unityWebRequest.error : null;
            unityWebRequest.Dispose();

            if (!isError)
            {
                float elapseSeconds = (float) (DateTime.UtcNow - startTime).TotalSeconds;
                loadBytesCallbacks.LoadBytesSuccessCallback(fileUri, bytes, elapseSeconds, userData);
            }
            else if (loadBytesCallbacks.LoadBytesFailureCallback != null)
            {
                loadBytesCallbacks.LoadBytesFailureCallback(fileUri, errorMessage, userData);
            }
        }

        private IEnumerator UnloadSceneCo(string sceneAssetName, UnloadSceneCallbacks unloadSceneCallbacks,
            object userData)
        {
            AsyncOperation asyncOperation = SceneManager.UnloadSceneAsync(SceneComponent.GetSceneName(sceneAssetName));
            if (asyncOperation == null)
            {
                yield break;
            }

            yield return asyncOperation;

            if (asyncOperation.allowSceneActivation)
            {
                if (unloadSceneCallbacks.UnloadSceneSuccessCallback != null)
                {
                    unloadSceneCallbacks.UnloadSceneSuccessCallback(sceneAssetName, userData);
                }
            }
            else
            {
                if (unloadSceneCallbacks.UnloadSceneFailureCallback != null)
                {
                    unloadSceneCallbacks.UnloadSceneFailureCallback(sceneAssetName, userData);
                }
            }
        }
    }
}