using System;
using System.IO;
using Spring.Collection.Reference;
using Spring.Core;
using Spring.Logger;
using Spring.Util;
using Spring.Util.Math;
using Spring.Util.Security;
using Spring.Util.Zip;
using Summer.Base.Model;
using Summer.Download;
using Summer.Download.Model.Eve;
using Summer.Resource.Model.Constant;
using Summer.Resource.Model.UpdatableVersion;

namespace Summer.Resource.Manager
{
    /// <summary>
    /// 版本资源列表处理器。
    /// </summary>
    public sealed class VersionListProcessor
    {
        [Autowired]
        private ResourceManager resourceManager;

        [Autowired]
        private IDownloadManager downloadManager;

        [Autowired]
        public UpdatableVersionListSerializer updatableVersionListSerializer;

        private int versionListLength;
        private int versionListHashCode;
        private int versionListZipLength;
        private int versionListZipHashCode;

        public GameFrameworkAction<string, string> VersionListUpdateSuccess;
        public GameFrameworkAction<string, string> VersionListUpdateFailure;


        [BeforePostConstruct]
        public void Init()
        {
            downloadManager.DownloadSuccess += OnDownloadSuccess;
            downloadManager.DownloadFailure += OnDownloadFailure;
        }

        /// <summary>
        /// 关闭并清理版本资源列表处理器。
        /// </summary>
        public void Shutdown()
        {
            if (downloadManager != null)
            {
                downloadManager.DownloadSuccess -= OnDownloadSuccess;
                downloadManager.DownloadFailure -= OnDownloadFailure;
            }
        }


        /// <summary>
        /// 检查版本资源列表。
        /// </summary>
        /// <param name="latestInternalResourceVersion">最新的内部资源版本号。</param>
        /// <returns>检查版本资源列表结果。</returns>
        public CheckVersionListResult CheckVersionList(int latestInternalResourceVersion)
        {
            if (string.IsNullOrEmpty(resourceManager.readWritePath))
            {
                throw new GameFrameworkException("Read-write path is invalid.");
            }

            string versionListFileName = PathUtils.GetRegularPath(Path.Combine(resourceManager.readWritePath, ResourceManager.RemoteVersionListFileName));
            if (!File.Exists(versionListFileName))
            {
                return CheckVersionListResult.NeedUpdate;
            }

            int internalResourceVersion = 0;
            FileStream fileStream = null;
            try
            {
                fileStream = new FileStream(versionListFileName, FileMode.Open, FileAccess.Read);
                object internalResourceVersionObject = null;
                if (!updatableVersionListSerializer.TryGetValue(fileStream, "InternalResourceVersion", out internalResourceVersionObject))
                {
                    return CheckVersionListResult.NeedUpdate;
                }

                internalResourceVersion = (int) internalResourceVersionObject;
            }
            catch
            {
                return CheckVersionListResult.NeedUpdate;
            }
            finally
            {
                if (fileStream != null)
                {
                    fileStream.Dispose();
                    fileStream = null;
                }
            }

            if (internalResourceVersion != latestInternalResourceVersion)
            {
                return CheckVersionListResult.NeedUpdate;
            }

            return CheckVersionListResult.Updated;
        }

        /// <summary>
        /// 更新版本资源列表。
        /// </summary>
        /// <param name="versionListLength">版本资源列表大小。</param>
        /// <param name="versionListHashCode">版本资源列表哈希值。</param>
        /// <param name="versionListZipLength">版本资源列表压缩后大小。</param>
        /// <param name="versionListZipHashCode">版本资源列表压缩后哈希值。</param>
        public void UpdateVersionList(int versionListLength, int versionListHashCode, int versionListZipLength, int versionListZipHashCode)
        {
            if (downloadManager == null)
            {
                throw new GameFrameworkException("You must set download manager first.");
            }

            this.versionListLength = versionListLength;
            this.versionListHashCode = versionListHashCode;
            this.versionListZipLength = versionListZipLength;
            this.versionListZipHashCode = versionListZipHashCode;
            string localVersionListFilePath = PathUtils.GetRegularPath(Path.Combine(resourceManager.readWritePath, ResourceManager.RemoteVersionListFileName));
            int dotPosition = ResourceManager.RemoteVersionListFileName.LastIndexOf('.');
            string latestVersionListFullNameWithCrc32 = StringUtils.Format("{}.{}.{}"
                , ResourceManager.RemoteVersionListFileName.Substring(0, dotPosition), NumberUtils.ToHex(this.versionListHashCode), ResourceManager.RemoteVersionListFileName.Substring(dotPosition + 1));
            var downloadUrl = PathUtils.GetRemotePath(Path.Combine(resourceManager.updatePrefixUri, latestVersionListFullNameWithCrc32));
            downloadManager.AddDownload(localVersionListFilePath, downloadUrl, this);
            Log.Info("开始从[{}]下载VersionList文件", downloadUrl);
        }

