using System;
using Summer.Base;
using Summer.Base.Model;
using Spring.Core;
using Spring.Logger;

namespace Summer.Setting
{
    /// <summary>
    /// 游戏配置管理器。
    /// </summary>
    [Bean]
    public sealed class SettingManager : ISettingManager
    {
        [Autowired]
        private ISettingHelper settingHelper;

        [BeforePostConstruct]
        private void Init()
        {
            if (settingHelper == null)
            {
                throw new GameFrameworkException("Setting helper is invalid.");
            }

            var flag = settingHelper.Load();

            if (!flag)
            {
                Log.Error("Load settings failure.");
            }
        }


        /// <summary>
        /// 保存游戏配置。
        /// </summary>
        /// <returns>是否保存游戏配置成功。</returns>
        public bool Save()
        {
            if (settingHelper == null)
            {
                throw new GameFrameworkException("Setting helper is invalid.");
            }

            return settingHelper.Save();
        }

        /// <summary>
        /// 获取所有游戏配置项的名称。
        /// </summary>
        /// <returns>所有游戏配置项的名称。</returns>
        public string[] GetAllSettingNames()
        {
            if (settingHelper == null)
            {
                throw new GameFrameworkException("Setting helper is invalid.");
            }

            return settingHelper.GetAllSettingNames();
        }


        /// <summary>
        /// 检查是否存在指定游戏配置项。
        /// </summary>
        /// <param name="settingName">要检查游戏配置项的名称。</param>
        /// <returns>指定的游戏配置项是否存在。</returns>
        public bool HasSetting(string settingName)
        {
            if (settingHelper == null)
            {
                throw new GameFrameworkException("Setting helper is invalid.");
            }

            if (string.IsNullOrEmpty(settingName))
            {
                throw new GameFrameworkException("Setting name is invalid.");
            }

            return settingHelper.HasSetting(settingName);
        }

        /// <summary>
        /// 移除指定游戏配置项。
        /// </summary>
        /// <param name="settingName">要移除游戏配置项的名称。</param>
        /// <returns>是否移除指定游戏配置项成功。</returns>
        public bool RemoveSetting(string settingName)
        {
            if (settingHelper == null)
            {
                throw new GameFrameworkException("Setting helper is invalid.");
            }

            if (string.IsNullOrEmpty(settingName))
            {
                throw new GameFrameworkException("Setting name is invalid.");
            }

            return settingHelper.RemoveSetting(settingName);
        }

        /// <summary>
        /// 清空所有游戏配置项。
        /// </summary>
        public void RemoveAllSettings()
        {
            if (settingHelper == null)
            {
                throw new GameFrameworkException("Setting helper is invalid.");
            }

            settingHelper.RemoveAllSettings();
        }

        /// <summary>
        /// 从指定游戏配置项中读取布尔值。
        /// </summary>
        /// <param name="settingName">要获取游戏配置项的名称。</param>
        /// <returns>读取的布尔值。</returns>
        public bool GetBool(string settingName)
        {
            if (settingHelper == null)
            {
                throw new GameFrameworkException("Setting helper is invalid.");
            }

            if (string.IsNullOrEmpty(settingName))
            {
                throw new GameFrameworkException("Setting name is invalid.");
            }

            return settingHelper.GetBool(settingName);
        }

        /// <summary>
        /// 从指定游戏配置项中读取布尔值。
        /// </summary>
        /// <param name="settingName">要获取游戏配置项的名称。</param>
        /// <param name="defaultValue">当指定的游戏配置项不存在时，返回此默认值。</param>
        /// <returns>读取的布尔值。</returns>
        public bool GetBool(string settingName, bool defaultValue)
        {
            if (settingHelper == null)
            {
                throw new GameFrameworkException("Setting helper is invalid.");
            }

            if (string.IsNullOrEmpty(settingName))
            {
                throw new GameFrameworkException("Setting name is invalid.");
            }

            return settingHelper.GetBool(settingName, defaultValue);
        }

