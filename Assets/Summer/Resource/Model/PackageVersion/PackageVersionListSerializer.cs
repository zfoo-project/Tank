using System;
using System.Collections.Generic;
using System.IO;
using Spring.Core;
using Spring.Util;

namespace Summer.Resource.Model.PackageVersion
{
    /// <summary>
    /// 单机模式版本资源列表序列化器。
    /// </summary>
    [Bean]
    public sealed class PackageVersionListSerializer : GameFrameworkSerializer<PackageVersionList>
    {
        private static readonly byte[] Header = new byte[] {(byte) 'G', (byte) 'F', (byte) 'P'};

        /// <summary>
        /// 获取单机模式版本资源列表头标识。
        /// </summary>
        /// <returns>单机模式版本资源列表头标识。</returns>
        protected override byte[] GetHeader()
        {
            return Header;
        }

        public PackageVersionListSerializer()
        {
            RegisterDeserializeCallback(0, PackageVersionListDeserializeCallback_V0);
            RegisterDeserializeCallback(1, PackageVersionListDeserializeCallback_V1);
            RegisterDeserializeCallback(2, PackageVersionListDeserializeCallback_V2);
            RegisterSerializeCallback(0, PackageVersionListSerializeCallback_V0);
            RegisterSerializeCallback(1, PackageVersionListSerializeCallback_V1);
            RegisterSerializeCallback(2, PackageVersionListSerializeCallback_V2);
        }

        /// <summary>
        /// 反序列化单机模式版本资源列表（版本 0）回调函数。
        /// </summary>
        /// <param name="binaryReader">指定流。</param>
        /// <returns>反序列化的单机模式版本资源列表（版本 0）。</returns>
        public static PackageVersionList PackageVersionListDeserializeCallback_V0(BinaryReader binaryReader)
        {
            byte[] encryptBytes = binaryReader.ReadBytes(CachedHashBytesLength);
            string applicableGameVersion = binaryReader.ReadEncryptedString(encryptBytes);
            int internalResourceVersion = binaryReader.ReadInt32();
            int assetCount = binaryReader.ReadInt32();
            PackageVersionAsset[] assets = assetCount > 0 ? new PackageVersionAsset[assetCount] : null;
            int resourceCount = binaryReader.ReadInt32();
            PackageVersionResource[] resources = resourceCount > 0 ? new PackageVersionResource[resourceCount] : null;
            string[][] resourceToAssetNames = new string[resourceCount][];
            List<KeyValuePair<string, string[]>> assetNameToDependencyAssetNames = new List<KeyValuePair<string, string[]>>(assetCount);
            for (int i = 0; i < resourceCount; i++)
            {
                string name = binaryReader.ReadEncryptedString(encryptBytes);
                string variant = binaryReader.ReadEncryptedString(encryptBytes);
                byte loadType = binaryReader.ReadByte();
                int length = binaryReader.ReadInt32();
                int hashCode = binaryReader.ReadInt32();
                ConverterUtils.GetBytes(hashCode, CachedHashBytes);

                int assetNameCount = binaryReader.ReadInt32();
                string[] assetNames = new string[assetNameCount];
                for (int j = 0; j < assetNameCount; j++)
                {
                    assetNames[j] = binaryReader.ReadEncryptedString(CachedHashBytes);
                    int dependencyAssetNameCount = binaryReader.ReadInt32();
                    string[] dependencyAssetNames = dependencyAssetNameCount > 0 ? new string[dependencyAssetNameCount] : null;
                    for (int k = 0; k < dependencyAssetNameCount; k++)
                    {
                        dependencyAssetNames[k] = binaryReader.ReadEncryptedString(CachedHashBytes);
                    }

                    assetNameToDependencyAssetNames.Add(new KeyValuePair<string, string[]>(assetNames[j], dependencyAssetNames));
                }

                resourceToAssetNames[i] = assetNames;
                resources[i] = new PackageVersionResource(name, variant, null, loadType, length, hashCode, assetNameCount > 0 ? new int[assetNameCount] : null);
            }

            assetNameToDependencyAssetNames.Sort(AssetNameToDependencyAssetNamesComparer);
            Array.Clear(CachedHashBytes, 0, CachedHashBytesLength);
            int index = 0;
            foreach (KeyValuePair<string, string[]> i in assetNameToDependencyAssetNames)
            {
                if (i.Value != null)
                {
                    int[] dependencyAssetIndexes = new int[i.Value.Length];
                    for (int j = 0; j < i.Value.Length; j++)
                    {
                        dependencyAssetIndexes[j] = GetAssetNameIndex(assetNameToDependencyAssetNames, i.Value[j]);
                    }

                    assets[index++] = new PackageVersionAsset(i.Key, dependencyAssetIndexes);
                }
                else
                {
                    assets[index++] = new PackageVersionAsset(i.Key, null);
                }
            }

            for (int i = 0; i < resources.Length; i++)
            {
                int[] assetIndexes = resources[i].GetAssetIndexes();
                for (int j = 0; j < assetIndexes.Length; j++)
                {
                    assetIndexes[j] = GetAssetNameIndex(assetNameToDependencyAssetNames, resourceToAssetNames[i][j]);
                }
            }

            int resourceGroupCount = binaryReader.ReadInt32();
            PackageVersionResourceGroup[] resourceGroups = resourceGroupCount > 0 ? new PackageVersionResourceGroup[resourceGroupCount] : null;
            for (int i = 0; i < resourceGroupCount; i++)
            {
                string name = binaryReader.ReadEncryptedString(encryptBytes);
                int resourceIndexCount = binaryReader.ReadInt32();
                int[] resourceIndexes = resourceIndexCount > 0 ? new int[resourceIndexCount] : null;
                for (int j = 0; j < resourceIndexCount; j++)
                {
                    resourceIndexes[j] = binaryReader.ReadUInt16();
                }

                resourceGroups[i] = new PackageVersionResourceGroup(name, resourceIndexes);
            }

            return new PackageVersionList(applicableGameVersion, internalResourceVersion, assets, resources, null, resourceGroups);
        }

