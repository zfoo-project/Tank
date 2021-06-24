using System;
using System.Collections.Generic;
using Summer.Base.Model;
using Summer.Resource;
using Summer.Resource.Model.Callback;
using Summer.Resource.Model.Constant;
using Summer.Scene.Model;
using Spring.Collection.Reference;
using Spring.Core;
using Spring.Event;
using Spring.Logger;
using Spring.Util;

namespace Summer.Scene
{
    /// <summary>
    /// 场景管理器。
    /// </summary>
    [Bean]
    public sealed class SceneManager : AbstractManager, ISceneManager
    {
        [Autowired]
        private IResourceManager resourceManager;

        private readonly List<string> loadedSceneAssetNames = new List<string>();
        private readonly List<string> loadingSceneAssetNames = new List<string>();
        private readonly List<string> unloadingSceneAssetNames = new List<string>();
        private readonly LoadSceneCallbacks loadSceneCallbacks;
        private readonly UnloadSceneCallbacks unloadSceneCallbacks;


        /// <summary>
        /// 初始化场景管理器的新实例。
        /// </summary>
        public SceneManager()
        {
            loadSceneCallbacks = new LoadSceneCallbacks(LoadSceneSuccessCallback, LoadSceneFailureCallback, LoadSceneUpdateCallback, LoadSceneDependencyAssetCallback);
            unloadSceneCallbacks = new UnloadSceneCallbacks(UnloadSceneSuccessCallback, UnloadSceneFailureCallback);
        }

        /// <summary>
        /// 获取游戏框架模块优先级。
        /// </summary>
        /// <remarks>优先级较高的模块会优先轮询，并且关闭操作会后进行。</remarks>
        public override int Priority
        {
            get { return 60; }
        }


        /// <summary>
        /// 场景管理器轮询。
        /// </summary>
        /// <param name="elapseSeconds">逻辑流逝时间，以秒为单位。</param>
        /// <param name="realElapseSeconds">真实流逝时间，以秒为单位。</param>
        public override void Update(float elapseSeconds, float realElapseSeconds)
        {
        }

        /// <summary>
        /// 关闭并清理场景管理器。
        /// </summary>
        public override void Shutdown()
        {
            foreach (string loadedSceneAssetName in loadedSceneAssetNames)
            {
                if (SceneIsUnloading(loadedSceneAssetName))
                {
                    continue;
                }

                UnloadScene(loadedSceneAssetName);
            }

            loadedSceneAssetNames.Clear();
            loadingSceneAssetNames.Clear();
            unloadingSceneAssetNames.Clear();
        }


        /// <summary>
        /// 获取场景是否已加载。
        /// </summary>
        /// <param name="sceneAssetName">场景资源名称。</param>
        /// <returns>场景是否已加载。</returns>
        public bool SceneIsLoaded(string sceneAssetName)
        {
            if (string.IsNullOrEmpty(sceneAssetName))
            {
                throw new GameFrameworkException("Scene asset name is invalid.");
            }

            return loadedSceneAssetNames.Contains(sceneAssetName);
        }

        /// <summary>
        /// 获取已加载场景的资源名称。
        /// </summary>
        /// <returns>已加载场景的资源名称。</returns>
        public string[] GetLoadedSceneAssetNames()
        {
            return loadedSceneAssetNames.ToArray();
        }

        /// <summary>
        /// 获取已加载场景的资源名称。
        /// </summary>
        /// <param name="results">已加载场景的资源名称。</param>
        public void GetLoadedSceneAssetNames(List<string> results)
        {
            if (results == null)
            {
                throw new GameFrameworkException("Results is invalid.");
            }

            results.Clear();
            results.AddRange(loadedSceneAssetNames);
        }

        /// <summary>
        /// 获取场景是否正在加载。
        /// </summary>
        /// <param name="sceneAssetName">场景资源名称。</param>
        /// <returns>场景是否正在加载。</returns>
        public bool SceneIsLoading(string sceneAssetName)
        {
            if (string.IsNullOrEmpty(sceneAssetName))
            {
                throw new GameFrameworkException("Scene asset name is invalid.");
            }

            return loadingSceneAssetNames.Contains(sceneAssetName);
        }

        /// <summary>
        /// 获取正在加载场景的资源名称。
        /// </summary>
        /// <returns>正在加载场景的资源名称。</returns>
        public string[] GetLoadingSceneAssetNames()
        {
            return loadingSceneAssetNames.ToArray();
        }

