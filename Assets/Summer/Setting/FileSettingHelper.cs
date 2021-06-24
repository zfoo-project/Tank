using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Spring.Core;
using Spring.Logger;
using Spring.Util;
using Spring.Util.Json;
using UnityEngine;

namespace Summer.Setting
{
    /// <summary>
    /// 默认游戏配置辅助器。
    /// </summary>
    public class FileSettingHelper : ISettingHelper
    {
        private const string SETTING_FILE_NAME = "MySetting.json";

        private IDictionary<string, string> settingMap = new SortedDictionary<string, string>(StringComparer.Ordinal);

        private readonly string filePath = PathUtils.GetRegularPath(Path.Combine(Application.persistentDataPath, SETTING_FILE_NAME));


        /// <summary>
        /// 加载游戏配置。
        /// </summary>
        /// <returns>是否加载游戏配置成功。</returns>
        public bool Load()
        {
            try
            {
                if (!File.Exists(filePath))
                {
                    return true;
                }

                using (FileStream fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
                {
                    using (BinaryReader binaryReader = new BinaryReader(fileStream, Encoding.UTF8))
                    {
                        settingMap =
                            JsonUtils.string2Object<SortedDictionary<string, string>>(binaryReader.ReadString());
                    }

                    return true;
                }
            }
            catch (Exception exception)
            {
                Log.Warning("Load settings failure with exception '{}'.", exception.ToString());
                return false;
            }
        }

        /// <summary>
        /// 保存游戏配置。
        /// </summary>
        /// <returns>是否保存游戏配置成功。</returns>
        public bool Save()
        {
            try
            {
                using (FileStream fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write))
                {
                    using (BinaryWriter binaryWriter = new BinaryWriter(fileStream, Encoding.UTF8))
                    {
                        binaryWriter.Write(JsonUtils.object2String(settingMap));
                        return true;
                    }
                }
            }
            catch (Exception exception)
            {
                Log.Warning("Save settings failure with exception '{}'.", exception.ToString());
                return false;
            }
        }

        /// <summary>
        /// 获取所有游戏配置项的名称。
        /// </summary>
        /// <returns>所有游戏配置项的名称。</returns>
        public string[] GetAllSettingNames()
        {
            return settingMap.Values.ToArray();
        }


        /// <summary>
        /// 检查是否存在指定游戏配置项。
        /// </summary>
        /// <param name="settingName">要检查游戏配置项的名称。</param>
        /// <returns>指定的游戏配置项是否存在。</returns>
        public bool HasSetting(string settingName)
        {
            return settingMap.ContainsKey(settingName);
        }

        /// <summary>
        /// 移除指定游戏配置项。
        /// </summary>
        /// <param name="settingName">要移除游戏配置项的名称。</param>
        /// <returns>是否移除指定游戏配置项成功。</returns>
        public bool RemoveSetting(string settingName)
        {
            return settingMap.Remove(settingName);
        }


        /// <summary>
        /// 清空所有游戏配置项。
        /// </summary>
        public void RemoveAllSettings()
        {
            settingMap.Clear();
        }

        /// <summary>
        /// 从指定游戏配置项中读取布尔值。
        /// </summary>
        /// <param name="settingName">要获取游戏配置项的名称。</param>
        /// <returns>读取的布尔值。</returns>
        public bool GetBool(string settingName)
        {
            string value = null;
            if (!settingMap.TryGetValue(settingName, out value))
            {
                Log.Warning("Setting '{}' is not exist.", settingName);
                return false;
            }

            return int.Parse(value) != 0;
        }

        /// <summary>
        /// 从指定游戏配置项中读取布尔值。
        /// </summary>
        /// <param name="settingName">要获取游戏配置项的名称。</param>
        /// <param name="defaultValue">当指定的游戏配置项不存在时，返回此默认值。</param>
        /// <returns>读取的布尔值。</returns>
        public bool GetBool(string settingName, bool defaultValue)
        {
            string value = null;
            if (!settingMap.TryGetValue(settingName, out value))
            {
                return defaultValue;
            }

            return int.Parse(value) != 0;
        }

        /// <summary>
        /// 向指定游戏配置项写入布尔值。
        /// </summary>
        /// <param name="settingName">要写入游戏配置项的名称。</param>
        /// <param name="value">要写入的布尔值。</param>
        public void SetBool(string settingName, bool value)
        {
            settingMap[settingName] = value ? "1" : "0";
        }

        /// <summary>
        /// 从指定游戏配置项中读取整数值。
        /// </summary>
        /// <param name="settingName">要获取游戏配置项的名称。</param>
        /// <returns>读取的整数值。</returns>
        public int GetInt(string settingName)
        {
            string value = null;
            if (!settingMap.TryGetValue(settingName, out value))
            {
                Log.Warning("Setting '{}' is not exist.", settingName);
                return 0;
            }

            return int.Parse(value);
        }

