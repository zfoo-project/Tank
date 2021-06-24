using System;
using System.Collections.Generic;
using System.IO;
using Summer.Base.Model;
using Summer.Download;
using Summer.Download.Model.Eve;
using Summer.FileSystem.Model;
using Summer.Resource.Model.Group;
using Summer.Resource.Model.LocalVersion;
using Summer.Resource.Model.ResourcePackVersion;
using Summer.Resource.Model.Vo;
using Spring.Collection.Reference;
using Spring.Core;
using Spring.Logger;
using Spring.Util;
using Spring.Util.Math;
using Spring.Util.Security;
using Spring.Util.Zip;
using Summer.Resource.Model.Constant;

namespace Summer.Resource.Manager
{
    /// <summary>
    /// 资源更新器。
    /// </summary>
    public sealed class ResourceUpdater
    {
        private const int CachedHashBytesLength = 4;
        private const int CachedBytesLength = 0x1000;

        [Autowired]
        private readonly ResourceManager resourceManager;

        [Autowired]
        private IDownloadManager downloadManager;

        [Autowired]
        public LocalVersionListSerializer localVersionListSerializer;

        [Autowired]
        public ResourcePackVersionListSerializer resourcePackVersionListSerializer;

        private readonly List<ApplyInfo> applyWaitingInfo = new List<ApplyInfo>();
        private readonly List<UpdateInfo> updateWaitingInfo = new List<UpdateInfo>();

        private readonly Dictionary<ResourceName, UpdateInfo> updateCandidateInfo = new Dictionary<ResourceName, UpdateInfo>();

        private readonly SortedDictionary<string, List<int>> cachedFileSystemsForGenerateReadWriteVersionList = new SortedDictionary<string, List<int>>(StringComparer.Ordinal);

        private readonly byte[] cachedHashBytes = new byte[CachedHashBytesLength];
        private readonly byte[] cachedBytes = new byte[CachedBytesLength];
        private bool checkResourcesComplete;
        private string applyingResourcePackPath;
        private FileStream applyingResourcePackStream;
        private ResourceGroup updatingResourceGroup;
        private int generateReadWriteVersionListLength;
        private int currentGenerateReadWriteVersionListLength;
        private int updateRetryCount = 3;
        private int updatingCount;
        private bool failureFlag;
        private string readWriteVersionListFileName;
        private string readWriteVersionListBackupFileName;

        public GameFrameworkAction<ResourceName, string, string, int, int> ResourceApplySuccess;
        public GameFrameworkAction<ResourceName, string, string> ResourceApplyFailure;
        public GameFrameworkAction<string, bool, bool> ResourceApplyComplete;
        public GameFrameworkAction<ResourceName, string, string, int, int, int> ResourceUpdateStart;
        public GameFrameworkAction<ResourceName, string, string, int, int> ResourceUpdateChanged;
        public GameFrameworkAction<ResourceName, string, string, int, int> ResourceUpdateSuccess;
        public GameFrameworkAction<ResourceName, string, int, int, string> ResourceUpdateFailure;
        public GameFrameworkAction<ResourceGroup, bool, bool> ResourceUpdateComplete;

        /// <summary>
        /// 初始化资源更新器的新实例。
        /// </summary>
        /// <param name="resourceManager">资源管理器。</param>
        [AfterPostConstruct]
        private void Init()
        {
            downloadManager.DownloadStart += OnDownloadStart;
            downloadManager.DownloadUpdate += OnDownloadUpdate;
            downloadManager.DownloadSuccess += OnDownloadSuccess;
            downloadManager.DownloadFailure += OnDownloadFailure;

            readWriteVersionListFileName = PathUtils.GetRegularPath(Path.Combine(resourceManager.readWritePath, ResourceManager.LocalVersionListFileName));
            readWriteVersionListBackupFileName = StringUtils.Format("{}.{}", readWriteVersionListFileName, ResourceManager.BackupExtension);
        }

        /// <summary>
        /// 获取或设置每更新多少字节的资源，重新生成一次版本资源列表。
        /// </summary>
        public int GenerateReadWriteVersionListLength
        {
            get { return generateReadWriteVersionListLength; }
            set { generateReadWriteVersionListLength = value; }
        }

        /// <summary>
        /// 获取正在应用的资源包路径。
        /// </summary>
        public string ApplyingResourcePackPath
        {
            get { return applyingResourcePackPath; }
        }

        /// <summary>
        /// 获取等待应用资源数量。
        /// </summary>
        public int ApplyWaitingCount
        {
            get { return applyWaitingInfo.Count; }
        }