        /// <summary>
        /// 向指定游戏配置项写入布尔值。
        /// </summary>
        /// <param name="settingName">要写入游戏配置项的名称。</param>
        /// <param name="value">要写入的布尔值。</param>
        public void SetBool(string settingName, bool value)
        {
            if (settingHelper == null)
            {
                throw new GameFrameworkException("Setting helper is invalid.");
            }

            if (string.IsNullOrEmpty(settingName))
            {
                throw new GameFrameworkException("Setting name is invalid.");
            }

            settingHelper.SetBool(settingName, value);
        }

        /// <summary>
        /// 从指定游戏配置项中读取整数值。
        /// </summary>
        /// <param name="settingName">要获取游戏配置项的名称。</param>
        /// <returns>读取的整数值。</returns>
        public int GetInt(string settingName)
        {
            if (settingHelper == null)
            {
                throw new GameFrameworkException("Setting helper is invalid.");
            }

            if (string.IsNullOrEmpty(settingName))
            {
                throw new GameFrameworkException("Setting name is invalid.");
            }

            return settingHelper.GetInt(settingName);
        }

        /// <summary>
        /// 从指定游戏配置项中读取整数值。
        /// </summary>
        /// <param name="settingName">要获取游戏配置项的名称。</param>
        /// <param name="defaultValue">当指定的游戏配置项不存在时，返回此默认值。</param>
        /// <returns>读取的整数值。</returns>
        public int GetInt(string settingName, int defaultValue)
        {
            if (settingHelper == null)
            {
                throw new GameFrameworkException("Setting helper is invalid.");
            }

            if (string.IsNullOrEmpty(settingName))
            {
                throw new GameFrameworkException("Setting name is invalid.");
            }

            return settingHelper.GetInt(settingName, defaultValue);
        }

        /// <summary>
        /// 向指定游戏配置项写入整数值。
        /// </summary>
        /// <param name="settingName">要写入游戏配置项的名称。</param>
        /// <param name="value">要写入的整数值。</param>
        public void SetInt(string settingName, int value)
        {
            if (settingHelper == null)
            {
                throw new GameFrameworkException("Setting helper is invalid.");
            }

            if (string.IsNullOrEmpty(settingName))
            {
                throw new GameFrameworkException("Setting name is invalid.");
            }

            settingHelper.SetInt(settingName, value);
        }

        /// <summary>
        /// 从指定游戏配置项中读取浮点数值。
        /// </summary>
        /// <param name="settingName">要获取游戏配置项的名称。</param>
        /// <returns>读取的浮点数值。</returns>
        public float GetFloat(string settingName)
        {
            if (settingHelper == null)
            {
                throw new GameFrameworkException("Setting helper is invalid.");
            }

            if (string.IsNullOrEmpty(settingName))
            {
                throw new GameFrameworkException("Setting name is invalid.");
            }

            return settingHelper.GetFloat(settingName);
        }

        /// <summary>
        /// 从指定游戏配置项中读取浮点数值。
        /// </summary>
        /// <param name="settingName">要获取游戏配置项的名称。</param>
        /// <param name="defaultValue">当指定的游戏配置项不存在时，返回此默认值。</param>
        /// <returns>读取的浮点数值。</returns>
        public float GetFloat(string settingName, float defaultValue)
        {
            if (settingHelper == null)
            {
                throw new GameFrameworkException("Setting helper is invalid.");
            }

            if (string.IsNullOrEmpty(settingName))
            {
                throw new GameFrameworkException("Setting name is invalid.");
            }

            return settingHelper.GetFloat(settingName, defaultValue);
        }

        /// <summary>
        /// 向指定游戏配置项写入浮点数值。
        /// </summary>
        /// <param name="settingName">要写入游戏配置项的名称。</param>
        /// <param name="value">要写入的浮点数值。</param>
        public void SetFloat(string settingName, float value)
        {
            if (settingHelper == null)
            {
                throw new GameFrameworkException("Setting helper is invalid.");
            }

            if (string.IsNullOrEmpty(settingName))
            {
                throw new GameFrameworkException("Setting name is invalid.");
            }

            settingHelper.SetFloat(settingName, value);
        }

