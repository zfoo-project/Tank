using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Spring.Core;
using Spring.Event;
using Spring.Logger;
using Spring.Storage.Model.Anno;
using Spring.Util;
using Spring.Util.Json;

namespace Spring.Storage.Helper
{
    public class ExcelStorageHelper : IStorageHelper
    {
        public static string ExcelName(Type type)
        {
            return StringUtils.Format("{}.xlsx", type.Name);
        }

        public void InitStorage()
        {
            ScanAndInitResource();
        }


        private static Dictionary<Type, FileInfo> GetResourceFiles()
        {
            // 获取所有配置表资源类
            var classTypes = AssemblyUtils.GetAllClassTypes();
            var resourceTypes = classTypes.Where(it => it.IsDefined(typeof(Resource), false)).ToList();

            // 获取所有的文件
            var allReadableFiles = FileUtils.GetAllReadableFiles();

            var resourceDefinitionMap = new Dictionary<Type, FileInfo>();
            foreach (var resourceType in resourceTypes)
            {
                var matchedResourceNameXlsx = ExcelName(resourceType);
                var findFlag = false;
                foreach (var fileInfo in allReadableFiles)
                {
                    if (fileInfo.Name.Equals(matchedResourceNameXlsx))
                    {
                        resourceDefinitionMap[resourceType] = fileInfo;
                        findFlag = true;
                    }
                }

                if (!findFlag)
                {
                    throw new Exception(StringUtils.Format("无法找到类型[{}]对应的资源文件", resourceType.Name));
                }
            }

            return resourceDefinitionMap;
        }

        public static void ScanAndInitResource()
        {
            var resourceDefinitionMap = GetResourceFiles();

            var resourceListMap = new Dictionary<Type, List<object>>();
            foreach (var resourceDefinition in resourceDefinitionMap)
            {
                var resourceType = resourceDefinition.Key;
                Stream resourceStream = null;

                // Log.Info(StringUtils.Format("Resource-{}-{}", resourceDefinition.Key, resourceDefinition.Value));
                try
                {
                    resourceStream = resourceDefinition.Value.Open(FileMode.Open);

                    var reader = StorageContext.GetResourceReader();
                    var list = reader.Read(resourceType, resourceStream);
                    resourceListMap[resourceDefinition.Key] = list;
                }
                catch (Exception e)
                {
                    throw new Exception(StringUtils.Format("配置表[{}]读取错误", resourceType.Name), e);
                }
                finally
                {
                    IOUtils.CloseIO(resourceStream);
                }
            }

            StorageContext.GetStorageManager().InitBefore(resourceListMap);

            foreach (var resourceType in resourceDefinitionMap.Keys)
            {
                EventBus.SyncSubmit(LoadStorageSuccessEvent.ValueOf(resourceType));
            }
        }

        public static void ExcelToJsonFiles()
        {
            Log.Info("开始生成Excel文件");
            SpringContext.Shutdown();

            var scanPaths = new List<string>();
            scanPaths.Add(typeof(StorageContext).Namespace);
            SpringContext.AddScanPath(scanPaths);

            SpringContext.RegisterBean(new ExcelStorageHelper());

            SpringContext.Scan();

            StorageContext.GetStorageHelper().InitStorage();

            var resourceDefinitionMap = GetResourceFiles();

            foreach (var resourceDefinition in resourceDefinitionMap)
            {
                var type = resourceDefinition.Key;
                var fileInfo = resourceDefinition.Value;

                var storage = StorageContext.GetStorageManager().GetStorage(type);
                var list = storage.GetAll();

                var jsonFilePath = PathUtils.GetRegularPath(Path.Combine(fileInfo.Directory.FullName, StringUtils.Format("{}.json", type.Name)));
                FileUtils.WriteTextFile(jsonFilePath, JsonUtils.object2String(list));
            }

            Log.Info("生成了[{}]个Json文件", resourceDefinitionMap.Count);
        }

        public static void ClearAllJsonFiles()
        {
            Log.Info("开始删除Excel文件");
            var resourceDefinitionMap = GetResourceFiles();
            var count = 0;
            foreach (var resourceDefinition in resourceDefinitionMap)
            {
                var type = resourceDefinition.Key;
                var fileInfo = resourceDefinition.Value;

                var jsonFilePath = PathUtils.GetRegularPath(Path.Combine(fileInfo.Directory.FullName, StringUtils.Format("{}.json", type.Name)));
                if (File.Exists(jsonFilePath))
                {
                    FileUtils.DeleteFile(jsonFilePath);
                    count++;
                }
            }

            Log.Info("删除了[{}]个Json文件", count);
        }
    }
}