using System;
using System.Collections.Generic;
using System.IO;
using Spring.Core;
using Spring.Util;

namespace Summer.Resource.Model.UpdatableVersion
{
    /// <summary>
    /// 可更新模式版本资源列表序列化器。
    /// </summary>
    [Bean]
    public sealed class UpdatableVersionListSerializer : GameFrameworkSerializer<UpdatableVersionList>
    {
        private static readonly byte[] Header = new byte[] {(byte) 'G', (byte) 'F', (byte) 'U'};


        /// <summary>
        /// 获取可更新模式版本资源列表头标识。
        /// </summary>
        /// <returns>可更新模式版本资源列表头标识。</returns>
        protected override byte[] GetHeader()
        {
            return Header;
        }

        /// <summary>
        /// 初始化可更新模式版本资源列表序列化器的新实例。
        /// </summary>
        public UpdatableVersionListSerializer()
        {
            RegisterDeserializeCallback(0, UpdatableVersionListDeserializeCallback_V0);
            RegisterDeserializeCallback(1, UpdatableVersionListDeserializeCallback_V1);
            RegisterDeserializeCallback(2, UpdatableVersionListDeserializeCallback_V2);

            RegisterSerializeCallback(0, UpdatableVersionListSerializeCallback_V0);
            RegisterSerializeCallback(1, UpdatableVersionListSerializeCallback_V1);
            RegisterSerializeCallback(2, UpdatableVersionListSerializeCallback_V2);

            RegisterTryGetValueCallback(0, UpdatableVersionListTryGetValueCallback_V0);
            RegisterTryGetValueCallback(1, UpdatableVersionListTryGetValueCallback_V1_V2);
            RegisterTryGetValueCallback(2, UpdatableVersionListTryGetValueCallback_V1_V2);
        }

