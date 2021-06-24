using System.Collections.Generic;
using System.IO;
using Spring.Util;
using Spring.Util.Math;
using Spring.Util.Security;
using Spring.Util.Zip;
using Summer.Base.Model;
using Summer.Editor.ResourceBuilder;
using Summer.Editor.ResourceBuilder.Model;
using Summer.Resource.Model.ResourcePackVersion;
using Summer.Resource.Model.UpdatableVersion;
using UnityEditor;
using UnityEngine;

namespace Summer.Editor.ResourcePackBuilder
{
    public sealed class ResourcePackBuilderController
    {
        private const string DefaultResourcePackName = "GameFrameworkResourcePack";
        private const string DefaultExtension = "dat";
        private static readonly string[] EmptyStringArray = new string[0];

        private static readonly UpdatableVersionResource[] EmptyResourceArray = new UpdatableVersionResource[0];

        private readonly UpdatableVersionListSerializer updatableVersionListSerializer;
        private readonly ResourcePackVersionListSerializer resourcePackVersionListSerializer;

        public ResourcePackBuilderController()
        {
            updatableVersionListSerializer = new UpdatableVersionListSerializer();

            resourcePackVersionListSerializer = new ResourcePackVersionListSerializer();

            ZipUtils.SetZipHelper(new DefaultZipHelper());
            Platform = Platform.Windows;
        }

        public string ProductName
        {
            get { return PlayerSettings.productName; }
        }

        public string CompanyName
        {
            get { return PlayerSettings.companyName; }
        }

        public string GameIdentifier
        {
            get { return PlayerSettings.applicationIdentifier; }
        }

        public string UnityVersion
        {
            get { return Application.unityVersion; }
        }

        public string ApplicableGameVersion
        {
            get { return Application.version; }
        }

        public string WorkingDirectory { get; set; }

        public bool IsValidWorkingDirectory
        {
            get
            {
                if (string.IsNullOrEmpty(WorkingDirectory))
                {
                    return false;
                }

                if (!Directory.Exists(WorkingDirectory))
                {
                    return false;
                }

                return true;
            }
        }

        public Platform Platform { get; set; }

        public int LengthLimit { get; set; }

        public string SourcePath
        {
            get
            {
                if (!IsValidWorkingDirectory)
                {
                    return string.Empty;
                }

                return PathUtils.GetRegularPath(new DirectoryInfo(StringUtils.Format("{}/Full/", WorkingDirectory))
                    .FullName);
            }
        }

        public string SourcePathForDisplay
        {
            get
            {
                if (!IsValidWorkingDirectory)
                {
                    return string.Empty;
                }

                return PathUtils.GetRegularPath(
                    new DirectoryInfo(StringUtils.Format("{}/Full/*/{}/", WorkingDirectory, Platform.ToString()))
                        .FullName);
            }
        }

        public string OutputPath
        {
            get
            {
                if (!IsValidWorkingDirectory)
                {
                    return string.Empty;
                }

                return PathUtils.GetRegularPath(new DirectoryInfo(StringUtils.Format("{}/ResourcePack/{}/",
                    WorkingDirectory, Platform.ToString())).FullName);
            }
        }

        public event GameFrameworkAction<int> OnBuildResourcePacksStarted = null;

        public event GameFrameworkAction<int, int> OnBuildResourcePacksCompleted = null;

        public event GameFrameworkAction<int, int, string, string> OnBuildResourcePackSuccess = null;

        public event GameFrameworkAction<int, int, string, string> OnBuildResourcePackFailure = null;

