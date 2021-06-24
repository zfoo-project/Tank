using System;
using System.Collections.Generic;
using Summer.Base.Model;
using Summer.ObjectPool;
using Summer.Procedure;
using Summer.Resource;
using Summer.Resource.Manager;
using Spring.Collection.Reference;
using Spring.Core;
using Spring.Event;
using Spring.Logger;
using Spring.Storage;
using Spring.Storage.Helper;
using Spring.Util;
using Spring.Util.Json;
using Spring.Util.Zip;
using Summer.Net.Dispatcher;
using UnityEngine;
using ResourceLoader = Summer.Resource.Model.Loader.ResourceLoader;

namespace Summer.Base
{
    /// <summary>
    /// 基础组件。
    /// </summary>
    [DisallowMultipleComponent]
    [AddComponentMenu("Summer/Base")]
    public sealed class BaseComponent : SpringComponent
    {
        private const int DefaultDpi = 96; // default windows dpi

        private float gameSpeedBeforePause = 1f;

        /// <summary>
        /// 游戏框架所在的场景编号。
        /// </summary>
        public const int GameFrameworkSceneId = 0;

        [SerializeField]
        public bool editorResourceMode = true;

        [SerializeField]
        public string versionHelperTypeName;

        [SerializeField]
        public string logHelperTypeName;

        [SerializeField]
        public string zipHelperTypeName;

        [SerializeField]
        public string jsonHelperTypeName;

        [SerializeField]
        public string storageHelperTypeName;

        [SerializeField]
        public int frameRate = 30;

        [SerializeField]
        public float gameSpeed = 1f;

        [SerializeField]
        private bool runInBackground = true;

        [SerializeField]
        private bool neverSleep = true;

        
        /// <summary>
        /// 获取或设置游戏帧率。
        /// </summary>
        public int FrameRate
        {
            get { return frameRate; }
            set { Application.targetFrameRate = frameRate = value; }
        }

        /// <summary>
        /// 获取或设置游戏速度。
        /// </summary>
        public float GameSpeed
        {
            get { return gameSpeed; }
            set { Time.timeScale = gameSpeed = value >= 0f ? value : 0f; }
        }

        /// <summary>
        /// 获取游戏是否暂停。
        /// </summary>
        public bool IsGamePaused
        {
            get { return gameSpeed <= 0f; }
        }

        /// <summary>
        /// 获取是否正常游戏速度。
        /// </summary>
        public bool IsNormalGameSpeed
        {
            get { return gameSpeed == 1f; }
        }

        /// <summary>
        /// 获取或设置是否允许后台运行。
        /// </summary>
        public bool RunInBackground
        {
            get { return runInBackground; }
            set { Application.runInBackground = runInBackground = value; }
        }

        /// <summary>
        /// 获取或设置是否禁止休眠。
        /// </summary>
        public bool NeverSleep
        {
            get { return neverSleep; }
            set
            {
                neverSleep = value;
                Screen.sleepTimeout = value ? SleepTimeout.NeverSleep : SleepTimeout.SystemSetting;
            }
        }

        public static BaseComponent INSTANCE = null;

        public BaseComponent()
        {
            INSTANCE = this;
        }


        /// <summary>
        /// 游戏框架组件初始化。
        /// </summary>
        protected override void Awake()
        {
            InitLogHelper();
            InitZipHelper();
            InitJsonHelper();
            InitStorageHelper();

            ConverterUtils.ScreenDpi = Screen.dpi;
            if (ConverterUtils.ScreenDpi <= 0)
            {
                ConverterUtils.ScreenDpi = DefaultDpi;
            }

            // 初始化资源加载
            editorResourceMode &= Application.isEditor;
            if (editorResourceMode)
            {
                SpringContext.RegisterBean<EditorResourceManager>();
                Log.Info("During this run, Game Framework will use editor resource files, which you should validate first.");
            }
            else
            {
                SpringContext.RegisterBean<ResourceManager>();
                SpringContext.RegisterBean<ResourceChecker>();
                SpringContext.RegisterBean<ResourceIniter>();
                SpringContext.RegisterBean<ResourceUpdater>();
                SpringContext.RegisterBean<ResourceLoader>();
                SpringContext.RegisterBean<VersionListProcessor>();
            }


            Application.targetFrameRate = frameRate;
            Time.timeScale = gameSpeed;
            Application.runInBackground = runInBackground;
            Screen.sleepTimeout = neverSleep ? SleepTimeout.NeverSleep : SleepTimeout.SystemSetting;

            Application.lowMemory += OnLowMemory;
            
            base.Awake();
        }

        private int moduleSize = 0;
        private readonly List<AbstractManager> CachedModules = new List<AbstractManager>();