        /// <summary>
        /// 获取正在加载场景的资源名称。
        /// </summary>
        /// <param name="results">正在加载场景的资源名称。</param>
        public void GetLoadingSceneAssetNames(List<string> results)
        {
            if (results == null)
            {
                throw new GameFrameworkException("Results is invalid.");
            }

            results.Clear();
            results.AddRange(loadingSceneAssetNames);
        }

        /// <summary>
        /// 获取场景是否正在卸载。
        /// </summary>
        /// <param name="sceneAssetName">场景资源名称。</param>
        /// <returns>场景是否正在卸载。</returns>
        public bool SceneIsUnloading(string sceneAssetName)
        {
            if (string.IsNullOrEmpty(sceneAssetName))
            {
                throw new GameFrameworkException("Scene asset name is invalid.");
            }

            return unloadingSceneAssetNames.Contains(sceneAssetName);
        }

        /// <summary>
        /// 获取正在卸载场景的资源名称。
        /// </summary>
        /// <returns>正在卸载场景的资源名称。</returns>
        public string[] GetUnloadingSceneAssetNames()
        {
            return unloadingSceneAssetNames.ToArray();
        }

        /// <summary>
        /// 获取正在卸载场景的资源名称。
        /// </summary>
        /// <param name="results">正在卸载场景的资源名称。</param>
        public void GetUnloadingSceneAssetNames(List<string> results)
        {
            if (results == null)
            {
                throw new GameFrameworkException("Results is invalid.");
            }

            results.Clear();
            results.AddRange(unloadingSceneAssetNames);
        }

        /// <summary>
        /// 检查场景资源是否存在。
        /// </summary>
        /// <param name="sceneAssetName">要检查场景资源的名称。</param>
        /// <returns>场景资源是否存在。</returns>
        public bool HasScene(string sceneAssetName)
        {
            return resourceManager.HasAsset(sceneAssetName) != HasAssetResult.NotExist;
        }

        /// <summary>
        /// 加载场景。
        /// </summary>
        /// <param name="sceneAssetName">场景资源名称。</param>
        public void LoadScene(string sceneAssetName)
        {
            LoadScene(sceneAssetName, ResourceConstant.DefaultPriority, null);
        }

        /// <summary>
        /// 加载场景。
        /// </summary>
        /// <param name="sceneAssetName">场景资源名称。</param>
        /// <param name="priority">加载场景资源的优先级。</param>
        public void LoadScene(string sceneAssetName, int priority)
        {
            LoadScene(sceneAssetName, priority, null);
        }

        /// <summary>
        /// 加载场景。
        /// </summary>
        /// <param name="sceneAssetName">场景资源名称。</param>
        /// <param name="userData">用户自定义数据。</param>
        public void LoadScene(string sceneAssetName, object userData)
        {
            LoadScene(sceneAssetName, ResourceConstant.DefaultPriority, userData);
        }

        /// <summary>
        /// 加载场景。
        /// </summary>
        /// <param name="sceneAssetName">场景资源名称。</param>
        /// <param name="priority">加载场景资源的优先级。</param>
        /// <param name="userData">用户自定义数据。</param>
        public void LoadScene(string sceneAssetName, int priority, object userData)
        {
            if (string.IsNullOrEmpty(sceneAssetName))
            {
                throw new GameFrameworkException("Scene asset name is invalid.");
            }

            if (!sceneAssetName.StartsWith("Assets/", StringComparison.Ordinal) || !sceneAssetName.EndsWith(".unity", StringComparison.Ordinal))
            {
                throw new GameFrameworkException(StringUtils.Format("Scene asset name '{}' is invalid.", sceneAssetName));
            }

            if (resourceManager == null)
            {
                throw new GameFrameworkException("You must set resource manager first.");
            }

            if (SceneIsUnloading(sceneAssetName))
            {
                throw new GameFrameworkException(StringUtils.Format("Scene asset '{}' is being unloaded.", sceneAssetName));
            }

            if (SceneIsLoading(sceneAssetName))
            {
                throw new GameFrameworkException(StringUtils.Format("Scene asset '{}' is being loaded.", sceneAssetName));
            }

            if (SceneIsLoaded(sceneAssetName))
            {
                throw new GameFrameworkException(StringUtils.Format("Scene asset '{}' is already loaded.", sceneAssetName));
            }

            loadingSceneAssetNames.Add(sceneAssetName);
            resourceManager.LoadScene(sceneAssetName, priority, loadSceneCallbacks, userData);
        }

