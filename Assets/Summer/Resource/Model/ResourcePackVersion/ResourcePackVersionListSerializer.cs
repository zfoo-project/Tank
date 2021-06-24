using System;
using System.IO;
using Summer.Base;
using Spring.Core;
using Spring.Util;

namespace Summer.Resource.Model.ResourcePackVersion
{
    /// <summary>
    /// 资源包版本资源列表序列化器。
    /// </summary>
    [Bean]
    public sealed class ResourcePackVersionListSerializer : GameFrameworkSerializer<ResourcePackVersionList>
    {
        private static readonly byte[] Header = new byte[] {(byte) 'G', (byte) 'F', (byte) 'K'};

        /// <summary>
        /// 获取资源包版本资源列表头标识。
        /// </summary>
        /// <returns>资源包版本资源列表头标识。</returns>
        protected override byte[] GetHeader()
        {
            return Header;
        }

        /// <summary>
        /// 初始化资源包版本资源列表序列化器的新实例。
        /// </summary>
        public ResourcePackVersionListSerializer()
        {
            RegisterDeserializeCallback(0, ResourcePackVersionListDeserializeCallback_V0);
            RegisterSerializeCallback(0, ResourcePackVersionListSerializeCallback_V0);
        }

        /// <summary>
        /// 反序列化资源包版本资源列表（版本 0）回调函数。
        /// </summary>
        /// <param name="binaryReader">指定流。</param>
        /// <returns>反序列化的资源包版本资源列表（版本 0）。</returns>
        public static ResourcePackVersionList ResourcePackVersionListDeserializeCallback_V0(BinaryReader binaryReader)
        {
            byte[] encryptBytes = binaryReader.ReadBytes(CachedHashBytesLength);
            int dataOffset = binaryReader.ReadInt32();
            long dataLength = binaryReader.ReadInt64();
            int dataHashCode = binaryReader.ReadInt32();
            int resourceCount = binaryReader.Read7BitEncodedInt32();
            ResourcePackVersionResource[] resources = resourceCount > 0 ? new ResourcePackVersionResource[resourceCount] : null;
            for (int i = 0; i < resourceCount; i++)
            {
                string name = binaryReader.ReadEncryptedString(encryptBytes);
                string variant = binaryReader.ReadEncryptedString(encryptBytes);
                string extension = binaryReader.ReadEncryptedString(encryptBytes) ?? DefaultExtension;
                byte loadType = binaryReader.ReadByte();
                long offset = binaryReader.Read7BitEncodedInt64();
                int length = binaryReader.Read7BitEncodedInt32();
                int hashCode = binaryReader.ReadInt32();
                int zipLength = binaryReader.Read7BitEncodedInt32();
                int zipHashCode = binaryReader.ReadInt32();
                resources[i] = new ResourcePackVersionResource(name, variant, extension, loadType, offset, length, hashCode, zipLength, zipHashCode);
            }

            return new ResourcePackVersionList(dataOffset, dataLength, dataHashCode, resources);
        }

        /// <summary>
        /// 序列化资源包版本资源列表（版本 0）回调函数。
        /// </summary>
        /// <param name="binaryWriter">目标流。</param>
        /// <param name="versionList">要序列化的资源包版本资源列表（版本 0）。</param>
        /// <returns>是否序列化资源包版本资源列表（版本 0）成功。</returns>
        public static bool ResourcePackVersionListSerializeCallback_V0(BinaryWriter binaryWriter, ResourcePackVersionList versionList)
        {
            if (!versionList.IsValid)
            {
                return false;
            }

            RandomUtils.GetRandomBytes(CachedHashBytes);
            binaryWriter.Write(CachedHashBytes);
            binaryWriter.Write(versionList.Offset);
            binaryWriter.Write(versionList.Length);
            binaryWriter.Write(versionList.HashCode);
            ResourcePackVersionResource[] resources = versionList.GetResources();
            binaryWriter.Write7BitEncodedInt32(resources.Length);
            foreach (ResourcePackVersionResource resource in resources)
            {
                binaryWriter.WriteEncryptedString(resource.Name, CachedHashBytes);
                binaryWriter.WriteEncryptedString(resource.Variant, CachedHashBytes);
                binaryWriter.WriteEncryptedString(resource.Extension != DefaultExtension ? resource.Extension : null, CachedHashBytes);
                binaryWriter.Write(resource.LoadType);
                binaryWriter.Write7BitEncodedInt64(resource.Offset);
                binaryWriter.Write7BitEncodedInt32(resource.Length);
                binaryWriter.Write(resource.HashCode);
                binaryWriter.Write7BitEncodedInt32(resource.ZipLength);
                binaryWriter.Write(resource.ZipHashCode);
            }

            Array.Clear(CachedHashBytes, 0, CachedHashBytesLength);
            return true;
        }
    }
}