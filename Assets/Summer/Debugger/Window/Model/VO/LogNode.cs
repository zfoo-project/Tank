using System;
using Spring.Collection.Reference;
using UnityEngine;

namespace Summer.Debugger.Window.Model.VO
{
    /// <summary>
    /// 日志记录结点。
    /// </summary>
    public sealed class LogNode : IReference
    {
        private DateTime logTime;
        private int logFrameCount;
        private LogType logType;
        private string logMessage;
        private string stackTrack;

        /// <summary>
        /// 初始化日志记录结点的新实例。
        /// </summary>
        public LogNode()
        {
            logTime = default(DateTime);
            logFrameCount = 0;
            logType = LogType.Error;
            logMessage = null;
            stackTrack = null;
        }

        /// <summary>
        /// 获取日志时间。
        /// </summary>
        public DateTime LogTime
        {
            get { return logTime; }
        }

        /// <summary>
        /// 获取日志帧计数。
        /// </summary>
        public int LogFrameCount
        {
            get { return logFrameCount; }
        }

        /// <summary>
        /// 获取日志类型。
        /// </summary>
        public LogType LogType
        {
            get { return logType; }
        }

        /// <summary>
        /// 获取日志内容。
        /// </summary>
        public string LogMessage
        {
            get { return logMessage; }
        }

        /// <summary>
        /// 获取日志堆栈信息。
        /// </summary>
        public string StackTrack
        {
            get { return stackTrack; }
        }

        /// <summary>
        /// 创建日志记录结点。
        /// </summary>
        /// <param name="logType">日志类型。</param>
        /// <param name="logMessage">日志内容。</param>
        /// <param name="stackTrack">日志堆栈信息。</param>
        /// <returns>创建的日志记录结点。</returns>
        public static LogNode Create(LogType logType, string logMessage, string stackTrack)
        {
            LogNode logNode = ReferenceCache.Acquire<LogNode>();
            logNode.logTime = DateTime.UtcNow;
            logNode.logFrameCount = Time.frameCount;
            logNode.logType = logType;
            logNode.logMessage = logMessage;
            logNode.stackTrack = stackTrack;
            return logNode;
        }

        /// <summary>
        /// 清理日志记录结点。
        /// </summary>
        public void Clear()
        {
            logTime = default(DateTime);
            logFrameCount = 0;
            logType = LogType.Error;
            logMessage = null;
            stackTrack = null;
        }
    }
}