        public string[] GetVersionNames()
        {
            if (Platform == Platform.Undefined || !IsValidWorkingDirectory)
            {
                return EmptyStringArray;
            }

            string platformName = Platform.ToString();
            DirectoryInfo sourceDirectoryInfo = new DirectoryInfo(SourcePath);
            if (!sourceDirectoryInfo.Exists)
            {
                return EmptyStringArray;
            }

            List<string> versionNames = new List<string>();
            foreach (DirectoryInfo directoryInfo in sourceDirectoryInfo.GetDirectories())
            {
                string[] splitedVersionNames = directoryInfo.Name.Split('_');
                if (splitedVersionNames.Length < 2)
                {
                    continue;
                }

                bool invalid = false;
                int value = 0;
                for (int i = 0; i < splitedVersionNames.Length; i++)
                {
                    if (!int.TryParse(splitedVersionNames[i], out value))
                    {
                        invalid = true;
                        break;
                    }
                }

                if (invalid)
                {
                    continue;
                }

                DirectoryInfo platformDirectoryInfo =
                    new DirectoryInfo(Path.Combine(directoryInfo.FullName, platformName));
                if (!platformDirectoryInfo.Exists)
                {
                    continue;
                }

                FileInfo[] versionListFiles =
                    platformDirectoryInfo.GetFiles("GameFrameworkVersion.*.dat", SearchOption.TopDirectoryOnly);
                if (versionListFiles.Length != 1)
                {
                    continue;
                }

                versionNames.Add(directoryInfo.Name);
            }

            versionNames.Sort((x, y) =>
            {
                return int.Parse(x.Substring(x.LastIndexOf('_') + 1))
                    .CompareTo(int.Parse(y.Substring(y.LastIndexOf('_') + 1)));
            });

            return versionNames.ToArray();
        }

        public void BuildResourcePacks(string[] sourceVersions, string targetVersion)
        {
            int count = sourceVersions.Length;
            if (OnBuildResourcePacksStarted != null)
            {
                OnBuildResourcePacksStarted(count);
            }

            int successCount = 0;
            for (int i = 0; i < count; i++)
            {
                if (BuildResourcePack(sourceVersions[i], targetVersion))
                {
                    successCount++;
                    if (OnBuildResourcePackSuccess != null)
                    {
                        OnBuildResourcePackSuccess(i, count, sourceVersions[i], targetVersion);
                    }
                }
                else
                {
                    if (OnBuildResourcePackFailure != null)
                    {
                        OnBuildResourcePackFailure(i, count, sourceVersions[i], targetVersion);
                    }
                }
            }

            if (OnBuildResourcePacksCompleted != null)
            {
                OnBuildResourcePacksCompleted(successCount, count);
            }
        }