        private void OnDownloadSuccess(object sender, DownloadSuccessEventArgs e)
        {
            VersionListProcessor versionListProcessor = e.UserData as VersionListProcessor;
            if (versionListProcessor == null || versionListProcessor != this)
            {
                return;
            }

            using (FileStream fileStream = new FileStream(e.DownloadPath, FileMode.Open, FileAccess.ReadWrite))
            {
                int length = (int) fileStream.Length;
                if (length != versionListZipLength)
                {
                    fileStream.Close();
                    string errorMessage = StringUtils.Format("Latest version list zip length error, need '{}', downloaded '{}'.",
                        versionListZipLength.ToString(), length.ToString());
                    DownloadFailureEventArgs downloadFailureEventArgs = DownloadFailureEventArgs.Create(e.SerialId, e.DownloadPath, e.DownloadUri, errorMessage, e.UserData);
                    OnDownloadFailure(this, downloadFailureEventArgs);
                    ReferenceCache.Release(downloadFailureEventArgs);
                    return;
                }

                fileStream.Position = 0L;
                int hashCode = Crc32Utils.GetCrc32(fileStream);
                if (hashCode != versionListZipHashCode)
                {
                    fileStream.Close();
                    string errorMessage = StringUtils.Format("Latest version list zip hash code error, need '{}', downloaded '{}'.", versionListZipHashCode.ToString(), hashCode.ToString());
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
                        string errorMessage = StringUtils.Format("Unable to decompress latest version list '{}'.", e.DownloadPath);
                        DownloadFailureEventArgs downloadFailureEventArgs = DownloadFailureEventArgs.Create(e.SerialId, e.DownloadPath, e.DownloadUri, errorMessage, e.UserData);
                        OnDownloadFailure(this, downloadFailureEventArgs);
                        ReferenceCache.Release(downloadFailureEventArgs);
                        return;
                    }

                    if (resourceManager.decompressCachedStream.Length != versionListLength)
                    {
                        fileStream.Close();
                        string errorMessage = StringUtils.Format("Latest version list length error, need '{}', downloaded '{}'.", versionListLength.ToString(),
                            resourceManager.decompressCachedStream.Length.ToString());
                        DownloadFailureEventArgs downloadFailureEventArgs = DownloadFailureEventArgs.Create(e.SerialId, e.DownloadPath, e.DownloadUri, errorMessage, e.UserData);
                        OnDownloadFailure(this, downloadFailureEventArgs);
                        ReferenceCache.Release(downloadFailureEventArgs);
                        return;
                    }

                    fileStream.Position = 0L;
                    fileStream.SetLength(0L);
                    fileStream.Write(resourceManager.decompressCachedStream.GetBuffer(), 0, (int) resourceManager.decompressCachedStream.Length);
                }
                catch (Exception exception)
                {
                    fileStream.Close();
                    string errorMessage = StringUtils.Format("Unable to decompress latest version list '{}' with error message '{}'.", e.DownloadPath, exception.ToString());
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

            if (VersionListUpdateSuccess != null)
            {
                VersionListUpdateSuccess(e.DownloadPath, e.DownloadUri);
            }
        }

        private void OnDownloadFailure(object sender, DownloadFailureEventArgs e)
        {
            VersionListProcessor versionListProcessor = e.UserData as VersionListProcessor;
            if (versionListProcessor == null || versionListProcessor != this)
            {
                return;
            }

            if (File.Exists(e.DownloadPath))
            {
                File.Delete(e.DownloadPath);
            }

            if (VersionListUpdateFailure != null)
            {
                VersionListUpdateFailure(e.DownloadUri, e.ErrorMessage);
            }
        }
    }
}