        /// <summary>
        /// 卸载场景。
        /// </summary>
        /// <param name="sceneAssetName">场景资源名称。</param>
        public void UnloadScene(string sceneAssetName)
        {
            UnloadScene(sceneAssetName, null);
        }

        /// <summary>
        /// 卸载场景。
        /// </summary>
        /// <param name="sceneAssetName">场景资源名称。</param>
        /// <param name="userData">用户自定义数据。</param>
        public void UnloadScene(string sceneAssetName, object userData)
        {
            if (string.IsNullOrEmpty(sceneAssetName))
            {
                throw new GameFrameworkException("Scene asset name is invalid.");
            }

            if (resourceManager == null)
            {
                throw new GameFrameworkException("You must set resource manager first.");
            }

            if (SceneIsUnloading(sceneAssetName))
            {
                throw new GameFrameworkException(StringUtils.Format("Scene asset '{}' is being unloaded.", sceneAssetName));
            }

            if (SceneIsLoading(sceneAssetName))
            {
                throw new GameFrameworkException(StringUtils.Format("Scene asset '{}' is being loaded.", sceneAssetName));
            }

            if (!SceneIsLoaded(sceneAssetName))
            {
                throw new GameFrameworkException(StringUtils.Format("Scene asset '{}' is not loaded yet.", sceneAssetName));
            }

            unloadingSceneAssetNames.Add(sceneAssetName);
            resourceManager.UnloadScene(sceneAssetName, unloadSceneCallbacks, userData);
        }

        public void UnloadAllScenes()
        {
            foreach (var loadedSceneAssetName in loadedSceneAssetNames)
            {
                UnloadScene(loadedSceneAssetName);
            }
        }

        private void LoadSceneSuccessCallback(string sceneAssetName, float duration, object userData)
        {
            loadingSceneAssetNames.Remove(sceneAssetName);
            loadedSceneAssetNames.Add(sceneAssetName);
            var eve = LoadSceneSuccessEvent.ValueOf(sceneAssetName, duration, userData);
            EventBus.SyncSubmit(eve);
            ReferenceCache.Release(eve);
        }

        private void LoadSceneFailureCallback(string sceneAssetName, LoadResourceStatus status, string errorMessage, object userData)
        {
            loadingSceneAssetNames.Remove(sceneAssetName);

            var eve = LoadSceneFailureEvent.ValueOf(sceneAssetName, userData);
            EventBus.SyncSubmit(eve);
            ReferenceCache.Release(eve);

            Log.Error("Load scene failure, scene asset name '{}', status '{}', error message '{}'.", sceneAssetName, status.ToString(), errorMessage);
        }

        private void LoadSceneUpdateCallback(string sceneAssetName, float progress, object userData)
        {
            var eve = LoadSceneUpdateEvent.ValueOf(sceneAssetName, progress, userData);
            EventBus.SyncSubmit(eve);
            ReferenceCache.Release(eve);
        }

        private void LoadSceneDependencyAssetCallback(string sceneAssetName, string dependencyAssetName, int loadedCount, int totalCount, object userData)
        {
            var eve = LoadSceneDependencyAssetEvent.ValueOf(sceneAssetName, dependencyAssetName, loadedCount, totalCount, userData);
            EventBus.SyncSubmit(eve);
            ReferenceCache.Release(eve);
        }

        private void UnloadSceneSuccessCallback(string sceneAssetName, object userData)
        {
            unloadingSceneAssetNames.Remove(sceneAssetName);
            loadedSceneAssetNames.Remove(sceneAssetName);

            var eve = UnloadSceneSuccessEvent.ValueOf(sceneAssetName, userData);
            EventBus.SyncSubmit(eve);
            ReferenceCache.Release(eve);
        }

        private void UnloadSceneFailureCallback(string sceneAssetName, object userData)
        {
            unloadingSceneAssetNames.Remove(sceneAssetName);

            var eve = UnloadSceneFailureEvent.ValueOf(sceneAssetName, userData);
            EventBus.SyncSubmit(eve);
            ReferenceCache.Release(eve);

            Log.Error(StringUtils.Format("Unload scene failure, scene asset name '{}'.", sceneAssetName));
        }
    }
}