        /// <summary>
        /// 反序列化单机模式版本资源列表（版本 1）回调函数。
        /// </summary>
        /// <param name="binaryReader">指定流。</param>
        /// <returns>反序列化的单机模式版本资源列表（版本 1）。</returns>
        public static PackageVersionList PackageVersionListDeserializeCallback_V1(BinaryReader binaryReader)
        {
            byte[] encryptBytes = binaryReader.ReadBytes(CachedHashBytesLength);
            string applicableGameVersion = binaryReader.ReadEncryptedString(encryptBytes);
            int internalResourceVersion = binaryReader.Read7BitEncodedInt32();
            int assetCount = binaryReader.Read7BitEncodedInt32();
            PackageVersionAsset[] assets = assetCount > 0 ? new PackageVersionAsset[assetCount] : null;
            for (int i = 0; i < assetCount; i++)
            {
                string name = binaryReader.ReadEncryptedString(encryptBytes);
                int dependencyAssetCount = binaryReader.Read7BitEncodedInt32();
                int[] dependencyAssetIndexes = dependencyAssetCount > 0 ? new int[dependencyAssetCount] : null;
                for (int j = 0; j < dependencyAssetCount; j++)
                {
                    dependencyAssetIndexes[j] = binaryReader.Read7BitEncodedInt32();
                }

                assets[i] = new PackageVersionAsset(name, dependencyAssetIndexes);
            }

            int resourceCount = binaryReader.Read7BitEncodedInt32();
            PackageVersionResource[] resources = resourceCount > 0 ? new PackageVersionResource[resourceCount] : null;
            for (int i = 0; i < resourceCount; i++)
            {
                string name = binaryReader.ReadEncryptedString(encryptBytes);
                string variant = binaryReader.ReadEncryptedString(encryptBytes);
                string extension = binaryReader.ReadEncryptedString(encryptBytes) ?? DefaultExtension;
                byte loadType = binaryReader.ReadByte();
                int length = binaryReader.Read7BitEncodedInt32();
                int hashCode = binaryReader.ReadInt32();
                int assetIndexCount = binaryReader.Read7BitEncodedInt32();
                int[] assetIndexes = assetIndexCount > 0 ? new int[assetIndexCount] : null;
                for (int j = 0; j < assetIndexCount; j++)
                {
                    assetIndexes[j] = binaryReader.Read7BitEncodedInt32();
                }

                resources[i] = new PackageVersionResource(name, variant, extension, loadType, length, hashCode, assetIndexes);
            }

            int resourceGroupCount = binaryReader.Read7BitEncodedInt32();
            PackageVersionResourceGroup[] resourceGroups = resourceGroupCount > 0 ? new PackageVersionResourceGroup[resourceGroupCount] : null;
            for (int i = 0; i < resourceGroupCount; i++)
            {
                string name = binaryReader.ReadEncryptedString(encryptBytes);
                int resourceIndexCount = binaryReader.Read7BitEncodedInt32();
                int[] resourceIndexes = resourceIndexCount > 0 ? new int[resourceIndexCount] : null;
                for (int j = 0; j < resourceIndexCount; j++)
                {
                    resourceIndexes[j] = binaryReader.Read7BitEncodedInt32();
                }

                resourceGroups[i] = new PackageVersionResourceGroup(name, resourceIndexes);
            }

            return new PackageVersionList(applicableGameVersion, internalResourceVersion, assets, resources, null, resourceGroups);
        }

