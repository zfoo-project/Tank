using System.IO;
using Spring.Util;
using Summer.Editor.ResourceBuilder.Model;
using UnityEditor;
using UnityEngine;

namespace Summer.Editor.ResourceBuilder.Handler
{
    public sealed class ResourceBuildEventHandler : IBuildEventHandler
    {
        public bool ContinueOnFailure
        {
            get { return false; }
        }

        public void OnPreprocessAllPlatforms(string productName, string companyName, string gameIdentifier,
            string unityVersion, string applicableGameVersion, int internalResourceVersion, BuildAssetBundleOptions buildOptions, bool zip,
            string outputDirectory, string workingPath, bool outputPackageSelected, string outputPackagePath, bool outputFullSelected, string outputFullPath, bool outputPackedSelected, string outputPackedPath, string buildReportPath)
        {
            string streamingAssetsPath = PathUtils.GetRegularPath(Path.Combine(Application.dataPath, "StreamingAssets"));
            FileUtils.DeleteFile(streamingAssetsPath, true);
        }

        public void OnPostprocessAllPlatforms(string productName, string companyName, string gameIdentifier,
            string unityVersion, string applicableGameVersion, int internalResourceVersion, BuildAssetBundleOptions buildOptions, bool zip,
            string outputDirectory, string workingPath, bool outputPackageSelected, string outputPackagePath, bool outputFullSelected, string outputFullPath, bool outputPackedSelected, string outputPackedPath, string buildReportPath)
        {
        }

        public void OnPreprocessPlatform(Platform platform, string workingPath, bool outputPackageSelected, string outputPackagePath, bool outputFullSelected, string outputFullPath, bool outputPackedSelected, string outputPackedPath)
        {
        }

        public void OnBuildAssetBundlesComplete(Platform platform, string workingPath, bool outputPackageSelected, string outputPackagePath, bool outputFullSelected, string outputFullPath, bool outputPackedSelected, string outputPackedPath,
            AssetBundleManifest assetBundleManifest)
        {
        }

        public void OnOutputUpdatableVersionListData(Platform platform, string versionListPath, int versionListLength, int versionListHashCode, int versionListZipLength, int versionListZipHashCode)
        {
        }

        public void OnPostprocessPlatform(Platform platform, string workingPath, bool outputPackageSelected, string outputPackagePath, bool outputFullSelected, string outputFullPath, bool outputPackedSelected, string outputPackedPath,
            bool isSuccess)
        {
            if (!outputPackageSelected)
            {
                return;
            }

            string streamingAssetsPath = PathUtils.GetRegularPath(Path.Combine(Application.dataPath, "StreamingAssets"));
            string[] fileNames = Directory.GetFiles(outputPackagePath, "*", SearchOption.AllDirectories);
            foreach (string fileName in fileNames)
            {
                string destFileName = PathUtils.GetRegularPath(Path.Combine(streamingAssetsPath, fileName.Substring(outputPackagePath.Length)));
                FileInfo destFileInfo = new FileInfo(destFileName);
                if (!destFileInfo.Directory.Exists)
                {
                    destFileInfo.Directory.Create();
                }

                File.Copy(fileName, destFileName);
            }
        }
    }
}