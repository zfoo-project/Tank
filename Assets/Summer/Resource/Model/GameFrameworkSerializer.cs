using System.Collections.Generic;
using System.IO;
using System.Text;
using Summer.Base;
using Summer.Base.Model;
using Spring.Util;

namespace Summer.Resource.Model
{
    /// <summary>
    /// 游戏框架序列化器基类。
    /// </summary>
    /// <typeparam name="T">要序列化的数据类型。</typeparam>
    public abstract class GameFrameworkSerializer<T>
    {
        
        private readonly Dictionary<byte, SerializeCallback> serializeCallbacks = new Dictionary<byte, SerializeCallback>();
        private readonly Dictionary<byte, DeserializeCallback> deserializeCallbacks = new Dictionary<byte, DeserializeCallback>();
        private readonly Dictionary<byte, TryGetValueCallback> tryGetValueCallbacks = new Dictionary<byte, TryGetValueCallback>();
        private byte latestSerializeCallbackVersion = 0;


        public static readonly string DefaultExtension = "dat";
        public static readonly int CachedHashBytesLength = 4;
        public static readonly byte[] CachedHashBytes = new byte[CachedHashBytesLength];

        public static int AssetNameToDependencyAssetNamesComparer(KeyValuePair<string, string[]> a, KeyValuePair<string, string[]> b)
        {
            return a.Key.CompareTo(b.Key);
        }

        public static int GetAssetNameIndex(List<KeyValuePair<string, string[]>> assetNameToDependencyAssetNames, string assetName)
        {
            return GetAssetNameIndexWithBinarySearch(assetNameToDependencyAssetNames, assetName, 0, assetNameToDependencyAssetNames.Count - 1);
        }

        private static int GetAssetNameIndexWithBinarySearch(List<KeyValuePair<string, string[]>> assetNameToDependencyAssetNames, string assetName, int leftIndex, int rightIndex)
        {
            if (leftIndex > rightIndex)
            {
                return -1;
            }

            int middleIndex = (leftIndex + rightIndex) / 2;
            if (assetNameToDependencyAssetNames[middleIndex].Key == assetName)
            {
                return middleIndex;
            }

            if (assetNameToDependencyAssetNames[middleIndex].Key.CompareTo(assetName) > 0)
            {
                return GetAssetNameIndexWithBinarySearch(assetNameToDependencyAssetNames, assetName, leftIndex, middleIndex - 1);
            }
            else
            {
                return GetAssetNameIndexWithBinarySearch(assetNameToDependencyAssetNames, assetName, middleIndex + 1, rightIndex);
            }
        }
        
        /// <summary>
        /// 序列化回调函数。
        /// </summary>
        /// <param name="binaryWriter">目标流。</param>
        /// <param name="data">要序列化的数据。</param>
        /// <returns>是否序列化数据成功。</returns>
        public delegate bool SerializeCallback(BinaryWriter binaryWriter, T data);

        /// <summary>
        /// 反序列化回调函数。
        /// </summary>
        /// <param name="binaryReader">指定流。</param>
        /// <returns>反序列化的数据。</returns>
        public delegate T DeserializeCallback(BinaryReader binaryReader);

        /// <summary>
        /// 尝试从指定流获取指定键的值回调函数。
        /// </summary>
        /// <param name="binaryReader">指定流。</param>
        /// <param name="key">指定键。</param>
        /// <param name="value">指定键的值。</param>
        /// <returns>是否从指定流获取指定键的值成功。</returns>
        public delegate bool TryGetValueCallback(BinaryReader binaryReader, string key, out object value);

        /// <summary>
        /// 注册序列化回调函数。
        /// </summary>
        /// <param name="version">序列化回调函数的版本。</param>
        /// <param name="callback">序列化回调函数。</param>
        public void RegisterSerializeCallback(byte version, SerializeCallback callback)
        {
            if (callback == null)
            {
                throw new GameFrameworkException("Serialize callback is invalid.");
            }

            serializeCallbacks[version] = callback;
            if (version > latestSerializeCallbackVersion)
            {
                latestSerializeCallbackVersion = version;
            }
        }

        /// <summary>
        /// 注册反序列化回调函数。
        /// </summary>
        /// <param name="version">反序列化回调函数的版本。</param>
        /// <param name="callback">反序列化回调函数。</param>
        public void RegisterDeserializeCallback(byte version, DeserializeCallback callback)
        {
            if (callback == null)
            {
                throw new GameFrameworkException("Deserialize callback is invalid.");
            }

            deserializeCallbacks[version] = callback;
        }