        /// <summary>
        /// 从指定游戏配置项中读取字符串值。
        /// </summary>
        /// <param name="settingName">要获取游戏配置项的名称。</param>
        /// <returns>读取的字符串值。</returns>
        public string GetString(string settingName)
        {
            if (settingHelper == null)
            {
                throw new GameFrameworkException("Setting helper is invalid.");
            }

            if (string.IsNullOrEmpty(settingName))
            {
                throw new GameFrameworkException("Setting name is invalid.");
            }

            return settingHelper.GetString(settingName);
        }

        /// <summary>
        /// 从指定游戏配置项中读取字符串值。
        /// </summary>
        /// <param name="settingName">要获取游戏配置项的名称。</param>
        /// <param name="defaultValue">当指定的游戏配置项不存在时，返回此默认值。</param>
        /// <returns>读取的字符串值。</returns>
        public string GetString(string settingName, string defaultValue)
        {
            if (settingHelper == null)
            {
                throw new GameFrameworkException("Setting helper is invalid.");
            }

            if (string.IsNullOrEmpty(settingName))
            {
                throw new GameFrameworkException("Setting name is invalid.");
            }

            return settingHelper.GetString(settingName, defaultValue);
        }

        /// <summary>
        /// 向指定游戏配置项写入字符串值。
        /// </summary>
        /// <param name="settingName">要写入游戏配置项的名称。</param>
        /// <param name="value">要写入的字符串值。</param>
        public void SetString(string settingName, string value)
        {
            if (settingHelper == null)
            {
                throw new GameFrameworkException("Setting helper is invalid.");
            }

            if (string.IsNullOrEmpty(settingName))
            {
                throw new GameFrameworkException("Setting name is invalid.");
            }

            settingHelper.SetString(settingName, value);
        }

        /// <summary>
        /// 从指定游戏配置项中读取对象。
        /// </summary>
        /// <typeparam name="T">要读取对象的类型。</typeparam>
        /// <param name="settingName">要获取游戏配置项的名称。</param>
        /// <returns>读取的对象。</returns>
        public T GetObject<T>(string settingName)
        {
            if (settingHelper == null)
            {
                throw new GameFrameworkException("Setting helper is invalid.");
            }

            if (string.IsNullOrEmpty(settingName))
            {
                throw new GameFrameworkException("Setting name is invalid.");
            }

            return settingHelper.GetObject<T>(settingName);
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
            if (settingHelper == null)
            {
                throw new GameFrameworkException("Setting helper is invalid.");
            }

            if (string.IsNullOrEmpty(settingName))
            {
                throw new GameFrameworkException("Setting name is invalid.");
            }

            return settingHelper.GetObject(settingName, defaultObj);
        }


        /// <summary>
        /// 向指定游戏配置项写入对象。
        /// </summary>
        /// <typeparam name="T">要写入对象的类型。</typeparam>
        /// <param name="settingName">要写入游戏配置项的名称。</param>
        /// <param name="obj">要写入的对象。</param>
        public void SetObject<T>(string settingName, T obj)
        {
            if (settingHelper == null)
            {
                throw new GameFrameworkException("Setting helper is invalid.");
            }

            if (string.IsNullOrEmpty(settingName))
            {
                throw new GameFrameworkException("Setting name is invalid.");
            }

            settingHelper.SetObject(settingName, obj);
        }

        /// <summary>
        /// 向指定游戏配置项写入对象。
        /// </summary>
        /// <param name="settingName">要写入游戏配置项的名称。</param>
        /// <param name="obj">要写入的对象。</param>
        public void SetObject(string settingName, object obj)
        {
            if (settingHelper == null)
            {
                throw new GameFrameworkException("Setting helper is invalid.");
            }

            if (string.IsNullOrEmpty(settingName))
            {
                throw new GameFrameworkException("Setting name is invalid.");
            }

            settingHelper.SetObject(settingName, obj);
        }
    }
}