        /// <summary>
        /// 获取或设置资源更新重试次数。
        /// </summary>
        public int UpdateRetryCount
        {
            get { return updateRetryCount; }
            set { updateRetryCount = value; }
        }

        /// <summary>
        /// 获取正在更新的资源组。
        /// </summary>
        public IResourceGroup UpdatingResourceGroup
        {
            get { return updatingResourceGroup; }
        }

        /// <summary>
        /// 获取等待更新资源数量。
        /// </summary>
        public int UpdateWaitingCount
        {
            get { return updateWaitingInfo.Count; }
        }

        /// <summary>
        /// 获取候选更新资源数量。
        /// </summary>
        public int UpdateCandidateCount
        {
            get { return updateCandidateInfo.Count; }
        }

        /// <summary>
        /// 获取正在更新资源数量。
        /// </summary>
        public int UpdatingCount
        {
            get { return updatingCount; }
        }

        /// <summary>
        /// 资源更新器轮询。
        /// </summary>
        /// <param name="elapseSeconds">逻辑流逝时间，以秒为单位。</param>
        /// <param name="realElapseSeconds">真实流逝时间，以秒为单位。</param>
        public void Update(float elapseSeconds, float realElapseSeconds)
        {
            if (applyingResourcePackStream != null)
            {
                while (applyWaitingInfo.Count > 0)
                {
                    ApplyInfo applyInfo = applyWaitingInfo[0];
                    applyWaitingInfo.RemoveAt(0);
                    if (ApplyResource(applyInfo))
                    {
                        return;
                    }
                }

                Array.Clear(cachedBytes, 0, CachedBytesLength);
                string resourcePackPath = applyingResourcePackPath;
                applyingResourcePackPath = null;
                applyingResourcePackStream.Dispose();
                applyingResourcePackStream = null;
                if (ResourceApplyComplete != null)
                {
                    ResourceApplyComplete(resourcePackPath, !failureFlag, updateCandidateInfo.Count <= 0);
                }
            }

            if (updateWaitingInfo.Count > 0)
            {
                while (updateWaitingInfo.Count > 0 && downloadManager.WaitingTaskCount < downloadManager.FreeAgentCount)
                {
                    UpdateInfo updateInfo = updateWaitingInfo[0];
                    updateWaitingInfo.RemoveAt(0);
                    string resourceFullNameWithCrc32 = updateInfo.ResourceName.Variant != null
                        ? StringUtils.Format("{}.{}.{}.{}", updateInfo.ResourceName.Name, updateInfo.ResourceName.Variant, NumberUtils.ToHex(updateInfo.HashCode), ResourceManager.DefaultExtension)
                        : StringUtils.Format("{}.{}.{}", updateInfo.ResourceName.Name, NumberUtils.ToHex(updateInfo.HashCode), ResourceManager.DefaultExtension);
                    var downloadUrl = PathUtils.GetRemotePath(Path.Combine(resourceManager.updatePrefixUri, resourceFullNameWithCrc32));
                    downloadManager.AddDownload(updateInfo.ResourcePath, downloadUrl, updateInfo);

                    Log.Info("开始从[{}]下载热更新文件", downloadUrl);
                    updatingCount++;
                }

                return;
            }

            if (updatingResourceGroup != null && updatingCount <= 0)
            {
                ResourceGroup updatingResourceGroup = this.updatingResourceGroup;
                this.updatingResourceGroup = null;
                if (ResourceUpdateComplete != null)
                {
                    ResourceUpdateComplete(updatingResourceGroup, !failureFlag, updateCandidateInfo.Count <= 0);
                }

                return;
            }
        }

        /// <summary>
        /// 关闭并清理资源更新器。
        /// </summary>
        public void Shutdown()
        {
            if (downloadManager != null)
            {
                downloadManager.DownloadStart -= OnDownloadStart;
                downloadManager.DownloadUpdate -= OnDownloadUpdate;
                downloadManager.DownloadSuccess -= OnDownloadSuccess;
                downloadManager.DownloadFailure -= OnDownloadFailure;
            }

            updateWaitingInfo.Clear();
            updateCandidateInfo.Clear();
            cachedFileSystemsForGenerateReadWriteVersionList.Clear();
        }