        /// <summary>
        /// 反序列化单机模式版本资源列表（版本 2）回调函数。
        /// </summary>
        /// <param name="binaryReader">指定流。</param>
        /// <returns>反序列化的单机模式版本资源列表（版本 2）。</returns>
        public static PackageVersionList PackageVersionListDeserializeCallback_V2(BinaryReader binaryReader)
        {
            byte[] encryptBytes = binaryReader.ReadBytes(CachedHashBytesLength);
            string applicableGameVersion = binaryReader.ReadEncryptedString(encryptBytes);
            int internalResourceVersion = binaryReader.Read7BitEncodedInt32();
            int assetCount = binaryReader.Read7BitEncodedInt32();
            PackageVersionAsset[] assets = assetCount > 0 ? new PackageVersionAsset[assetCount] : null;
            for (int i = 0; i < assetCount; i++)
            {
                string name = binaryReader.ReadEncryptedString(encryptBytes);
                int dependencyAssetCount = binaryReader.Read7BitEncodedInt32();
                int[] dependencyAssetIndexes = dependencyAssetCount > 0 ? new int[dependencyAssetCount] : null;
                for (int j = 0; j < dependencyAssetCount; j++)
                {
                    dependencyAssetIndexes[j] = binaryReader.Read7BitEncodedInt32();
                }

                assets[i] = new PackageVersionAsset(name, dependencyAssetIndexes);
            }

            int resourceCount = binaryReader.Read7BitEncodedInt32();
            PackageVersionResource[] resources = resourceCount > 0 ? new PackageVersionResource[resourceCount] : null;
            for (int i = 0; i < resourceCount; i++)
            {
                string name = binaryReader.ReadEncryptedString(encryptBytes);
                string variant = binaryReader.ReadEncryptedString(encryptBytes);
                string extension = binaryReader.ReadEncryptedString(encryptBytes) ?? DefaultExtension;
                byte loadType = binaryReader.ReadByte();
                int length = binaryReader.Read7BitEncodedInt32();
                int hashCode = binaryReader.ReadInt32();
                int assetIndexCount = binaryReader.Read7BitEncodedInt32();
                int[] assetIndexes = assetIndexCount > 0 ? new int[assetIndexCount] : null;
                for (int j = 0; j < assetIndexCount; j++)
                {
                    assetIndexes[j] = binaryReader.Read7BitEncodedInt32();
                }

                resources[i] = new PackageVersionResource(name, variant, extension, loadType, length, hashCode, assetIndexes);
            }

            int fileSystemCount = binaryReader.Read7BitEncodedInt32();
            PackageVersionFileSystem[] fileSystems = fileSystemCount > 0 ? new PackageVersionFileSystem[fileSystemCount] : null;
            for (int i = 0; i < fileSystemCount; i++)
            {
                string name = binaryReader.ReadEncryptedString(encryptBytes);
                int resourceIndexCount = binaryReader.Read7BitEncodedInt32();
                int[] resourceIndexes = resourceIndexCount > 0 ? new int[resourceIndexCount] : null;
                for (int j = 0; j < resourceIndexCount; j++)
                {
                    resourceIndexes[j] = binaryReader.Read7BitEncodedInt32();
                }

                fileSystems[i] = new PackageVersionFileSystem(name, resourceIndexes);
            }

            int resourceGroupCount = binaryReader.Read7BitEncodedInt32();
            PackageVersionResourceGroup[] resourceGroups = resourceGroupCount > 0 ? new PackageVersionResourceGroup[resourceGroupCount] : null;
            for (int i = 0; i < resourceGroupCount; i++)
            {
                string name = binaryReader.ReadEncryptedString(encryptBytes);
                int resourceIndexCount = binaryReader.Read7BitEncodedInt32();
                int[] resourceIndexes = resourceIndexCount > 0 ? new int[resourceIndexCount] : null;
                for (int j = 0; j < resourceIndexCount; j++)
                {
                    resourceIndexes[j] = binaryReader.Read7BitEncodedInt32();
                }

                resourceGroups[i] = new PackageVersionResourceGroup(name, resourceIndexes);
            }

            return new PackageVersionList(applicableGameVersion, internalResourceVersion, assets, resources, fileSystems, resourceGroups);
        }