        /// <summary>
        /// 注册尝试从指定流获取指定键的值回调函数。
        /// </summary>
        /// <param name="version">尝试从指定流获取指定键的值回调函数的版本。</param>
        /// <param name="callback">尝试从指定流获取指定键的值回调函数。</param>
        public void RegisterTryGetValueCallback(byte version, TryGetValueCallback callback)
        {
            if (callback == null)
            {
                throw new GameFrameworkException("Try get value callback is invalid.");
            }

            tryGetValueCallbacks[version] = callback;
        }

        /// <summary>
        /// 序列化数据到目标流中。
        /// </summary>
        /// <param name="stream">目标流。</param>
        /// <param name="data">要序列化的数据。</param>
        /// <returns>是否序列化数据成功。</returns>
        public bool Serialize(Stream stream, T data)
        {
            if (serializeCallbacks.Count <= 0)
            {
                throw new GameFrameworkException("No serialize callback registered.");
            }

            return Serialize(stream, data, latestSerializeCallbackVersion);
        }

        /// <summary>
        /// 序列化数据到目标流中。
        /// </summary>
        /// <param name="stream">目标流。</param>
        /// <param name="data">要序列化的数据。</param>
        /// <param name="version">序列化回调函数的版本。</param>
        /// <returns>是否序列化数据成功。</returns>
        public bool Serialize(Stream stream, T data, byte version)
        {
            using (BinaryWriter binaryWriter = new BinaryWriter(stream, Encoding.UTF8))
            {
                byte[] header = GetHeader();
                binaryWriter.Write(header[0]);
                binaryWriter.Write(header[1]);
                binaryWriter.Write(header[2]);
                binaryWriter.Write(version);
                SerializeCallback callback = null;
                if (!serializeCallbacks.TryGetValue(version, out callback))
                {
                    throw new GameFrameworkException(StringUtils.Format("Serialize callback '{}' is not exist.", version.ToString()));
                }

                return callback(binaryWriter, data);
            }
        }

        /// <summary>
        /// 从指定流反序列化数据。
        /// </summary>
        /// <param name="stream">指定流。</param>
        /// <returns>反序列化的数据。</returns>
        public T Deserialize(Stream stream)
        {
            using (BinaryReader binaryReader = new BinaryReader(stream, Encoding.UTF8))
            {
                byte[] header = GetHeader();
                byte header0 = binaryReader.ReadByte();
                byte header1 = binaryReader.ReadByte();
                byte header2 = binaryReader.ReadByte();
                if (header0 != header[0] || header1 != header[1] || header2 != header[2])
                {
                    throw new GameFrameworkException(StringUtils.Format("Header is invalid, need '{}{}{}', current '{}{}{}'.",
                        ((char) header[0]).ToString(), ((char) header[1]).ToString(), ((char) header[2]).ToString(),
                        ((char) header0).ToString(), ((char) header1).ToString(), ((char) header2).ToString()));
                }

                byte version = binaryReader.ReadByte();
                DeserializeCallback callback = null;
                if (!deserializeCallbacks.TryGetValue(version, out callback))
                {
                    throw new GameFrameworkException(StringUtils.Format("Deserialize callback '{}' is not exist.", version.ToString()));
                }

                return callback(binaryReader);
            }
        }

        /// <summary>
        /// 尝试从指定流获取指定键的值。
        /// </summary>
        /// <param name="stream">指定流。</param>
        /// <param name="key">指定键。</param>
        /// <param name="value">指定键的值。</param>
        /// <returns>是否从指定流获取指定键的值成功。</returns>
        public bool TryGetValue(Stream stream, string key, out object value)
        {
            value = null;
            using (BinaryReader binaryReader = new BinaryReader(stream, Encoding.UTF8))
            {
                byte[] header = GetHeader();
                byte header0 = binaryReader.ReadByte();
                byte header1 = binaryReader.ReadByte();
                byte header2 = binaryReader.ReadByte();
                if (header0 != header[0] || header1 != header[1] || header2 != header[2])
                {
                    return false;
                }

                byte version = binaryReader.ReadByte();
                TryGetValueCallback callback = null;
                if (!tryGetValueCallbacks.TryGetValue(version, out callback))
                {
                    return false;
                }

                return callback(binaryReader, key, out value);
            }
        }

        /// <summary>
        /// 获取数据头标识。
        /// </summary>
        /// <returns>数据头标识。</returns>
        protected abstract byte[] GetHeader();
    }
}