        /// <summary>
        /// 增加资源更新。
        /// </summary>
        /// <param name="resourceName">资源名称。</param>
        /// <param name="fileSystemName">资源所在的文件系统名称。</param>
        /// <param name="loadType">资源加载方式。</param>
        /// <param name="length">资源大小。</param>
        /// <param name="hashCode">资源哈希值。</param>
        /// <param name="zipLength">压缩后大小。</param>
        /// <param name="zipHashCode">压缩后哈希值。</param>
        /// <param name="resourcePath">资源路径。</param>
        public void AddResourceUpdate(ResourceName resourceName, string fileSystemName, LoadType loadType, int length,
            int hashCode, int zipLength, int zipHashCode, string resourcePath)
        {
            updateCandidateInfo.Add(resourceName, new UpdateInfo(resourceName, fileSystemName, loadType, length, hashCode, zipLength, zipHashCode, resourcePath));
        }

        /// <summary>
        /// 检查资源完成。
        /// </summary>
        /// <param name="needGenerateReadWriteVersionList">是否需要生成读写区版本资源列表。</param>
        public void CheckResourceComplete(bool needGenerateReadWriteVersionList)
        {
            checkResourcesComplete = true;
            if (needGenerateReadWriteVersionList)
            {
                GenerateReadWriteVersionList();
            }
        }

        /// <summary>
        /// 应用指定资源包的资源。
        /// </summary>
        /// <param name="resourcePackPath">要应用的资源包路径。</param>
        public void ApplyResources(string resourcePackPath)
        {
            if (!checkResourcesComplete)
            {
                throw new GameFrameworkException("You must check resources complete first.");
            }

            if (applyingResourcePackStream != null)
            {
                throw new GameFrameworkException(StringUtils.Format("There is already a resource pack '{}' being applied.", applyingResourcePackPath));
            }

            if (updatingResourceGroup != null)
            {
                throw new GameFrameworkException(StringUtils.Format("There is already a resource group '{}' being updated.", updatingResourceGroup.Name));
            }

            try
            {
                long length = 0L;
                ResourcePackVersionList versionList = default(ResourcePackVersionList);
                using (FileStream fileStream = new FileStream(resourcePackPath, FileMode.Open, FileAccess.Read))
                {
                    length = fileStream.Length;
                    versionList = resourcePackVersionListSerializer.Deserialize(fileStream);
                }

                if (!versionList.IsValid)
                {
                    throw new GameFrameworkException("Deserialize resource pack version list failure.");
                }

                if (versionList.Offset + versionList.Length != length)
                {
                    throw new GameFrameworkException("Resource pack length is invalid.");
                }

                applyingResourcePackPath = resourcePackPath;
                applyingResourcePackStream = new FileStream(resourcePackPath, FileMode.Open, FileAccess.Read);
                applyingResourcePackStream.Position = versionList.Offset;
                failureFlag = false;

                ResourcePackVersionResource[] resources = versionList.GetResources();
                foreach (ResourcePackVersionResource resource in resources)
                {
                    ResourceName resourceName = new ResourceName(resource.Name, resource.Variant, resource.Extension);
                    UpdateInfo updateInfo = null;
                    if (!updateCandidateInfo.TryGetValue(resourceName, out updateInfo))
                    {
                        continue;
                    }

                    if (updateInfo.LoadType == (LoadType) resource.LoadType && updateInfo.Length == resource.Length &&
                        updateInfo.HashCode == resource.HashCode)
                    {
                        applyWaitingInfo.Add(new ApplyInfo(resourceName, updateInfo.FileSystemName,
                            (LoadType) resource.LoadType, resource.Offset, resource.Length, resource.HashCode,
                            resource.ZipLength, resource.ZipHashCode, updateInfo.ResourcePath));
                    }
                }
            }
            catch (Exception exception)
            {
                if (applyingResourcePackStream != null)
                {
                    applyingResourcePackStream.Dispose();
                    applyingResourcePackStream = null;
                }

                throw new GameFrameworkException(StringUtils.Format("Apply resources '{}' with exception '{}'.", resourcePackPath, exception.ToString()), exception);
            }
        }