        /// <summary>
        /// 序列化单机模式版本资源列表（版本 0）回调函数。
        /// </summary>
        /// <param name="binaryWriter">目标流。</param>
        /// <param name="versionList">要序列化的单机模式版本资源列表（版本 0）。</param>
        /// <returns>是否序列化单机模式版本资源列表（版本 0）成功。</returns>
        public static bool PackageVersionListSerializeCallback_V0(BinaryWriter binaryWriter, PackageVersionList versionList)
        {
            if (!versionList.IsValid)
            {
                return false;
            }

            RandomUtils.GetRandomBytes(CachedHashBytes);
            binaryWriter.Write(CachedHashBytes);
            binaryWriter.WriteEncryptedString(versionList.ApplicableGameVersion, CachedHashBytes);
            binaryWriter.Write(versionList.InternalResourceVersion);
            PackageVersionAsset[] assets = versionList.GetAssets();
            binaryWriter.Write(assets.Length);
            PackageVersionResource[] resources = versionList.GetResources();
            binaryWriter.Write(resources.Length);
            foreach (PackageVersionResource resource in resources)
            {
                binaryWriter.WriteEncryptedString(resource.Name, CachedHashBytes);
                binaryWriter.WriteEncryptedString(resource.Variant, CachedHashBytes);
                binaryWriter.Write(resource.LoadType);
                binaryWriter.Write(resource.Length);
                binaryWriter.Write(resource.HashCode);
                int[] assetIndexes = resource.GetAssetIndexes();
                binaryWriter.Write(assetIndexes.Length);
                byte[] hashBytes = new byte[CachedHashBytesLength];
                foreach (int assetIndex in assetIndexes)
                {
                    ConverterUtils.GetBytes(resource.HashCode, hashBytes);
                    PackageVersionAsset packageVersionAsset = assets[assetIndex];
                    binaryWriter.WriteEncryptedString(packageVersionAsset.Name, hashBytes);
                    int[] dependencyAssetIndexes = packageVersionAsset.GetDependencyAssetIndexes();
                    binaryWriter.Write(dependencyAssetIndexes.Length);
                    foreach (int dependencyAssetIndex in dependencyAssetIndexes)
                    {
                        binaryWriter.WriteEncryptedString(assets[dependencyAssetIndex].Name, hashBytes);
                    }
                }
            }

            PackageVersionResourceGroup[] resourceGroups = versionList.GetResourceGroups();
            binaryWriter.Write(resourceGroups.Length);
            foreach (PackageVersionResourceGroup resourceGroup in resourceGroups)
            {
                binaryWriter.WriteEncryptedString(resourceGroup.Name, CachedHashBytes);
                int[] resourceIndexes = resourceGroup.GetResourceIndexes();
                binaryWriter.Write(resourceIndexes.Length);
                foreach (ushort resourceIndex in resourceIndexes)
                {
                    binaryWriter.Write(resourceIndex);
                }
            }

            Array.Clear(CachedHashBytes, 0, CachedHashBytesLength);
            return true;
        }