 /// <summary>
        /// 反序列化可更新模式版本资源列表（版本 0）回调函数。
        /// </summary>
        /// <param name="binaryReader">指定流。</param>
        /// <returns>反序列化的可更新模式版本资源列表（版本 0）。</returns>
        public static UpdatableVersionList UpdatableVersionListDeserializeCallback_V0(BinaryReader binaryReader)
        {
            byte[] encryptBytes = binaryReader.ReadBytes(CachedHashBytesLength);
            string applicableGameVersion = binaryReader.ReadEncryptedString(encryptBytes);
            int internalResourceVersion = binaryReader.ReadInt32();
            int assetCount = binaryReader.ReadInt32();
            UpdatableVersionAsset[] assets = assetCount > 0 ? new UpdatableVersionAsset[assetCount] : null;
            int resourceCount = binaryReader.ReadInt32();
            UpdatableVersionResource[] resources = resourceCount > 0 ? new UpdatableVersionResource[resourceCount] : null;
            string[][] resourceToAssetNames = new string[resourceCount][];
            List<KeyValuePair<string, string[]>> assetNameToDependencyAssetNames = new List<KeyValuePair<string, string[]>>(assetCount);
            for (int i = 0; i < resourceCount; i++)
            {
                string name = binaryReader.ReadEncryptedString(encryptBytes);
                string variant = binaryReader.ReadEncryptedString(encryptBytes);
                byte loadType = binaryReader.ReadByte();
                int length = binaryReader.ReadInt32();
                int hashCode = binaryReader.ReadInt32();
                int zipLength = binaryReader.ReadInt32();
                int zipHashCode = binaryReader.ReadInt32();
                ConverterUtils.GetBytes(hashCode, CachedHashBytes);

                int assetNameCount = binaryReader.ReadInt32();
                string[] assetNames = assetNameCount > 0 ? new string[assetNameCount] : null;
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
                resources[i] = new UpdatableVersionResource(name, variant, null, loadType, length, hashCode, zipLength, zipHashCode, assetNameCount > 0 ? new int[assetNameCount] : null);
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

                    assets[index++] = new UpdatableVersionAsset(i.Key, dependencyAssetIndexes);
                }
                else
                {
                    assets[index++] = new UpdatableVersionAsset(i.Key, null);
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
            UpdatableVersionResourceGroup[] resourceGroups = resourceGroupCount > 0 ? new UpdatableVersionResourceGroup[resourceGroupCount] : null;
            for (int i = 0; i < resourceGroupCount; i++)
            {
                string name = binaryReader.ReadEncryptedString(encryptBytes);
                int resourceIndexCount = binaryReader.ReadInt32();
                int[] resourceIndexes = resourceIndexCount > 0 ? new int[resourceIndexCount] : null;
                for (int j = 0; j < resourceIndexCount; j++)
                {
                    resourceIndexes[j] = binaryReader.ReadUInt16();
                }

                resourceGroups[i] = new UpdatableVersionResourceGroup(name, resourceIndexes);
            }

            return new UpdatableVersionList(applicableGameVersion, internalResourceVersion, assets, resources, null, resourceGroups);
        }

        /// <summary>
        /// 反序列化可更新模式版本资源列表（版本 1）回调函数。
        /// </summary>
        /// <param name="binaryReader">指定流。</param>
        /// <returns>反序列化的可更新模式版本资源列表（版本 1）。</returns>
        public static UpdatableVersionList UpdatableVersionListDeserializeCallback_V1(BinaryReader binaryReader)
        {
            byte[] encryptBytes = binaryReader.ReadBytes(CachedHashBytesLength);
            string applicableGameVersion = binaryReader.ReadEncryptedString(encryptBytes);
            int internalResourceVersion = binaryReader.Read7BitEncodedInt32();
            int assetCount = binaryReader.Read7BitEncodedInt32();
            UpdatableVersionAsset[] assets = assetCount > 0 ? new UpdatableVersionAsset[assetCount] : null;
            for (int i = 0; i < assetCount; i++)
            {
                string name = binaryReader.ReadEncryptedString(encryptBytes);
                int dependencyAssetCount = binaryReader.Read7BitEncodedInt32();
                int[] dependencyAssetIndexes = dependencyAssetCount > 0 ? new int[dependencyAssetCount] : null;
                for (int j = 0; j < dependencyAssetCount; j++)
                {
                    dependencyAssetIndexes[j] = binaryReader.Read7BitEncodedInt32();
                }

                assets[i] = new UpdatableVersionAsset(name, dependencyAssetIndexes);
            }

            int resourceCount = binaryReader.Read7BitEncodedInt32();
            UpdatableVersionResource[] resources = resourceCount > 0 ? new UpdatableVersionResource[resourceCount] : null;
            for (int i = 0; i < resourceCount; i++)
            {
                string name = binaryReader.ReadEncryptedString(encryptBytes);
                string variant = binaryReader.ReadEncryptedString(encryptBytes);
                string extension = binaryReader.ReadEncryptedString(encryptBytes) ?? DefaultExtension;
                byte loadType = binaryReader.ReadByte();
                int length = binaryReader.Read7BitEncodedInt32();
                int hashCode = binaryReader.ReadInt32();
                int zipLength = binaryReader.Read7BitEncodedInt32();
                int zipHashCode = binaryReader.ReadInt32();
                int assetIndexCount = binaryReader.Read7BitEncodedInt32();
                int[] assetIndexes = assetIndexCount > 0 ? new int[assetIndexCount] : null;
                for (int j = 0; j < assetIndexCount; j++)
                {
                    assetIndexes[j] = binaryReader.Read7BitEncodedInt32();
                }

                resources[i] = new UpdatableVersionResource(name, variant, extension, loadType, length, hashCode, zipLength, zipHashCode, assetIndexes);
            }

            int resourceGroupCount = binaryReader.Read7BitEncodedInt32();
            UpdatableVersionResourceGroup[] resourceGroups = resourceGroupCount > 0 ? new UpdatableVersionResourceGroup[resourceGroupCount] : null;
            for (int i = 0; i < resourceGroupCount; i++)
            {
                string name = binaryReader.ReadEncryptedString(encryptBytes);
                int resourceIndexCount = binaryReader.Read7BitEncodedInt32();
                int[] resourceIndexes = resourceIndexCount > 0 ? new int[resourceIndexCount] : null;
                for (int j = 0; j < resourceIndexCount; j++)
                {
                    resourceIndexes[j] = binaryReader.Read7BitEncodedInt32();
                }

                resourceGroups[i] = new UpdatableVersionResourceGroup(name, resourceIndexes);
            }

            return new UpdatableVersionList(applicableGameVersion, internalResourceVersion, assets, resources, null, resourceGroups);
        }

        /// <summary>
        /// 反序列化可更新模式版本资源列表（版本 2）回调函数。
        /// </summary>
        /// <param name="binaryReader">指定流。</param>
        /// <returns>反序列化的可更新模式版本资源列表（版本 2）。</returns>
        public static UpdatableVersionList UpdatableVersionListDeserializeCallback_V2(BinaryReader binaryReader)
        {
            byte[] encryptBytes = binaryReader.ReadBytes(CachedHashBytesLength);
            string applicableGameVersion = binaryReader.ReadEncryptedString(encryptBytes);
            int internalResourceVersion = binaryReader.Read7BitEncodedInt32();
            int assetCount = binaryReader.Read7BitEncodedInt32();
            UpdatableVersionAsset[] assets = assetCount > 0 ? new UpdatableVersionAsset[assetCount] : null;
            for (int i = 0; i < assetCount; i++)
            {
                string name = binaryReader.ReadEncryptedString(encryptBytes);
                int dependencyAssetCount = binaryReader.Read7BitEncodedInt32();
                int[] dependencyAssetIndexes = dependencyAssetCount > 0 ? new int[dependencyAssetCount] : null;
                for (int j = 0; j < dependencyAssetCount; j++)
                {
                    dependencyAssetIndexes[j] = binaryReader.Read7BitEncodedInt32();
                }

                assets[i] = new UpdatableVersionAsset(name, dependencyAssetIndexes);
            }

            int resourceCount = binaryReader.Read7BitEncodedInt32();
            UpdatableVersionResource[] resources = resourceCount > 0 ? new UpdatableVersionResource[resourceCount] : null;
            for (int i = 0; i < resourceCount; i++)
            {
                string name = binaryReader.ReadEncryptedString(encryptBytes);
                string variant = binaryReader.ReadEncryptedString(encryptBytes);
                string extension = binaryReader.ReadEncryptedString(encryptBytes) ?? DefaultExtension;
                byte loadType = binaryReader.ReadByte();
                int length = binaryReader.Read7BitEncodedInt32();
                int hashCode = binaryReader.ReadInt32();
                int zipLength = binaryReader.Read7BitEncodedInt32();
                int zipHashCode = binaryReader.ReadInt32();
                int assetIndexCount = binaryReader.Read7BitEncodedInt32();
                int[] assetIndexes = assetIndexCount > 0 ? new int[assetIndexCount] : null;
                for (int j = 0; j < assetIndexCount; j++)
                {
                    assetIndexes[j] = binaryReader.Read7BitEncodedInt32();
                }

                resources[i] = new UpdatableVersionResource(name, variant, extension, loadType, length, hashCode, zipLength, zipHashCode, assetIndexes);
            }

            int fileSystemCount = binaryReader.Read7BitEncodedInt32();
            UpdatableVersionFileSystem[] fileSystems = fileSystemCount > 0 ? new UpdatableVersionFileSystem[fileSystemCount] : null;
            for (int i = 0; i < fileSystemCount; i++)
            {
                string name = binaryReader.ReadEncryptedString(encryptBytes);
                int resourceIndexCount = binaryReader.Read7BitEncodedInt32();
                int[] resourceIndexes = resourceIndexCount > 0 ? new int[resourceIndexCount] : null;
                for (int j = 0; j < resourceIndexCount; j++)
                {
                    resourceIndexes[j] = binaryReader.Read7BitEncodedInt32();
                }

                fileSystems[i] = new UpdatableVersionFileSystem(name, resourceIndexes);
            }

            int resourceGroupCount = binaryReader.Read7BitEncodedInt32();
            UpdatableVersionResourceGroup[] resourceGroups = resourceGroupCount > 0 ? new UpdatableVersionResourceGroup[resourceGroupCount] : null;
            for (int i = 0; i < resourceGroupCount; i++)
            {
                string name = binaryReader.ReadEncryptedString(encryptBytes);
                int resourceIndexCount = binaryReader.Read7BitEncodedInt32();
                int[] resourceIndexes = resourceIndexCount > 0 ? new int[resourceIndexCount] : null;
                for (int j = 0; j < resourceIndexCount; j++)
                {
                    resourceIndexes[j] = binaryReader.Read7BitEncodedInt32();
                }

                resourceGroups[i] = new UpdatableVersionResourceGroup(name, resourceIndexes);
            }

            return new UpdatableVersionList(applicableGameVersion, internalResourceVersion, assets, resources, fileSystems, resourceGroups);
        }
        
          /// <summary>
        /// 序列化可更新模式版本资源列表（版本 0）回调函数。
        /// </summary>
        /// <param name="binaryWriter">目标流。</param>
        /// <param name="versionList">要序列化的可更新模式版本资源列表（版本 0）。</param>
        /// <returns>是否序列化可更新模式版本资源列表（版本 0）成功。</returns>
        public static bool UpdatableVersionListSerializeCallback_V0(BinaryWriter binaryWriter, UpdatableVersionList versionList)
        {
            if (!versionList.IsValid)
            {
                return false;
            }

            RandomUtils.GetRandomBytes(CachedHashBytes);
            binaryWriter.Write(CachedHashBytes);
            binaryWriter.WriteEncryptedString(versionList.ApplicableGameVersion, CachedHashBytes);
            binaryWriter.Write(versionList.InternalResourceVersion);
            UpdatableVersionAsset[] assets = versionList.GetAssets();
            binaryWriter.Write(assets.Length);
            UpdatableVersionResource[] resources = versionList.GetResources();
            binaryWriter.Write(resources.Length);
            foreach (UpdatableVersionResource resource in resources)
            {
                binaryWriter.WriteEncryptedString(resource.Name, CachedHashBytes);
                binaryWriter.WriteEncryptedString(resource.Variant, CachedHashBytes);
                binaryWriter.Write(resource.LoadType);
                binaryWriter.Write(resource.Length);
                binaryWriter.Write(resource.HashCode);
                binaryWriter.Write(resource.ZipLength);
                binaryWriter.Write(resource.ZipHashCode);
                int[] assetIndexes = resource.GetAssetIndexes();
                binaryWriter.Write(assetIndexes.Length);
                byte[] hashBytes = new byte[CachedHashBytesLength];
                foreach (int assetIndex in assetIndexes)
                {
                    ConverterUtils.GetBytes(resource.HashCode, hashBytes);
                    UpdatableVersionAsset updatableVersionAsset = assets[assetIndex];
                    binaryWriter.WriteEncryptedString(updatableVersionAsset.Name, hashBytes);
                    int[] dependencyAssetIndexes = updatableVersionAsset.GetDependencyAssetIndexes();
                    binaryWriter.Write(dependencyAssetIndexes.Length);
                    foreach (int dependencyAssetIndex in dependencyAssetIndexes)
                    {
                        binaryWriter.WriteEncryptedString(assets[dependencyAssetIndex].Name, hashBytes);
                    }
                }
            }

            UpdatableVersionResourceGroup[] resourceGroups = versionList.GetResourceGroups();
            binaryWriter.Write(resourceGroups.Length);
            foreach (UpdatableVersionResourceGroup resourceGroup in resourceGroups)
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
        /// 序列化可更新模式版本资源列表（版本 1）回调函数。
        /// </summary>
        /// <param name="binaryWriter">目标流。</param>
        /// <param name="versionList">要序列化的可更新模式版本资源列表（版本 1）。</param>
        /// <returns>是否序列化可更新模式版本资源列表（版本 1）成功。</returns>
        public static bool UpdatableVersionListSerializeCallback_V1(BinaryWriter binaryWriter, UpdatableVersionList versionList)
        {
            if (!versionList.IsValid)
            {
                return false;
            }

            RandomUtils.GetRandomBytes(CachedHashBytes);
            binaryWriter.Write(CachedHashBytes);
            binaryWriter.WriteEncryptedString(versionList.ApplicableGameVersion, CachedHashBytes);
            binaryWriter.Write7BitEncodedInt32(versionList.InternalResourceVersion);
            UpdatableVersionAsset[] assets = versionList.GetAssets();
            binaryWriter.Write7BitEncodedInt32(assets.Length);
            foreach (UpdatableVersionAsset asset in assets)
            {
                binaryWriter.WriteEncryptedString(asset.Name, CachedHashBytes);
                int[] dependencyAssetIndexes = asset.GetDependencyAssetIndexes();
                binaryWriter.Write7BitEncodedInt32(dependencyAssetIndexes.Length);
                foreach (int dependencyAssetIndex in dependencyAssetIndexes)
                {
                    binaryWriter.Write7BitEncodedInt32(dependencyAssetIndex);
                }
            }

            UpdatableVersionResource[] resources = versionList.GetResources();
            binaryWriter.Write7BitEncodedInt32(resources.Length);
            foreach (UpdatableVersionResource resource in resources)
            {
                binaryWriter.WriteEncryptedString(resource.Name, CachedHashBytes);
                binaryWriter.WriteEncryptedString(resource.Variant, CachedHashBytes);
                binaryWriter.WriteEncryptedString(resource.Extension != DefaultExtension ? resource.Extension : null, CachedHashBytes);
                binaryWriter.Write(resource.LoadType);
                binaryWriter.Write7BitEncodedInt32(resource.Length);
                binaryWriter.Write(resource.HashCode);
                binaryWriter.Write7BitEncodedInt32(resource.ZipLength);
                binaryWriter.Write(resource.ZipHashCode);
                int[] assetIndexes = resource.GetAssetIndexes();
                binaryWriter.Write7BitEncodedInt32(assetIndexes.Length);
                foreach (int assetIndex in assetIndexes)
                {
                    binaryWriter.Write7BitEncodedInt32(assetIndex);
                }
            }

            UpdatableVersionResourceGroup[] resourceGroups = versionList.GetResourceGroups();
            binaryWriter.Write7BitEncodedInt32(resourceGroups.Length);
            foreach (UpdatableVersionResourceGroup resourceGroup in resourceGroups)
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
        /// 序列化可更新模式版本资源列表（版本 2）回调函数。
        /// </summary>
        /// <param name="binaryWriter">目标流。</param>
        /// <param name="versionList">要序列化的可更新模式版本资源列表（版本 2）。</param>
        /// <returns>是否序列化可更新模式版本资源列表（版本 2）成功。</returns>
        public static bool UpdatableVersionListSerializeCallback_V2(BinaryWriter binaryWriter, UpdatableVersionList versionList)
        {
            if (!versionList.IsValid)
            {
                return false;
            }

            RandomUtils.GetRandomBytes(CachedHashBytes);
            binaryWriter.Write(CachedHashBytes);
            binaryWriter.WriteEncryptedString(versionList.ApplicableGameVersion, CachedHashBytes);
            binaryWriter.Write7BitEncodedInt32(versionList.InternalResourceVersion);
            UpdatableVersionAsset[] assets = versionList.GetAssets();
            binaryWriter.Write7BitEncodedInt32(assets.Length);
            foreach (UpdatableVersionAsset asset in assets)
            {
                binaryWriter.WriteEncryptedString(asset.Name, CachedHashBytes);
                int[] dependencyAssetIndexes = asset.GetDependencyAssetIndexes();
                binaryWriter.Write7BitEncodedInt32(dependencyAssetIndexes.Length);
                foreach (int dependencyAssetIndex in dependencyAssetIndexes)
                {
                    binaryWriter.Write7BitEncodedInt32(dependencyAssetIndex);
                }
            }

            UpdatableVersionResource[] resources = versionList.GetResources();
            binaryWriter.Write7BitEncodedInt32(resources.Length);
            foreach (UpdatableVersionResource resource in resources)
            {
                binaryWriter.WriteEncryptedString(resource.Name, CachedHashBytes);
                binaryWriter.WriteEncryptedString(resource.Variant, CachedHashBytes);
                binaryWriter.WriteEncryptedString(resource.Extension != DefaultExtension ? resource.Extension : null, CachedHashBytes);
                binaryWriter.Write(resource.LoadType);
                binaryWriter.Write7BitEncodedInt32(resource.Length);
                binaryWriter.Write(resource.HashCode);
                binaryWriter.Write7BitEncodedInt32(resource.ZipLength);
                binaryWriter.Write(resource.ZipHashCode);
                int[] assetIndexes = resource.GetAssetIndexes();
                binaryWriter.Write7BitEncodedInt32(assetIndexes.Length);
                foreach (int assetIndex in assetIndexes)
                {
                    binaryWriter.Write7BitEncodedInt32(assetIndex);
                }
            }

            UpdatableVersionFileSystem[] fileSystems = versionList.GetFileSystems();
            binaryWriter.Write7BitEncodedInt32(fileSystems.Length);
            foreach (UpdatableVersionFileSystem fileSystem in fileSystems)
            {
                binaryWriter.WriteEncryptedString(fileSystem.Name, CachedHashBytes);
                int[] resourceIndexes = fileSystem.GetResourceIndexes();
                binaryWriter.Write7BitEncodedInt32(resourceIndexes.Length);
                foreach (int resourceIndex in resourceIndexes)
                {
                    binaryWriter.Write7BitEncodedInt32(resourceIndex);
                }
            }

            UpdatableVersionResourceGroup[] resourceGroups = versionList.GetResourceGroups();
            binaryWriter.Write7BitEncodedInt32(resourceGroups.Length);
            foreach (UpdatableVersionResourceGroup resourceGroup in resourceGroups)
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
        /// 尝试从可更新模式版本资源列表（版本 0）获取指定键的值回调函数。
        /// </summary>
        /// <param name="binaryReader">指定流。</param>
        /// <param name="key">指定键。</param>
        /// <param name="value">指定键的值。</param>
        /// <returns></returns>
        public static bool UpdatableVersionListTryGetValueCallback_V0(BinaryReader binaryReader, string key, out object value)
        {
            value = null;
            if (key != "InternalResourceVersion")
            {
                return false;
            }

            binaryReader.BaseStream.Position += CachedHashBytesLength;
            byte stringLength = binaryReader.ReadByte();
            binaryReader.BaseStream.Position += stringLength;
            value = binaryReader.ReadInt32();
            return true;
        }

        /// <summary>
        /// 尝试从可更新模式版本资源列表（版本 1 或版本 2）获取指定键的值回调函数。
        /// </summary>
        /// <param name="binaryReader">指定流。</param>
        /// <param name="key">指定键。</param>
        /// <param name="value">指定键的值。</param>
        /// <returns></returns>
        public static bool UpdatableVersionListTryGetValueCallback_V1_V2(BinaryReader binaryReader, string key, out object value)
        {
            value = null;
            if (key != "InternalResourceVersion")
            {
                return false;
            }

            binaryReader.BaseStream.Position += CachedHashBytesLength;
            byte stringLength = binaryReader.ReadByte();
            binaryReader.BaseStream.Position += stringLength;
            value = binaryReader.Read7BitEncodedInt32();
            return true;
        }
    }
}