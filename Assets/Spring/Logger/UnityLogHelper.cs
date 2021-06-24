using System;
using Spring.Util;
using UnityEngine;

namespace Spring.Logger
{
    /// <summary>
    /// 默认游戏框架日志辅助器。
    /// </summary>
    public class UnityLogHelper : ILogHelper
    {
        /// <summary>
        /// 记录日志。
        /// </summary>
        /// <param name="level">日志等级。</param>
        /// <param name="message">日志内容。</param>
        public void Log(LogLevel level, object message)
        {
            switch (level)
            {
                case LogLevel.Debug:
                    Debug.Log(StringUtils.Format("<color=#888888>{}</color>", message.ToString()));
                    break;

                case LogLevel.Info:
                    Debug.Log(message.ToString());
                    break;

                case LogLevel.Warning:
                    Debug.LogWarning(message.ToString());
                    break;

                case LogLevel.Error:
                    Debug.LogError(message.ToString());
                    break;

                default:
                    throw new Exception(message.ToString());
            }
        }
    }
}