        /// <summary>
        /// 更新指定资源组的资源。
        /// </summary>
        /// <param name="resourceGroup">要更新的资源组。</param>
        public void UpdateResources(ResourceGroup resourceGroup)
        {
            if (downloadManager == null)
            {
                throw new GameFrameworkException("You must set download manager first.");
            }

            if (!checkResourcesComplete)
            {
                throw new GameFrameworkException("You must check resources complete first.");
            }

            if (applyingResourcePackStream != null)
            {
                throw new GameFrameworkException(StringUtils.Format("There is already a resource pack '{}' being applied.", applyingResourcePackPath));
            }

            if (updatingResourceGroup != null)
            {
                throw new GameFrameworkException(StringUtils.Format("There is already a resource group '{}' being updated.", updatingResourceGroup.Name));
            }

            if (string.IsNullOrEmpty(resourceGroup.Name))
            {
                foreach (KeyValuePair<ResourceName, UpdateInfo> updateInfo in updateCandidateInfo)
                {
                    updateWaitingInfo.Add(updateInfo.Value);
                }

                updateCandidateInfo.Clear();
            }
            else
            {
                ResourceName[] resourceNames = resourceGroup.InternalGetResourceNames();
                foreach (ResourceName resourceName in resourceNames)
                {
                    UpdateInfo updateInfo = null;
                    if (!updateCandidateInfo.TryGetValue(resourceName, out updateInfo))
                    {
                        continue;
                    }

                    updateWaitingInfo.Add(updateInfo);
                    updateCandidateInfo.Remove(resourceName);
                }
            }

            updatingResourceGroup = resourceGroup;
            failureFlag = false;
        }

        public void UpdateResource(ResourceName resourceName)
        {
            if (downloadManager == null)
            {
                throw new GameFrameworkException("You must set download manager first.");
            }

            if (!checkResourcesComplete)
            {
                throw new GameFrameworkException("You must check resources complete first.");
            }

            if (applyingResourcePackStream != null)
            {
                throw new GameFrameworkException(StringUtils.Format("There is already a resource pack '{}' being applied.", applyingResourcePackPath));
            }

            UpdateInfo updateInfo = null;
            if (updateCandidateInfo.TryGetValue(resourceName, out updateInfo))
            {
                updateWaitingInfo.Add(updateInfo);
                updateCandidateInfo.Remove(resourceName);
            }
        }

