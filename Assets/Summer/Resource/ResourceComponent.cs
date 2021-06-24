using System;
using Summer.Base;
using Summer.Resource.Manager;
using Summer.Resource.Model.Constant;
using Summer.Resource.Model.Loader;
using Spring.Core;
using Spring.Logger;
using Spring.Util;
using UnityEngine;
using SpringComponent = Summer.Base.SpringComponent;

namespace Summer.Resource
{
    /// <summary>
    /// 资源组件。
    /// </summary>
    [DisallowMultipleComponent]
    [AddComponentMenu("Summer/Resource")]
    public sealed class ResourceComponent : SpringComponent
    {
        private const int OneMegaBytes = 1024 * 1024;

        [Autowired]
        private IResourceManager resourceManager;

        private bool forceUnloadUnusedAssets;
        private bool preorderUnloadUnusedAssets;
        private bool performGCCollect;
        private AsyncOperation asyncOperation;
        private float lastOperationElapse;

        [SerializeField]
        private ResourceMode resourceMode = ResourceMode.Package;

        [SerializeField]
        public ReadWritePathType readWritePathType = ReadWritePathType.Unspecified;

        [SerializeField]
        public float unloadUnusedAssetsInterval = 60f;

        [SerializeField]
        private float assetAutoReleaseInterval = 60f;

        [SerializeField]
        private int assetCapacity = 64;

        [SerializeField]
        private float assetExpireTime = 60f;

        [SerializeField]
        private int assetPriority = 0;

        [SerializeField]
        private float resourceAutoReleaseInterval = 60f;

        [SerializeField]
        private int resourceCapacity = 16;

        [SerializeField]
        private float resourceExpireTime = 60f;

        [SerializeField]
        private int resourcePriority;

        [SerializeField]
        private int generateReadWriteVersionListLength = OneMegaBytes;

        [SerializeField]
        private int updateRetryCount = 3;

        [SerializeField]
        private int loadResourceAgentHelperCount = 3;

        /// 游戏框架组件初始化。
        /// </summary>
        protected override void Awake()
        {
            var resourceGameObject = new GameObject();
            var resourceHelper = resourceGameObject.AddComponent<SimpleLoadResourceMono>();
            resourceGameObject.name = "Resource Helper";
            resourceGameObject.transform.SetParent(transform);
            resourceGameObject.transform.localScale = Vector3.one;
            SpringContext.RegisterBean(resourceHelper);
            
            base.Awake();
        }

        [PostConstruct]
        private void Init()
        {
            var baseComponent = SpringContext.GetBean<BaseComponent>();

            resourceManager.SetReadOnlyPath(Application.streamingAssetsPath);
            if (readWritePathType == ReadWritePathType.TemporaryCache)
            {
                resourceManager.SetReadWritePath(Application.temporaryCachePath);
            }
            else
            {
                if (readWritePathType == ReadWritePathType.Unspecified)
                {
                    readWritePathType = ReadWritePathType.PersistentData;
                }

                resourceManager.SetReadWritePath(Application.persistentDataPath);
            }

            if (baseComponent.editorResourceMode)
            {
                return;
            }

            resourceManager.SetResourceMode(resourceMode);

            resourceManager.AssetAutoReleaseInterval = assetAutoReleaseInterval;
            resourceManager.AssetCapacity = assetCapacity;
            resourceManager.AssetExpireTime = assetExpireTime;
            resourceManager.AssetPriority = assetPriority;
            resourceManager.ResourceAutoReleaseInterval = resourceAutoReleaseInterval;
            resourceManager.ResourceCapacity = resourceCapacity;
            resourceManager.ResourceExpireTime = resourceExpireTime;
            resourceManager.ResourcePriority = resourcePriority;
            if (resourceMode == ResourceMode.Updatable)
            {
                resourceManager.GenerateReadWriteVersionListLength = generateReadWriteVersionListLength;
                resourceManager.UpdateRetryCount = updateRetryCount;
            }


            for (int i = 0; i < loadResourceAgentHelperCount; i++)
            {
                AddLoadResourceAgentHelper(i);
            }
        }

        private void Update()
        {
            lastOperationElapse += Time.unscaledDeltaTime;
            if (asyncOperation == null && (forceUnloadUnusedAssets || preorderUnloadUnusedAssets && lastOperationElapse >= unloadUnusedAssetsInterval))
            {
                Log.Info("Unload unused assets...");
                forceUnloadUnusedAssets = false;
                preorderUnloadUnusedAssets = false;
                lastOperationElapse = 0f;
                asyncOperation = Resources.UnloadUnusedAssets();
            }

            if (asyncOperation != null && asyncOperation.isDone)
            {
                asyncOperation = null;
                if (performGCCollect)
                {
                    Log.Info("GC.Collect...");
                    performGCCollect = false;
                    GC.Collect();
                }
            }
        }


        /// <summary>
        /// 强制执行释放未被使用的资源。
        /// </summary>
        /// <param name="performGCCollect">是否使用垃圾回收。</param>
        public void ForceUnloadUnusedAssets(bool performGCCollect)
        {
            forceUnloadUnusedAssets = true;
            if (performGCCollect)
            {
                this.performGCCollect = performGCCollect;
            }
        }


        /// <summary>
        /// 增加加载资源代理辅助器。
        /// </summary>
        /// <param name="index">加载资源代理辅助器索引。</param>
        private void AddLoadResourceAgentHelper(int index)
        {
            var loadResourceAgentObject = new GameObject();
            var loadResourceAgentMono = loadResourceAgentObject.AddComponent<LoadResourceAgentMono>();

            loadResourceAgentObject.name = StringUtils.Format("Load Resource Agent Helper - {}", index.ToString());
            loadResourceAgentObject.transform.SetParent(transform);
            loadResourceAgentObject.transform.localScale = Vector3.one;

            SpringContext.GetBean<ResourceLoader>().AddLoadResourceAgent(loadResourceAgentMono);
        }
    }
}