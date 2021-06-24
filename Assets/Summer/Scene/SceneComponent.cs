using System;
using System.Collections.Generic;
using Summer.Base;
using Summer.Scene.Model;
using Spring.Core;
using Spring.Event;
using Spring.Logger;
using UnityEngine;
using SpringComponent = Summer.Base.SpringComponent;

namespace Summer.Scene
{
    /// <summary>
    /// 场景组件。
    /// </summary>
    [DisallowMultipleComponent]
    [AddComponentMenu("Summer/Scene")]
    public sealed class SceneComponent : SpringComponent
    {
        [Autowired]
        private ISceneManager sceneManager;


        private readonly SortedDictionary<string, int> sceneOrder = new SortedDictionary<string, int>(StringComparer.Ordinal);

        private Camera mainCamera;
        private UnityEngine.SceneManagement.Scene gameFrameworkScene = default(UnityEngine.SceneManagement.Scene);


        /// <summary>
        /// 获取当前场景主摄像机。
        /// </summary>
        public Camera MainCamera
        {
            get { return mainCamera; }
        }

        protected override void Awake()
        {
            gameFrameworkScene = UnityEngine.SceneManagement.SceneManager.GetSceneAt(BaseComponent.GameFrameworkSceneId);
            if (!gameFrameworkScene.IsValid())
            {
                Log.Fatal("Game Framework scene is invalid.");
                return;
            }
            
            base.Awake();
        }


        /// <summary>
        /// 获取场景名称。
        /// </summary>
        /// <param name="sceneAssetName">场景资源名称。</param>
        /// <returns>场景名称。</returns>
        public static string GetSceneName(string sceneAssetName)
        {
            if (string.IsNullOrEmpty(sceneAssetName))
            {
                Log.Error("Scene asset name is invalid.");
                return null;
            }

            int sceneNamePosition = sceneAssetName.LastIndexOf('/');
            if (sceneNamePosition + 1 >= sceneAssetName.Length)
            {
                Log.Error("Scene asset name '{}' is invalid.", sceneAssetName);
                return null;
            }

            string sceneName = sceneAssetName.Substring(sceneNamePosition + 1);
            sceneNamePosition = sceneName.LastIndexOf(".unity");
            if (sceneNamePosition > 0)
            {
                sceneName = sceneName.Substring(0, sceneNamePosition);
            }

            return sceneName;
        }



        /// <summary>
        /// 刷新当前场景主摄像机。
        /// </summary>
        public void RefreshMainCamera()
        {
            mainCamera = Camera.main;
        }

        private void RefreshSceneOrder()
        {
            if (sceneOrder.Count > 0)
            {
                string maxSceneName = null;
                int maxSceneOrder = 0;
                foreach (KeyValuePair<string, int> sceneOrder in sceneOrder)
                {
                    if (sceneManager.SceneIsLoading(sceneOrder.Key))
                    {
                        continue;
                    }

                    if (maxSceneName == null)
                    {
                        maxSceneName = sceneOrder.Key;
                        maxSceneOrder = sceneOrder.Value;
                        continue;
                    }

                    if (sceneOrder.Value > maxSceneOrder)
                    {
                        maxSceneName = sceneOrder.Key;
                        maxSceneOrder = sceneOrder.Value;
                    }
                }

                if (maxSceneName == null)
                {
                    SetActiveScene(gameFrameworkScene);
                    return;
                }

                UnityEngine.SceneManagement.Scene scene = UnityEngine.SceneManagement.SceneManager.GetSceneByName(GetSceneName(maxSceneName));
                if (!scene.IsValid())
                {
                    Log.Error("Active scene '{}' is invalid.", maxSceneName);
                    return;
                }

                SetActiveScene(scene);
            }
            else
            {
                SetActiveScene(gameFrameworkScene);
            }
        }

        private void SetActiveScene(UnityEngine.SceneManagement.Scene activeScene)
        {
            UnityEngine.SceneManagement.Scene lastActiveScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
            if (lastActiveScene != activeScene)
            {
                UnityEngine.SceneManagement.SceneManager.SetActiveScene(activeScene);
            }

            RefreshMainCamera();
        }

        [EventReceiver]
        private void OnLoadSceneSuccessEvent(LoadSceneSuccessEvent eve)
        {
            if (!sceneOrder.ContainsKey(eve.sceneAssetName))
            {
                sceneOrder.Add(eve.sceneAssetName, 0);
            }

            RefreshSceneOrder();
        }

        [EventReceiver]
        private void OnUnloadSceneSuccessEvent(UnloadSceneSuccessEvent eve)
        {
            sceneOrder.Remove(eve.sceneAssetName);
            RefreshSceneOrder();
        }
    }
}