        public bool BuildResourcePack(string sourceVersion, string targetVersion)
        {
            if (!Directory.Exists(OutputPath))
            {
                Directory.CreateDirectory(OutputPath);
            }

            string defaultResourcePackName = Path.Combine(OutputPath,
                StringUtils.Format("{}.{}", DefaultResourcePackName, DefaultExtension));
            if (File.Exists(defaultResourcePackName))
            {
                File.Delete(defaultResourcePackName);
            }

            UpdatableVersionList sourceUpdatableVersionList = default(UpdatableVersionList);
            if (sourceVersion != null)
            {
                DirectoryInfo sourceDirectoryInfo =
                    new DirectoryInfo(Path.Combine(Path.Combine(SourcePath, sourceVersion), Platform.ToString()));
                FileInfo[] sourceVersionListFiles =
                    sourceDirectoryInfo.GetFiles("GameFrameworkVersion.*.dat", SearchOption.TopDirectoryOnly);
                byte[] sourceVersionListBytes = File.ReadAllBytes(sourceVersionListFiles[0].FullName);
                sourceVersionListBytes = ZipUtils.Decompress(sourceVersionListBytes);
                using (Stream stream = new MemoryStream(sourceVersionListBytes))
                {
                    sourceUpdatableVersionList = updatableVersionListSerializer.Deserialize(stream);
                }
            }

            UpdatableVersionList targetUpdatableVersionList = default(UpdatableVersionList);
            DirectoryInfo targetDirectoryInfo =
                new DirectoryInfo(Path.Combine(Path.Combine(SourcePath, targetVersion), Platform.ToString()));
            FileInfo[] targetVersionListFiles =
                targetDirectoryInfo.GetFiles("GameFrameworkVersion.*.dat", SearchOption.TopDirectoryOnly);
            byte[] targetVersionListBytes = File.ReadAllBytes(targetVersionListFiles[0].FullName);
            targetVersionListBytes = ZipUtils.Decompress(targetVersionListBytes);
            using (Stream stream = new MemoryStream(targetVersionListBytes))
            {
                targetUpdatableVersionList = updatableVersionListSerializer.Deserialize(stream);
            }

            List<ResourcePackVersionResource> resources = new List<ResourcePackVersionResource>();
            UpdatableVersionResource[] sourceResources = sourceUpdatableVersionList.IsValid
                ? sourceUpdatableVersionList.GetResources()
                : EmptyResourceArray;
            UpdatableVersionResource[] targetResources = targetUpdatableVersionList.GetResources();
            long offset = 0L;
            foreach (UpdatableVersionResource targetResource in targetResources)
            {
                bool ready = false;
                foreach (UpdatableVersionResource sourceResource in sourceResources)
                {
                    if (sourceResource.Name != targetResource.Name ||
                        sourceResource.Variant != targetResource.Variant ||
                        sourceResource.Extension != targetResource.Extension)
                    {
                        continue;
                    }

                    if (sourceResource.LoadType == targetResource.LoadType &&
                        sourceResource.Length == targetResource.Length &&
                        sourceResource.HashCode == targetResource.HashCode)
                    {
                        ready = true;
                    }

                    break;
                }

                if (!ready)
                {
                    resources.Add(new ResourcePackVersionResource(targetResource.Name, targetResource.Variant,
                        targetResource.Extension, targetResource.LoadType, offset, targetResource.Length,
                        targetResource.HashCode, targetResource.ZipLength, targetResource.ZipHashCode));
                    offset += targetResource.ZipLength;
                }
            }

            ResourcePackVersionResource[] resourceArray = resources.ToArray();
            using (FileStream fileStream = new FileStream(defaultResourcePackName, FileMode.Create, FileAccess.Write))
            {
                if (!resourcePackVersionListSerializer.Serialize(fileStream,
                    new ResourcePackVersionList(0, 0L, 0, resourceArray)))
                {
                    return false;
                }
            }

            int position = 0;
            int hashCode = 0;
            string targetDirectoryPath = targetDirectoryInfo.FullName;
            using (FileStream fileStream = new FileStream(defaultResourcePackName, FileMode.Open, FileAccess.ReadWrite))
            {
                position = (int) fileStream.Length;
                fileStream.Position = position;
                foreach (ResourcePackVersionResource resource in resourceArray)
                {
                    string resourcePath = Path.Combine(targetDirectoryPath,
                        GetResourceFullName(resource.Name, resource.Variant, resource.HashCode));
                    if (!File.Exists(resourcePath))
                    {
                        return false;
                    }

                    byte[] resourceBytes = File.ReadAllBytes(resourcePath);
                    fileStream.Write(resourceBytes, 0, resourceBytes.Length);
                }

                if (fileStream.Position - position != offset)
                {
                    return false;
                }

                fileStream.Position = position;
                hashCode = Crc32Utils.GetCrc32(fileStream);

                fileStream.Position = 0L;
                if (!resourcePackVersionListSerializer.Serialize(fileStream,
                    new ResourcePackVersionList(position, offset, hashCode, resourceArray)))
                {
                    return false;
                }
            }

            string targetResourcePackName = Path.Combine(OutputPath,
                StringUtils.Format("{}-{}-{}.{}.{}", DefaultResourcePackName,
                    sourceVersion ?? GetNoneVersion(targetVersion), targetVersion, NumberUtils.ToHex(hashCode), DefaultExtension));
            if (File.Exists(targetResourcePackName))
            {
                File.Delete(targetResourcePackName);
            }

            File.Move(defaultResourcePackName, targetResourcePackName);
            return true;
        }

        private string GetNoneVersion(string targetVersion)
        {
            string[] splitedVersionNames = targetVersion.Split('_');
            for (int i = 0; i < splitedVersionNames.Length; i++)
            {
                splitedVersionNames[i] = "0";
            }

            return string.Join("_", splitedVersionNames);
        }

        private string GetResourceFullName(string name, string variant, int hashCode)
        {
            return !string.IsNullOrEmpty(variant)
                ? StringUtils.Format("{}.{}.{}.{}", name, variant, NumberUtils.ToHex(hashCode), DefaultExtension)
                : StringUtils.Format("{}.{}.{}", name, NumberUtils.ToHex(hashCode), DefaultExtension);
        }
    }
}