        /// <summary>
        /// 序列化单机模式版本资源列表（版本 1）回调函数。
        /// </summary>
        /// <param name="binaryWriter">目标流。</param>
        /// <param name="versionList">要序列化的单机模式版本资源列表（版本 1）。</param>
        /// <returns>是否序列化单机模式版本资源列表（版本 1）成功。</returns>
        public static bool PackageVersionListSerializeCallback_V1(BinaryWriter binaryWriter, PackageVersionList versionList)
        {
            if (!versionList.IsValid)
            {
                return false;
            }

            RandomUtils.GetRandomBytes(CachedHashBytes);
            binaryWriter.Write(CachedHashBytes);
            binaryWriter.WriteEncryptedString(versionList.ApplicableGameVersion, CachedHashBytes);
            binaryWriter.Write7BitEncodedInt32(versionList.InternalResourceVersion);
            PackageVersionAsset[] assets = versionList.GetAssets();
            binaryWriter.Write7BitEncodedInt32(assets.Length);
            foreach (PackageVersionAsset asset in assets)
            {
                binaryWriter.WriteEncryptedString(asset.Name, CachedHashBytes);
                int[] dependencyAssetIndexes = asset.GetDependencyAssetIndexes();
                binaryWriter.Write7BitEncodedInt32(dependencyAssetIndexes.Length);
                foreach (int dependencyAssetIndex in dependencyAssetIndexes)
                {
                    binaryWriter.Write7BitEncodedInt32(dependencyAssetIndex);
                }
            }

            PackageVersionResource[] resources = versionList.GetResources();
            binaryWriter.Write7BitEncodedInt32(resources.Length);
            foreach (PackageVersionResource resource in resources)
            {
                binaryWriter.WriteEncryptedString(resource.Name, CachedHashBytes);
                binaryWriter.WriteEncryptedString(resource.Variant, CachedHashBytes);
                binaryWriter.WriteEncryptedString(resource.Extension != DefaultExtension ? resource.Extension : null, CachedHashBytes);
                binaryWriter.Write(resource.LoadType);
                binaryWriter.Write7BitEncodedInt32(resource.Length);
                binaryWriter.Write(resource.HashCode);
                int[] assetIndexes = resource.GetAssetIndexes();
                binaryWriter.Write7BitEncodedInt32(assetIndexes.Length);
                foreach (int assetIndex in assetIndexes)
                {
                    binaryWriter.Write7BitEncodedInt32(assetIndex);
                }
            }

            PackageVersionResourceGroup[] resourceGroups = versionList.GetResourceGroups();
            binaryWriter.Write7BitEncodedInt32(resourceGroups.Length);
            foreach (PackageVersionResourceGroup resourceGroup in resourceGroups)
            {
                binaryWriter.WriteEncryptedString(resourceGroup.Name, CachedHashBytes);
                int[] resourceIndexes = resourceGroup.GetResourceIndexes();
                binaryWriter.Write7BitEncodedInt32(resourceIndexes.Length);
                foreach (int resourceIndex in resourceIndexes)
                {
                    binaryWriter.Write7BitEncodedInt32(resourceIndex);
                }
            }

            Array.Clear(CachedHashBytes, 0, CachedHashBytesLength);
            return true;
        }

