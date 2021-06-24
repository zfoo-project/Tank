using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Summer.Resource;
using Summer.Resource.Model.Callback;
using Summer.Resource.Model.Constant;
using Spring.Core;
using Spring.Event;
using Spring.Storage.Model.Anno;
using Spring.Util;
using Spring.Util.Json;
using UnityEngine;

namespace Spring.Storage.Helper
{
    public class JsonStorageHelper : IStorageHelper
    {
        
        public static string GetJsonStorageAsset(string assetName)
        {
            return StringUtils.Format("Assets/Excel/{}.json", assetName);
        }
        
        public void InitStorage()
        {
            var classTypes = AssemblyUtils.GetAllClassTypes();
            var resourceTypes = classTypes.Where(it => it.IsDefined(typeof(Resource), false)).ToList();

            var resourceManager = SpringContext.GetBean<IResourceManager>();
            foreach (var resourceType in resourceTypes)
            {
                // var assetPath = PathUtils.GetRemotePath(Path.Combine(resourceManager.readOnlyPath, StringUtils.Format(ASSET_PATH, ExcelName(resourceType))));
                var assetPath = GetJsonStorageAsset(resourceType.Name);
                resourceManager.LoadAsset(assetPath, ResourceConstant.ExcelAsset, new LoadAssetCallbacks(OnLoadJsonSuccess), resourceType);
            }
        }


        private void OnLoadJsonSuccess(string assetName, object asset, float duration, object userData)
        {
            var resourceType = (Type) userData;

            var textAsset = (TextAsset) asset;
            var resourceListType = typeof(List<>).MakeGenericType(resourceType);
            var list = (IList) JsonUtils.string2Object(textAsset.text, resourceListType);

            var resourceList = new List<object>();
            foreach (var resource in list)
            {
                resourceList.Add(resource);
            }

            var resourceListMap = new Dictionary<Type, List<object>>();
            resourceListMap[resourceType] = resourceList;
            StorageContext.GetStorageManager().InitBefore(resourceListMap);

            EventBus.SyncSubmit(LoadStorageSuccessEvent.ValueOf(resourceType));
        }
    }
}