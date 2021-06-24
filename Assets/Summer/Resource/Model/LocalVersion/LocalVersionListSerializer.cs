using System;
using System.IO;
using Summer.Base;
using Spring.Core;
using Spring.Util;

namespace Summer.Resource.Model.LocalVersion
{
    /// <summary>
    /// 本地读写区版本资源列表序列化器。
    /// </summary>
    [Bean]
    public sealed class LocalVersionListSerializer : GameFrameworkSerializer<LocalVersionList>
    {
        private static readonly byte[] Header = new byte[] {(byte) 'G', (byte) 'F', (byte) 'W'};

        /// <summary>
        /// 获取本地读写区版本资源列表头标识。
        /// </summary>
        /// <returns>本地读写区版本资源列表头标识。</returns>
        protected override byte[] GetHeader()
        {
            return Header;
        }

        /// <summary>
        /// 初始化本地读写区版本资源列表序列化器的新实例。
        /// </summary>
        public LocalVersionListSerializer()
        {
            RegisterSerializeCallback(0, LocalVersionListSerializeCallback_V0);
            RegisterSerializeCallback(1, LocalVersionListSerializeCallback_V1);
            RegisterSerializeCallback(2, LocalVersionListSerializeCallback_V2);

            RegisterDeserializeCallback(0, LocalVersionListDeserializeCallback_V0);
            RegisterDeserializeCallback(1, LocalVersionListDeserializeCallback_V1);
            RegisterDeserializeCallback(2, LocalVersionListDeserializeCallback_V2);
        }

        /// <summary>
        /// 反序列化本地版本资源列表（版本 0）回调函数。
        /// </summary>
        /// <param name="binaryReader">指定流。</param>
        /// <returns>反序列化的本地版本资源列表（版本 0）。</returns>
        public static LocalVersionList LocalVersionListDeserializeCallback_V0(BinaryReader binaryReader)
        {
            byte[] encryptBytes = binaryReader.ReadBytes(CachedHashBytesLength);
            int resourceCount = binaryReader.ReadInt32();
            LocalVersionResource[] resources = resourceCount > 0 ? new LocalVersionResource[resourceCount] : null;
            for (int i = 0; i < resourceCount; i++)
            {
                string name = binaryReader.ReadEncryptedString(encryptBytes);
                string variant = binaryReader.ReadEncryptedString(encryptBytes);
                byte loadType = binaryReader.ReadByte();
                int length = binaryReader.ReadInt32();
                int hashCode = binaryReader.ReadInt32();
                resources[i] = new LocalVersionResource(name, variant, null, loadType, length, hashCode);
            }

            return new LocalVersionList(resources, null);
        }

        /// <summary>
        /// 反序列化本地版本资源列表（版本 1）回调函数。
        /// </summary>
        /// <param name="binaryReader">指定流。</param>
        /// <returns>反序列化的本地版本资源列表（版本 1）。</returns>
        public static LocalVersionList LocalVersionListDeserializeCallback_V1(BinaryReader binaryReader)
        {
            byte[] encryptBytes = binaryReader.ReadBytes(CachedHashBytesLength);
            int resourceCount = binaryReader.Read7BitEncodedInt32();
            LocalVersionResource[] resources = resourceCount > 0 ? new LocalVersionResource[resourceCount] : null;
            for (int i = 0; i < resourceCount; i++)
            {
                string name = binaryReader.ReadEncryptedString(encryptBytes);
                string variant = binaryReader.ReadEncryptedString(encryptBytes);
                string extension = binaryReader.ReadEncryptedString(encryptBytes) ?? DefaultExtension;
                byte loadType = binaryReader.ReadByte();
                int length = binaryReader.Read7BitEncodedInt32();
                int hashCode = binaryReader.ReadInt32();
                resources[i] = new LocalVersionResource(name, variant, extension, loadType, length, hashCode);
            }

            return new LocalVersionList(resources, null);
        }

        /// <summary>
        /// 反序列化本地版本资源列表（版本 2）回调函数。
        /// </summary>
        /// <param name="binaryReader">指定流。</param>
        /// <returns>反序列化的本地版本资源列表（版本 2）。</returns>
        public static LocalVersionList LocalVersionListDeserializeCallback_V2(BinaryReader binaryReader)
        {
            byte[] encryptBytes = binaryReader.ReadBytes(CachedHashBytesLength);
            int resourceCount = binaryReader.Read7BitEncodedInt32();
            LocalVersionResource[] resources = resourceCount > 0 ? new LocalVersionResource[resourceCount] : null;
            for (int i = 0; i < resourceCount; i++)
            {
                string name = binaryReader.ReadEncryptedString(encryptBytes);
                string variant = binaryReader.ReadEncryptedString(encryptBytes);
                string extension = binaryReader.ReadEncryptedString(encryptBytes) ?? DefaultExtension;
                byte loadType = binaryReader.ReadByte();
                int length = binaryReader.Read7BitEncodedInt32();
                int hashCode = binaryReader.ReadInt32();
                resources[i] = new LocalVersionResource(name, variant, extension, loadType, length, hashCode);
            }

            int fileSystemCount = binaryReader.Read7BitEncodedInt32();
            LocalVersionFileSystem[] fileSystems = fileSystemCount > 0 ? new LocalVersionFileSystem[fileSystemCount] : null;
            for (int i = 0; i < fileSystemCount; i++)
            {
                string name = binaryReader.ReadEncryptedString(encryptBytes);
                int resourceIndexCount = binaryReader.Read7BitEncodedInt32();
                int[] resourceIndexes = resourceIndexCount > 0 ? new int[resourceIndexCount] : null;
                for (int j = 0; j < resourceIndexCount; j++)
                {
                    resourceIndexes[j] = binaryReader.Read7BitEncodedInt32();
                }

                fileSystems[i] = new LocalVersionFileSystem(name, resourceIndexes);
            }

            return new LocalVersionList(resources, fileSystems);
        }