        /// <summary>
        /// 序列化单机模式版本资源列表（版本 2）回调函数。
        /// </summary>
        /// <param name="binaryWriter">目标流。</param>
        /// <param name="versionList">要序列化的单机模式版本资源列表（版本 2）。</param>
        /// <returns>是否序列化单机模式版本资源列表（版本 2）成功。</returns>
        public static bool PackageVersionListSerializeCallback_V2(BinaryWriter binaryWriter, PackageVersionList versionList)
        {
            if (!versionList.IsValid)
            {
                return false;
            }

            RandomUtils.GetRandomBytes(CachedHashBytes);
            binaryWriter.Write(CachedHashBytes);
            binaryWriter.WriteEncryptedString(versionList.ApplicableGameVersion, CachedHashBytes);
            binaryWriter.Write7BitEncodedInt32(versionList.InternalResourceVersion);
            PackageVersionAsset[] assets = versionList.GetAssets();
            binaryWriter.Write7BitEncodedInt32(assets.Length);
            foreach (PackageVersionAsset asset in assets)
            {
                binaryWriter.WriteEncryptedString(asset.Name, CachedHashBytes);
                int[] dependencyAssetIndexes = asset.GetDependencyAssetIndexes();
                binaryWriter.Write7BitEncodedInt32(dependencyAssetIndexes.Length);
                foreach (int dependencyAssetIndex in dependencyAssetIndexes)
                {
                    binaryWriter.Write7BitEncodedInt32(dependencyAssetIndex);
                }
            }

            PackageVersionResource[] resources = versionList.GetResources();
            binaryWriter.Write7BitEncodedInt32(resources.Length);
            foreach (PackageVersionResource resource in resources)
            {
                binaryWriter.WriteEncryptedString(resource.Name, CachedHashBytes);
                binaryWriter.WriteEncryptedString(resource.Variant, CachedHashBytes);
                binaryWriter.WriteEncryptedString(resource.Extension != DefaultExtension ? resource.Extension : null, CachedHashBytes);
                binaryWriter.Write(resource.LoadType);
                binaryWriter.Write7BitEncodedInt32(resource.Length);
                binaryWriter.Write(resource.HashCode);
                int[] assetIndexes = resource.GetAssetIndexes();
                binaryWriter.Write7BitEncodedInt32(assetIndexes.Length);
                foreach (int assetIndex in assetIndexes)
                {
                    binaryWriter.Write7BitEncodedInt32(assetIndex);
                }
            }

            PackageVersionFileSystem[] fileSystems = versionList.GetFileSystems();
            binaryWriter.Write7BitEncodedInt32(fileSystems.Length);
            foreach (PackageVersionFileSystem fileSystem in fileSystems)
            {
                binaryWriter.WriteEncryptedString(fileSystem.Name, CachedHashBytes);
                int[] resourceIndexes = fileSystem.GetResourceIndexes();
                binaryWriter.Write7BitEncodedInt32(resourceIndexes.Length);
                foreach (int resourceIndex in resourceIndexes)
                {
                    binaryWriter.Write7BitEncodedInt32(resourceIndex);
                }
            }

            PackageVersionResourceGroup[] resourceGroups = versionList.GetResourceGroups();
            binaryWriter.Write7BitEncodedInt32(resourceGroups.Length);
            foreach (PackageVersionResourceGroup resourceGroup in resourceGroups)
            {
                binaryWriter.WriteEncryptedString(resourceGroup.Name, CachedHashBytes);
                int[] resourceIndexes = resourceGroup.GetResourceIndexes();
                binaryWriter.Write7BitEncodedInt32(resourceIndexes.Length);
                foreach (int resourceIndex in resourceIndexes)
                {
                    binaryWriter.Write7BitEncodedInt32(resourceIndex);
                }
            }

            Array.Clear(CachedHashBytes, 0, CachedHashBytesLength);
            return true;
        }
    }
}