        /// <summary>
        /// 从指定游戏配置项中读取整数值。
        /// </summary>
        /// <param name="settingName">要获取游戏配置项的名称。</param>
        /// <param name="defaultValue">当指定的游戏配置项不存在时，返回此默认值。</param>
        /// <returns>读取的整数值。</returns>
        public int GetInt(string settingName, int defaultValue)
        {
            string value = null;
            if (!settingMap.TryGetValue(settingName, out value))
            {
                return defaultValue;
            }

            return int.Parse(value);
        }

        /// <summary>
        /// 向指定游戏配置项写入整数值。
        /// </summary>
        /// <param name="settingName">要写入游戏配置项的名称。</param>
        /// <param name="value">要写入的整数值。</param>
        public void SetInt(string settingName, int value)
        {
            settingMap[settingName] = value.ToString();
        }

        /// <summary>
        /// 从指定游戏配置项中读取浮点数值。
        /// </summary>
        /// <param name="settingName">要获取游戏配置项的名称。</param>
        /// <returns>读取的浮点数值。</returns>
        public float GetFloat(string settingName)
        {
            string value = null;
            if (!settingMap.TryGetValue(settingName, out value))
            {
                Log.Warning("Setting '{}' is not exist.", settingName);
                return 0f;
            }

            return float.Parse(value);
        }

        /// <summary>
        /// 从指定游戏配置项中读取浮点数值。
        /// </summary>
        /// <param name="settingName">要获取游戏配置项的名称。</param>
        /// <param name="defaultValue">当指定的游戏配置项不存在时，返回此默认值。</param>
        /// <returns>读取的浮点数值。</returns>
        public float GetFloat(string settingName, float defaultValue)
        {
            string value = null;
            if (!settingMap.TryGetValue(settingName, out value))
            {
                return defaultValue;
            }

            return float.Parse(value);
        }

        /// <summary>
        /// 向指定游戏配置项写入浮点数值。
        /// </summary>
        /// <param name="settingName">要写入游戏配置项的名称。</param>
        /// <param name="value">要写入的浮点数值。</param>
        public void SetFloat(string settingName, float value)
        {
            settingMap[settingName] = value.ToString();
        }

        /// <summary>
        /// 从指定游戏配置项中读取字符串值。
        /// </summary>
        /// <param name="settingName">要获取游戏配置项的名称。</param>
        /// <returns>读取的字符串值。</returns>
        public string GetString(string settingName)
        {
            string value = null;
            if (!settingMap.TryGetValue(settingName, out value))
            {
                Log.Warning("Setting '{}' is not exist.", settingName);
                return null;
            }

            return value;
        }

        /// <summary>
        /// 从指定游戏配置项中读取字符串值。
        /// </summary>
        /// <param name="settingName">要获取游戏配置项的名称。</param>
        /// <param name="defaultValue">当指定的游戏配置项不存在时，返回此默认值。</param>
        /// <returns>读取的字符串值。</returns>
        public string GetString(string settingName, string defaultValue)
        {
            string value = null;
            if (!settingMap.TryGetValue(settingName, out value))
            {
                return defaultValue;
            }

            return value;
        }

        /// <summary>
        /// 向指定游戏配置项写入字符串值。
        /// </summary>
        /// <param name="settingName">要写入游戏配置项的名称。</param>
        /// <param name="value">要写入的字符串值。</param>
        public void SetString(string settingName, string value)
        {
            settingMap[settingName] = value;
        }

        /// <summary>
        /// 从指定游戏配置项中读取对象。
        /// </summary>
        /// <typeparam name="T">要读取对象的类型。</typeparam>
        /// <param name="settingName">要获取游戏配置项的名称。</param>
        /// <returns>读取的对象。</returns>
        public T GetObject<T>(string settingName)
        {
            return JsonUtils.string2Object<T>(GetString(settingName));
        }

        /// <summary>
        /// 从指定游戏配置项中读取对象。
        /// </summary>
        /// <typeparam name="T">要读取对象的类型。</typeparam>
        /// <param name="settingName">要获取游戏配置项的名称。</param>
        /// <param name="defaultObj">当指定的游戏配置项不存在时，返回此默认对象。</param>
        /// <returns>读取的对象。</returns>
        public T GetObject<T>(string settingName, T defaultObj)
        {
            string json = GetString(settingName, null);
            if (json == null)
            {
                return defaultObj;
            }

            return JsonUtils.string2Object<T>(json);
        }


        /// <summary>
        /// 向指定游戏配置项写入对象。
        /// </summary>
        /// <typeparam name="T">要写入对象的类型。</typeparam>
        /// <param name="settingName">要写入游戏配置项的名称。</param>
        /// <param name="obj">要写入的对象。</param>
        public void SetObject<T>(string settingName, T obj)
        {
            SetString(settingName, JsonUtils.object2String(obj));
        }

        /// <summary>
        /// 向指定游戏配置项写入对象。
        /// </summary>
        /// <param name="settingName">要写入游戏配置项的名称。</param>
        /// <param name="obj">要写入的对象。</param>
        public void SetObject(string settingName, object obj)
        {
            SetString(settingName, JsonUtils.object2String(obj));
        }
    }
}