        /// <summary>
        /// 序列化本地版本资源列表（版本 0）回调函数。
        /// </summary>
        /// <param name="binaryWriter">目标流。</param>
        /// <param name="versionList">要序列化的本地版本资源列表（版本 0）。</param>
        /// <returns>是否序列化本地版本资源列表（版本 0）成功。</returns>
        public static bool LocalVersionListSerializeCallback_V0(BinaryWriter binaryWriter, LocalVersionList versionList)
        {
            if (!versionList.IsValid)
            {
                return false;
            }

            RandomUtils.GetRandomBytes(CachedHashBytes);
            binaryWriter.Write(CachedHashBytes);
            LocalVersionResource[] resources = versionList.GetResources();
            binaryWriter.Write(resources.Length);
            foreach (LocalVersionResource resource in resources)
            {
                binaryWriter.WriteEncryptedString(resource.Name, CachedHashBytes);
                binaryWriter.WriteEncryptedString(resource.Variant, CachedHashBytes);
                binaryWriter.Write(resource.LoadType);
                binaryWriter.Write(resource.Length);
                binaryWriter.Write(resource.HashCode);
            }

            Array.Clear(CachedHashBytes, 0, CachedHashBytesLength);
            return true;
        }

        /// <summary>
        /// 序列化本地版本资源列表（版本 1）回调函数。
        /// </summary>
        /// <param name="binaryWriter">目标流。</param>
        /// <param name="versionList">要序列化的本地版本资源列表（版本 1）。</param>
        /// <returns>是否序列化本地版本资源列表（版本 1）成功。</returns>
        public static bool LocalVersionListSerializeCallback_V1(BinaryWriter binaryWriter, LocalVersionList versionList)
        {
            if (!versionList.IsValid)
            {
                return false;
            }

            RandomUtils.GetRandomBytes(CachedHashBytes);
            binaryWriter.Write(CachedHashBytes);
            LocalVersionResource[] resources = versionList.GetResources();
            binaryWriter.Write7BitEncodedInt32(resources.Length);
            foreach (LocalVersionResource resource in resources)
            {
                binaryWriter.WriteEncryptedString(resource.Name, CachedHashBytes);
                binaryWriter.WriteEncryptedString(resource.Variant, CachedHashBytes);
                binaryWriter.WriteEncryptedString(resource.Extension != DefaultExtension ? resource.Extension : null, CachedHashBytes);
                binaryWriter.Write(resource.LoadType);
                binaryWriter.Write7BitEncodedInt32(resource.Length);
                binaryWriter.Write(resource.HashCode);
            }

            Array.Clear(CachedHashBytes, 0, CachedHashBytesLength);
            return true;
        }

        /// <summary>
        /// 序列化本地版本资源列表（版本 2）回调函数。
        /// </summary>
        /// <param name="binaryWriter">目标流。</param>
        /// <param name="versionList">要序列化的本地版本资源列表（版本 2）。</param>
        /// <returns>是否序列化本地版本资源列表（版本 2）成功。</returns>
        public static bool LocalVersionListSerializeCallback_V2(BinaryWriter binaryWriter, LocalVersionList versionList)
        {
            if (!versionList.IsValid)
            {
                return false;
            }

            RandomUtils.GetRandomBytes(CachedHashBytes);
            binaryWriter.Write(CachedHashBytes);
            LocalVersionResource[] resources = versionList.GetResources();
            binaryWriter.Write7BitEncodedInt32(resources.Length);
            foreach (LocalVersionResource resource in resources)
            {
                binaryWriter.WriteEncryptedString(resource.Name, CachedHashBytes);
                binaryWriter.WriteEncryptedString(resource.Variant, CachedHashBytes);
                binaryWriter.WriteEncryptedString(resource.Extension != DefaultExtension ? resource.Extension : null, CachedHashBytes);
                binaryWriter.Write(resource.LoadType);
                binaryWriter.Write7BitEncodedInt32(resource.Length);
                binaryWriter.Write(resource.HashCode);
            }

            LocalVersionFileSystem[] fileSystems = versionList.GetFileSystems();
            binaryWriter.Write7BitEncodedInt32(fileSystems.Length);
            foreach (LocalVersionFileSystem fileSystem in fileSystems)
            {
                binaryWriter.WriteEncryptedString(fileSystem.Name, CachedHashBytes);
                int[] resourceIndexes = fileSystem.GetResourceIndexes();
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