        public void StartSpring()
        {
            // 扫描路径
            var scanPaths = new List<string>()
            {
                "Summer",
                "MiniKing",
                typeof(StorageContext).Namespace
            };
            SpringContext.AddScanPath(scanPaths);

            // 扫描全部的类
            SpringContext.Scan();

            // 事件扫描
            EventBus.Scan();
            
            // 网络扫描
            PacketDispatcher.Scan();

            // 获取全部的Module
            var moduleList = new List<AbstractManager>();
            var moduleComponents = SpringContext.GetBeans(typeof(AbstractManager));
            moduleComponents.ForEach(it => moduleList.Add((AbstractManager) it));
            moduleList.Sort((a, b) => b.Priority - a.Priority);
            moduleList.ForEach(it => CachedModules.Add(it));
            moduleSize = (short) moduleList.Count;

            // 初始化流程状态机
            SpringContext.GetBean<ProcedureComponent>().StartProcedure();
        }
        

        private void Update()
        {
            for (var i = 0; i < moduleSize; i++)
            {
                CachedModules[i].Update(Time.deltaTime, Time.unscaledDeltaTime);
            }
        }

        private void OnApplicationQuit()
        {
            Application.lowMemory -= OnLowMemory;
            StopAllCoroutines();
        }

        private void OnDestroy()
        {
            for (var i = moduleSize - 1; i >= 0; i--)
            {
                CachedModules[i].Shutdown();
            }

            CachedModules.Clear();
            ReferenceCache.ClearAll();
            MarshalUtils.FreeCachedHGlobal();
            LogBridge.SetLogHelper(null);
            SpringContext.Shutdown();
        }

        /// <summary>
        /// 暂停游戏。
        /// </summary>
        public void PauseGame()
        {
            if (IsGamePaused)
            {
                return;
            }

            gameSpeedBeforePause = GameSpeed;
            GameSpeed = 0f;
        }

        /// <summary>
        /// 恢复游戏。
        /// </summary>
        public void ResumeGame()
        {
            if (!IsGamePaused)
            {
                return;
            }

            GameSpeed = gameSpeedBeforePause;
        }

        /// <summary>
        /// 重置为正常游戏速度。
        /// </summary>
        public void ResetNormalGameSpeed()
        {
            if (IsNormalGameSpeed)
            {
                return;
            }

            GameSpeed = 1f;
        }

        private void InitLogHelper()
        {
            Type logHelperType = AssemblyUtils.GetTypeByName(logHelperTypeName);
            if (logHelperType == null)
            {
                throw new GameFrameworkException(StringUtils.Format("Can not find log helper type '{}'.", logHelperTypeName));
            }

            ILogHelper logHelper = (ILogHelper) Activator.CreateInstance(logHelperType);
            if (logHelper == null)
            {
                throw new GameFrameworkException(StringUtils.Format("Can not create log helper instance '{}'.", logHelperTypeName));
            }

            LogBridge.SetLogHelper(logHelper);
        }

        private void InitZipHelper()
        {
            Type zipHelperType = AssemblyUtils.GetTypeByName(zipHelperTypeName);
            if (zipHelperType == null)
            {
                Log.Error("Can not find Zip helper type '{}'.", zipHelperTypeName);
                return;
            }

            IZipHelper zipHelper = (IZipHelper) Activator.CreateInstance(zipHelperType);
            if (zipHelper == null)
            {
                Log.Error("Can not create Zip helper instance '{}'.", zipHelperTypeName);
                return;
            }

            ZipUtils.SetZipHelper(zipHelper);
        }

        private void InitJsonHelper()
        {
            Type jsonHelperType = AssemblyUtils.GetTypeByName(jsonHelperTypeName);
            if (jsonHelperType == null)
            {
                Log.Error("Can not find JSON helper type '{}'.", jsonHelperTypeName);
                return;
            }

            IJsonHelper jsonHelper = (IJsonHelper) Activator.CreateInstance(jsonHelperType);
            if (jsonHelper == null)
            {
                Log.Error("Can not create JSON helper instance '{}'.", jsonHelperTypeName);
                return;
            }

            JsonUtils.SetJsonHelper(jsonHelper);
        }

        private void InitStorageHelper()
        {
            Type storageHelperType = AssemblyUtils.GetTypeByName(storageHelperTypeName);
            if (storageHelperType == null)
            {
                Log.Error("Can not find Storage helper type '{}'.", storageHelperTypeName);
                return;
            }

            IStorageHelper storageHelper = (IStorageHelper) Activator.CreateInstance(storageHelperType);
            if (storageHelper == null)
            {
                Log.Error("Can not create Storage helper instance '{}'.", jsonHelperTypeName);
                return;
            }

            SpringContext.RegisterBean(storageHelper);
        }

        private void OnLowMemory()
        {
            Log.Info("Low memory reported...");

            SpringContext.GetBean<IObjectPoolManager>().ReleaseAllUnused();
            SpringContext.GetBean<ResourceComponent>().ForceUnloadUnusedAssets(true);
        }


        /// <summary>
        /// 关闭游戏框架。
        /// </summary>
        /// <param name="shutdownType">关闭游戏框架类型。</param>
        public void Shutdown(ShutdownType shutdownType)
        {
            Log.Info("Shutdown Game Framework ({})...", shutdownType.ToString());

            Destroy(gameObject);

            Application.Quit();

#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#endif
            return;
        }
    }
}