        private bool ApplyResource(ApplyInfo applyInfo)
        {
            long position = applyingResourcePackStream.Position;
            try
            {
                bool zip = applyInfo.Length != applyInfo.ZipLength || applyInfo.HashCode != applyInfo.ZipHashCode;

                int bytesRead = 0;
                int bytesLeft = applyInfo.ZipLength;
                string directory = Path.GetDirectoryName(applyInfo.ResourcePath);
                if (!Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                applyingResourcePackStream.Position += applyInfo.Offset;
                using (FileStream fileStream = new FileStream(applyInfo.ResourcePath, FileMode.Create, FileAccess.ReadWrite))
                {
                    while ((bytesRead = applyingResourcePackStream.Read(cachedBytes, 0, bytesLeft < CachedBytesLength ? bytesLeft : CachedBytesLength)) > 0)
                    {
                        bytesLeft -= bytesRead;
                        fileStream.Write(cachedBytes, 0, bytesRead);
                    }

                    if (zip)
                    {
                        fileStream.Position = 0L;
                        int hashCode = Crc32Utils.GetCrc32(fileStream);
                        if (hashCode != applyInfo.ZipHashCode)
                        {
                            if (ResourceApplyFailure != null)
                            {
                                string errorMessage = StringUtils.Format("Resource zip hash code error, need '{}', applied '{}'.", applyInfo.ZipHashCode.ToString(), hashCode.ToString());
                                ResourceApplyFailure(applyInfo.ResourceName, applyingResourcePackPath, errorMessage);
                            }
                            failureFlag = true;
                            return false;
                        }

                        if (resourceManager.decompressCachedStream == null)
                        {
                            resourceManager.decompressCachedStream = new MemoryStream();
                        }

                        fileStream.Position = 0L;
                        resourceManager.decompressCachedStream.Position = 0L;
                        resourceManager.decompressCachedStream.SetLength(0L);
                        if (!ZipUtils.Decompress(fileStream, resourceManager.decompressCachedStream))
                        {
                            if (ResourceApplyFailure != null)
                            {
                                string errorMessage = StringUtils.Format("Unable to decompress resource '{}'.", applyInfo.ResourcePath);
                                ResourceApplyFailure(applyInfo.ResourceName, applyingResourcePackPath, errorMessage);
                            }
                            failureFlag = true;
                            return false;
                        }

                        fileStream.Position = 0L;
                        fileStream.SetLength(0L);
                        fileStream.Write(resourceManager.decompressCachedStream.GetBuffer(), 0, (int) resourceManager.decompressCachedStream.Length);
                    }
                    else
                    {
                        int hashCode = 0;
                        fileStream.Position = 0L;
                        if (applyInfo.LoadType == LoadType.LoadFromMemoryAndQuickDecrypt || applyInfo.LoadType == LoadType.LoadFromMemoryAndDecrypt
                                                                                         || applyInfo.LoadType == LoadType.LoadFromBinaryAndQuickDecrypt
                                                                                         || applyInfo.LoadType == LoadType.LoadFromBinaryAndDecrypt)
                        {
                            applyInfo.HashCode = ConverterUtils.GetInt32(cachedHashBytes);
                            if (applyInfo.LoadType == LoadType.LoadFromMemoryAndQuickDecrypt || applyInfo.LoadType == LoadType.LoadFromBinaryAndQuickDecrypt)
                            {
                                hashCode = Crc32Utils.GetCrc32(fileStream, cachedHashBytes, EncryptionUtils.QuickEncryptLength);
                            }
                            else if (applyInfo.LoadType == LoadType.LoadFromMemoryAndDecrypt || applyInfo.LoadType == LoadType.LoadFromBinaryAndDecrypt)
                            {
                                hashCode = Crc32Utils.GetCrc32(fileStream, cachedHashBytes, applyInfo.Length);
                            }

                            Array.Clear(cachedHashBytes, 0, CachedHashBytesLength);
                        }
                        else
                        {
                            hashCode = Crc32Utils.GetCrc32(fileStream);
                        }

                        if (hashCode != applyInfo.HashCode)
                        {
                            if (ResourceApplyFailure != null)
                            {
                                string errorMessage = StringUtils.Format("Resource hash code error, need '{}', applied '{}'.", applyInfo.HashCode.ToString(), hashCode.ToString());
                                ResourceApplyFailure(applyInfo.ResourceName, applyingResourcePackPath, errorMessage);
                            }
                            failureFlag = true;
                            return false;
                        }
                    }
                }

                if (applyInfo.UseFileSystem)
                {
                    IFileSystem fileSystem = resourceManager.GetFileSystem(applyInfo.FileSystemName, false);
                    bool retVal = fileSystem.WriteFile(applyInfo.ResourceName.FullName, applyInfo.ResourcePath);
                    if (File.Exists(applyInfo.ResourcePath))
                    {
                        File.Delete(applyInfo.ResourcePath);
                    }

                    if (!retVal)
                    {
                        if (ResourceApplyFailure != null)
                        {
                            string errorMessage = StringUtils.Format("Unable to write resource '{}' to file system '{}'.", applyInfo.ResourcePath, applyInfo.FileSystemName);
                            ResourceApplyFailure(applyInfo.ResourceName, applyingResourcePackPath, errorMessage);
                        }

                        failureFlag = true;
                        return false;
                    }
                }
                
                string downloadingResource = StringUtils.Format("{}.download", applyInfo.ResourcePath);
                if (File.Exists(downloadingResource))
                {
                    File.Delete(downloadingResource);
                }

                updateCandidateInfo.Remove(applyInfo.ResourceName);
                resourceManager.resourceInfos[applyInfo.ResourceName].MarkReady();
                resourceManager.readWriteResourceInfos.Add(applyInfo.ResourceName, new ReadWriteResourceInfo(applyInfo.FileSystemName, applyInfo.LoadType, applyInfo.Length, applyInfo.HashCode));

                if (ResourceApplySuccess != null)
                {
                    ResourceApplySuccess(applyInfo.ResourceName, applyInfo.ResourcePath, applyingResourcePackPath, applyInfo.Length, applyInfo.ZipLength);
                }

                currentGenerateReadWriteVersionListLength += applyInfo.ZipLength;
                if (applyWaitingInfo.Count <= 0 || currentGenerateReadWriteVersionListLength >= generateReadWriteVersionListLength)
                {
                    GenerateReadWriteVersionList();
                    return true;
                }

                return false;
            }
            catch (Exception exception)
            {
                if (ResourceApplyFailure != null)
                {
                    ResourceApplyFailure(applyInfo.ResourceName, applyingResourcePackPath, exception.ToString());
                }

                return false;
            }
            finally
            {
                applyingResourcePackStream.Position = position;
            }
        }

        private void GenerateReadWriteVersionList()
        {
            if (File.Exists(readWriteVersionListFileName))
            {
                if (File.Exists(readWriteVersionListBackupFileName))
                {
                    File.Delete(readWriteVersionListBackupFileName);
                }

                File.Move(readWriteVersionListFileName, readWriteVersionListBackupFileName);
            }

            currentGenerateReadWriteVersionListLength = 0;
            
            FileStream fileStream = null;
            try
            {
                fileStream = new FileStream(readWriteVersionListFileName, FileMode.Create, FileAccess.Write);
                LocalVersionResource[] resources = resourceManager.readWriteResourceInfos.Count > 0
                    ? new LocalVersionResource[resourceManager.readWriteResourceInfos.Count]
                    : null;
                if (resources != null)
                {
                    int index = 0;
                    foreach (KeyValuePair<ResourceName, ReadWriteResourceInfo> i in resourceManager
                        .readWriteResourceInfos)
                    {
                        resources[index] = new LocalVersionResource(i.Key.Name, i.Key.Variant, i.Key.Extension, (byte) i.Value.LoadType, i.Value.Length, i.Value.HashCode);
                        if (i.Value.UseFileSystem)
                        {
                            List<int> resourceIndexes = null;
                            if (!cachedFileSystemsForGenerateReadWriteVersionList.TryGetValue(i.Value.FileSystemName,
                                out resourceIndexes))
                            {
                                resourceIndexes = new List<int>();
                                cachedFileSystemsForGenerateReadWriteVersionList.Add(i.Value.FileSystemName, resourceIndexes);
                            }

                            resourceIndexes.Add(index);
                        }

                        index++;
                    }
                }

                LocalVersionFileSystem[] fileSystems = cachedFileSystemsForGenerateReadWriteVersionList.Count > 0
                    ? new LocalVersionFileSystem[cachedFileSystemsForGenerateReadWriteVersionList.Count]
                    : null;
                if (fileSystems != null)
                {
                    int index = 0;
                    foreach (KeyValuePair<string, List<int>> i in cachedFileSystemsForGenerateReadWriteVersionList)
                    {
                        fileSystems[index++] = new LocalVersionFileSystem(i.Key, i.Value.ToArray());
                        i.Value.Clear();
                    }
                }

                LocalVersionList versionList = new LocalVersionList(resources, fileSystems);
                if (!localVersionListSerializer.Serialize(fileStream, versionList))
                {
                    throw new GameFrameworkException("Serialize read write version list failure.");
                }

                if (fileStream != null)
                {
                    fileStream.Dispose();
                    fileStream = null;
                }

                if (File.Exists(readWriteVersionListBackupFileName))
                {
                    File.Delete(readWriteVersionListBackupFileName);
                }
            }
            catch (Exception exception)
            {
                if (fileStream != null)
                {
                    fileStream.Dispose();
                    fileStream = null;
                }

                if (File.Exists(readWriteVersionListFileName))
                {
                    File.Delete(readWriteVersionListFileName);
                }

                if (File.Exists(readWriteVersionListBackupFileName))
                {
                    File.Move(readWriteVersionListBackupFileName, readWriteVersionListFileName);
                }

                throw new GameFrameworkException(
                    StringUtils.Format("Generate read write version list exception '{}'.", exception.ToString()),
                    exception);
            }
        }

        private void OnDownloadStart(object sender, DownloadStartEventArgs e)
        {
            UpdateInfo updateInfo = e.UserData as UpdateInfo;
            if (updateInfo == null)
            {
                return;
            }

            if (ResourceUpdateStart != null)
            {
                ResourceUpdateStart(updateInfo.ResourceName, e.DownloadPath, e.DownloadUri, (int) e.CurrentLength, updateInfo.ZipLength, updateInfo.RetryCount);
            }
        }

        private void OnDownloadUpdate(object sender, DownloadUpdateEventArgs e)
        {
            UpdateInfo updateInfo = e.UserData as UpdateInfo;
            if (updateInfo == null)
            {
                return;
            }

            if (downloadManager == null)
            {
                throw new GameFrameworkException("You must set download manager first.");
            }

            if (e.CurrentLength > updateInfo.ZipLength)
            {
                downloadManager.RemoveDownload(e.SerialId);
                string downloadFile = StringUtils.Format("{}.download", e.DownloadPath);
                if (File.Exists(downloadFile))
                {
                    File.Delete(downloadFile);
                }

                string errorMessage = StringUtils.Format("When download update, downloaded length is larger than zip length, need '{}', downloaded '{}'.", updateInfo.ZipLength.ToString(), e.CurrentLength.ToString());
                DownloadFailureEventArgs downloadFailureEventArgs = DownloadFailureEventArgs.Create(e.SerialId, e.DownloadPath, e.DownloadUri, errorMessage, e.UserData);
                OnDownloadFailure(this, downloadFailureEventArgs);
                ReferenceCache.Release(downloadFailureEventArgs);
                return;
            }

            if (ResourceUpdateChanged != null)
            {
                ResourceUpdateChanged(updateInfo.ResourceName, e.DownloadPath, e.DownloadUri, (int) e.CurrentLength, updateInfo.ZipLength);
            }
        }

        private void OnDownloadSuccess(object sender, DownloadSuccessEventArgs e)
        {
            UpdateInfo updateInfo = e.UserData as UpdateInfo;
            if (updateInfo == null)
            {
                return;
            }

            using (FileStream fileStream = new FileStream(e.DownloadPath, FileMode.Open, FileAccess.ReadWrite))
            {
                bool zip = updateInfo.Length != updateInfo.ZipLength || updateInfo.HashCode != updateInfo.ZipHashCode;

                int length = (int) fileStream.Length;
                if (length != updateInfo.ZipLength)
                {
                    fileStream.Close();
                    string errorMessage = StringUtils.Format("Resource zip length error, need '{}', downloaded '{}'.", updateInfo.ZipLength.ToString(), length.ToString());
                    DownloadFailureEventArgs downloadFailureEventArgs = DownloadFailureEventArgs.Create(e.SerialId, e.DownloadPath, e.DownloadUri, errorMessage, e.UserData);
                    OnDownloadFailure(this, downloadFailureEventArgs);
                    ReferenceCache.Release(downloadFailureEventArgs);
                    return;
                }

                if (zip)
                {
                    fileStream.Position = 0L;
                    int hashCode = Crc32Utils.GetCrc32(fileStream);
                    if (hashCode != updateInfo.ZipHashCode)
                    {
                        fileStream.Close();
                        string errorMessage = StringUtils.Format("Resource zip hash code error, need '{}', downloaded '{}'.", updateInfo.ZipHashCode.ToString(), hashCode.ToString());
                        DownloadFailureEventArgs downloadFailureEventArgs = DownloadFailureEventArgs.Create(e.SerialId, e.DownloadPath, e.DownloadUri, errorMessage, e.UserData);
                        OnDownloadFailure(this, downloadFailureEventArgs);
                        ReferenceCache.Release(downloadFailureEventArgs);
                        return;
                    }

                    if (resourceManager.decompressCachedStream == null)
                    {
                        resourceManager.decompressCachedStream = new MemoryStream();
                    }

                    try
                    {
                        fileStream.Position = 0L;
                        resourceManager.decompressCachedStream.Position = 0L;
                        resourceManager.decompressCachedStream.SetLength(0L);
                        if (!ZipUtils.Decompress(fileStream, resourceManager.decompressCachedStream))
                        {
                            fileStream.Close();
                            string errorMessage = StringUtils.Format("Unable to decompress resource '{}'.", e.DownloadPath);
                            DownloadFailureEventArgs downloadFailureEventArgs = DownloadFailureEventArgs.Create(e.SerialId, e.DownloadPath, e.DownloadUri, errorMessage, e.UserData);
                            OnDownloadFailure(this, downloadFailureEventArgs);
                            ReferenceCache.Release(downloadFailureEventArgs);
                            return;
                        }

                        if (resourceManager.decompressCachedStream.Length != updateInfo.Length)
                        {
                            fileStream.Close();
                            string errorMessage = StringUtils.Format("Resource length error, need '{}', downloaded '{}'.", updateInfo.Length.ToString(), resourceManager.decompressCachedStream.Length.ToString());
                            DownloadFailureEventArgs downloadFailureEventArgs = DownloadFailureEventArgs.Create(e.SerialId, e.DownloadPath, e.DownloadUri, errorMessage, e.UserData);
                            OnDownloadFailure(this, downloadFailureEventArgs);
                            ReferenceCache.Release(downloadFailureEventArgs);
                            return;
                        }

                        fileStream.Position = 0L;
                        fileStream.SetLength(0L);
                        fileStream.Write(resourceManager.decompressCachedStream.GetBuffer(), 0,
                            (int) resourceManager.decompressCachedStream.Length);
                    }
                    catch (Exception exception)
                    {
                        fileStream.Close();
                        string errorMessage = StringUtils.Format("Unable to decompress resource '{}' with error message '{}'.", e.DownloadPath, exception.ToString());
                        DownloadFailureEventArgs downloadFailureEventArgs = DownloadFailureEventArgs.Create(e.SerialId, e.DownloadPath, e.DownloadUri, errorMessage, e.UserData);
                        OnDownloadFailure(this, downloadFailureEventArgs);
                        ReferenceCache.Release(downloadFailureEventArgs);
                        return;
                    }
                    finally
                    {
                        resourceManager.decompressCachedStream.Position = 0L;
                        resourceManager.decompressCachedStream.SetLength(0L);
                    }
                }
                else
                {
                    int hashCode = 0;
                    fileStream.Position = 0L;
                    if (updateInfo.LoadType == LoadType.LoadFromMemoryAndQuickDecrypt
                        || updateInfo.LoadType == LoadType.LoadFromMemoryAndDecrypt
                        || updateInfo.LoadType == LoadType.LoadFromBinaryAndQuickDecrypt
                        || updateInfo.LoadType == LoadType.LoadFromBinaryAndDecrypt)
                    {
                        if (updateInfo.LoadType == LoadType.LoadFromMemoryAndQuickDecrypt || updateInfo.LoadType == LoadType.LoadFromBinaryAndQuickDecrypt)
                        {
                            hashCode = Crc32Utils.GetCrc32(fileStream, cachedHashBytes, EncryptionUtils.QuickEncryptLength);
                        }
                        else if (updateInfo.LoadType == LoadType.LoadFromMemoryAndDecrypt || updateInfo.LoadType == LoadType.LoadFromBinaryAndDecrypt)
                        {
                            hashCode = Crc32Utils.GetCrc32(fileStream, cachedHashBytes, length);
                        }

                        Array.Clear(cachedHashBytes, 0, CachedHashBytesLength);
                    }
                    else
                    {
                        hashCode = Crc32Utils.GetCrc32(fileStream);
                    }

                    if (hashCode != updateInfo.HashCode)
                    {
                        fileStream.Close();
                        string errorMessage = StringUtils.Format("Resource hash code error, need '{}', downloaded '{}'.", updateInfo.HashCode.ToString(), hashCode.ToString());
                        DownloadFailureEventArgs downloadFailureEventArgs = DownloadFailureEventArgs.Create(e.SerialId, e.DownloadPath, e.DownloadUri, errorMessage, e.UserData);
                        OnDownloadFailure(this, downloadFailureEventArgs);
                        ReferenceCache.Release(downloadFailureEventArgs);
                        return;
                    }
                }
            }

            if (updateInfo.UseFileSystem)
            {
                IFileSystem fileSystem = resourceManager.GetFileSystem(updateInfo.FileSystemName, false);
                bool retVal = fileSystem.WriteFile(updateInfo.ResourceName.FullName, updateInfo.ResourcePath);
                if (File.Exists(updateInfo.ResourcePath))
                {
                    File.Delete(updateInfo.ResourcePath);
                }

                if (!retVal)
                {
                    string errorMessage = StringUtils.Format("Write resource to file system '{}' error.", fileSystem.FullPath);
                    DownloadFailureEventArgs downloadFailureEventArgs = DownloadFailureEventArgs.Create(e.SerialId, e.DownloadPath, e.DownloadUri, errorMessage, e.UserData);
                    OnDownloadFailure(this, downloadFailureEventArgs);
                    return;
                }
            }

            updatingCount--;
            resourceManager.resourceInfos[updateInfo.ResourceName].MarkReady();
            resourceManager.readWriteResourceInfos.Add(updateInfo.ResourceName, new ReadWriteResourceInfo(updateInfo.FileSystemName, updateInfo.LoadType, updateInfo.Length, updateInfo.HashCode));
            currentGenerateReadWriteVersionListLength += updateInfo.ZipLength;
            if (updatingCount <= 0 ||
                currentGenerateReadWriteVersionListLength >= generateReadWriteVersionListLength)
            {
                GenerateReadWriteVersionList();
            }

            if (ResourceUpdateSuccess != null)
            {
                ResourceUpdateSuccess(updateInfo.ResourceName, e.DownloadPath, e.DownloadUri, updateInfo.Length, updateInfo.ZipLength);
            }
        }

        private void OnDownloadFailure(object sender, DownloadFailureEventArgs e)
        {
            UpdateInfo updateInfo = e.UserData as UpdateInfo;
            if (updateInfo == null)
            {
                return;
            }

            if (File.Exists(e.DownloadPath))
            {
                File.Delete(e.DownloadPath);
            }

            updatingCount--;

            if (ResourceUpdateFailure != null)
            {
                ResourceUpdateFailure(updateInfo.ResourceName, e.DownloadUri, updateInfo.RetryCount, updateRetryCount, e.ErrorMessage);
            }

            if (updateInfo.RetryCount < updateRetryCount)
            {
                updateInfo.RetryCount++;
                updateWaitingInfo.Add(updateInfo);
            }
            else
            {
                failureFlag = true;
                updateInfo.RetryCount = 0;
                updateCandidateInfo.Add(updateInfo.ResourceName, updateInfo);
            }
        }
    }
}