using System;
using System.Collections.Generic;
using Summer.Debugger.Window.Model;
using Summer.Debugger.Window.Model.VO;
using Summer.Setting;
using Spring.Collection.Reference;
using Spring.Core;
using Spring.Logger;
using Spring.Util;
using UnityEngine;

namespace Summer.Debugger.Window
{
    [Serializable]
    public sealed class ConsoleWindow : IDebuggerWindow
    {
        private readonly Queue<LogNode> logNodes = new Queue<LogNode>();
        private readonly TextEditor textEditor = new TextEditor();

        private Vector2 logScrollPosition = Vector2.zero;
        private Vector2 stackScrollPosition = Vector2.zero;
        private int infoCount = 0;
        private int warningCount = 0;
        private int errorCount = 0;
        private int fatalCount = 0;
        private LogNode selectedNode = null;
        private bool lastLockScroll = true;
        private bool lastInfoFilter = true;
        private bool lastWarningFilter = true;
        private bool lastErrorFilter = true;
        private bool lastFatalFilter = true;

        [SerializeField]
        private bool lockScroll = true;

        [SerializeField]
        private int maxLine = 100;

        [SerializeField]
        private bool infoFilter = true;

        [SerializeField]
        private bool warningFilter = true;

        [SerializeField]
        private bool errorFilter = true;

        [SerializeField]
        private bool fatalFilter = true;

        [SerializeField]
        private Color32 infoColor = Color.white;

        [SerializeField]
        private Color32 warningColor = Color.yellow;

        [SerializeField]
        private Color32 errorColor = Color.red;

        [SerializeField]
        private Color32 fatalColor = new Color(0.7f, 0.2f, 0.2f);

        public bool LockScroll
        {
            get { return lockScroll; }
            set { lockScroll = value; }
        }

        public int MaxLine
        {
            get { return maxLine; }
            set { maxLine = value; }
        }

        public bool InfoFilter
        {
            get { return infoFilter; }
            set { infoFilter = value; }
        }

        public bool WarningFilter
        {
            get { return warningFilter; }
            set { warningFilter = value; }
        }

        public bool ErrorFilter
        {
            get { return errorFilter; }
            set { errorFilter = value; }
        }

        public bool FatalFilter
        {
            get { return fatalFilter; }
            set { fatalFilter = value; }
        }

        public int InfoCount
        {
            get { return infoCount; }
        }

        public int WarningCount
        {
            get { return warningCount; }
        }

        public int ErrorCount
        {
            get { return errorCount; }
        }

        public int FatalCount
        {
            get { return fatalCount; }
        }

        public Color32 InfoColor
        {
            get { return infoColor; }
            set { infoColor = value; }
        }

        public Color32 WarningColor
        {
            get { return warningColor; }
            set { warningColor = value; }
        }

        public Color32 ErrorColor
        {
            get { return errorColor; }
            set { errorColor = value; }
        }

        public Color32 FatalColor
        {
            get { return fatalColor; }
            set { fatalColor = value; }
        }

        public void Initialize(params object[] args)
        {
            Application.logMessageReceived += OnLogMessageReceived;
            lockScroll = lastLockScroll = SpringContext.GetBean<ISettingManager>().GetBool("Debugger.Console.LockScroll", true);
            infoFilter = lastInfoFilter = SpringContext.GetBean<ISettingManager>().GetBool("Debugger.Console.InfoFilter", true);
            warningFilter = lastWarningFilter = SpringContext.GetBean<ISettingManager>().GetBool("Debugger.Console.WarningFilter", true);
            errorFilter = lastErrorFilter = SpringContext.GetBean<ISettingManager>().GetBool("Debugger.Console.ErrorFilter", true);
            fatalFilter = lastFatalFilter = SpringContext.GetBean<ISettingManager>().GetBool("Debugger.Console.FatalFilter", true);
        }

        public void Shutdown()
        {
            Application.logMessageReceived -= OnLogMessageReceived;
            Clear();
        }

        public void OnEnter()
        {
        }

        public void OnLeave()
        {
        }

        public void OnUpdate(float elapseSeconds, float realElapseSeconds)
        {
            if (lastLockScroll != lockScroll)
            {
                lastLockScroll = lockScroll;
                SpringContext.GetBean<ISettingManager>().SetBool("Debugger.Console.LockScroll", lockScroll);
            }

            if (lastInfoFilter != infoFilter)
            {
                lastInfoFilter = infoFilter;
                SpringContext.GetBean<ISettingManager>().SetBool("Debugger.Console.InfoFilter", infoFilter);
            }

            if (lastWarningFilter != warningFilter)
            {
                lastWarningFilter = warningFilter;
                SpringContext.GetBean<ISettingManager>().SetBool("Debugger.Console.WarningFilter", warningFilter);
            }

            if (lastErrorFilter != errorFilter)
            {
                lastErrorFilter = errorFilter;
                SpringContext.GetBean<ISettingManager>().SetBool("Debugger.Console.ErrorFilter", errorFilter);
            }

            if (lastFatalFilter != fatalFilter)
            {
                lastFatalFilter = fatalFilter;
                SpringContext.GetBean<ISettingManager>().SetBool("Debugger.Console.FatalFilter", fatalFilter);
            }
        }

        public void OnDraw()
        {
            RefreshCount();

            GUILayout.BeginHorizontal();
            {
                if (GUILayout.Button("Clear All", GUILayout.Width(100f)))
                {
                    Clear();
                }

                lockScroll = GUILayout.Toggle(lockScroll, "Lock Scroll", GUILayout.Width(90f));
                GUILayout.FlexibleSpace();
                infoFilter = GUILayout.Toggle(infoFilter, StringUtils.Format("Info ({})", infoCount.ToString()),
                    GUILayout.Width(90f));
                warningFilter = GUILayout.Toggle(warningFilter,
                    StringUtils.Format("Warning ({})", warningCount.ToString()), GUILayout.Width(90f));
                errorFilter = GUILayout.Toggle(errorFilter,
                    StringUtils.Format("Error ({})", errorCount.ToString()), GUILayout.Width(90f));
                fatalFilter = GUILayout.Toggle(fatalFilter,
                    StringUtils.Format("Fatal ({})", fatalCount.ToString()), GUILayout.Width(90f));
            }
            GUILayout.EndHorizontal();

            GUILayout.BeginVertical("box");
            {
                if (lockScroll)
                {
                    logScrollPosition.y = float.MaxValue;
                }

                logScrollPosition = GUILayout.BeginScrollView(logScrollPosition);
                {
                    bool selected = false;
                    foreach (LogNode logNode in logNodes)
                    {
                        switch (logNode.LogType)
                        {
                            case LogType.Log:
                                if (!infoFilter)
                                {
                                    continue;
                                }

                                break;

                            case LogType.Warning:
                                if (!warningFilter)
                                {
                                    continue;
                                }

                                break;

                            case LogType.Error:
                                if (!errorFilter)
                                {
                                    continue;
                                }

                                break;

                            case LogType.Exception:
                                if (!fatalFilter)
                                {
                                    continue;
                                }

                                break;
                        }

                        if (GUILayout.Toggle(selectedNode == logNode, GetLogString(logNode)))
                        {
                            selected = true;
                            if (selectedNode != logNode)
                            {
                                selectedNode = logNode;
                                stackScrollPosition = Vector2.zero;
                            }
                        }
                    }

                    if (!selected)
                    {
                        selectedNode = null;
                    }
                }
                GUILayout.EndScrollView();
            }
            GUILayout.EndVertical();

            GUILayout.BeginVertical("box");
            {
                stackScrollPosition = GUILayout.BeginScrollView(stackScrollPosition, GUILayout.Height(100f));
                {
                    if (selectedNode != null)
                    {
                        GUILayout.BeginHorizontal();
                        Color32 color = GetLogStringColor(selectedNode.LogType);
                        GUILayout.Label(StringUtils.Format("<color=#{}{}{}{}><b>{}</b></color>",
                            color.r.ToString("x2"), color.g.ToString("x2"), color.b.ToString("x2"),
                            color.a.ToString("x2"), selectedNode.LogMessage));
                        if (GUILayout.Button("COPY", GUILayout.Width(60f), GUILayout.Height(30f)))
                        {
                            textEditor.text = StringUtils.Format("{}{}{}{}", selectedNode.LogMessage,
                                selectedNode.StackTrack, Environment.NewLine);
                            textEditor.OnFocus();
                            textEditor.Copy();
                            textEditor.text = null;
                        }

                        GUILayout.EndHorizontal();
                        GUILayout.Label(selectedNode.StackTrack);
                    }

                    GUILayout.EndScrollView();
                }
            }
            GUILayout.EndVertical();
        }

        private void Clear()
        {
            logNodes.Clear();
        }

        public void RefreshCount()
        {
            infoCount = 0;
            warningCount = 0;
            errorCount = 0;
            fatalCount = 0;
            foreach (LogNode logNode in logNodes)
            {
                switch (logNode.LogType)
                {
                    case LogType.Log:
                        infoCount++;
                        break;

                    case LogType.Warning:
                        warningCount++;
                        break;

                    case LogType.Error:
                        errorCount++;
                        break;

                    case LogType.Exception:
                        fatalCount++;
                        break;
                }
            }
        }

        public void GetRecentLogs(List<LogNode> results)
        {
            if (results == null)
            {
                Log.Error("Results is invalid.");
                return;
            }

            results.Clear();
            foreach (LogNode logNode in logNodes)
            {
                results.Add(logNode);
            }
        }

        public void GetRecentLogs(List<LogNode> results, int count)
        {
            if (results == null)
            {
                Log.Error("Results is invalid.");
                return;
            }

            if (count <= 0)
            {
                Log.Error("Count is invalid.");
                return;
            }

            int position = logNodes.Count - count;
            if (position < 0)
            {
                position = 0;
            }

            int index = 0;
            results.Clear();
            foreach (LogNode logNode in logNodes)
            {
                if (index++ < position)
                {
                    continue;
                }

                results.Add(logNode);
            }
        }

        private void OnLogMessageReceived(string logMessage, string stackTrace, LogType logType)
        {
            if (logType == LogType.Assert)
            {
                logType = LogType.Error;
            }

            logNodes.Enqueue(LogNode.Create(logType, logMessage, stackTrace));
            while (logNodes.Count > maxLine)
            {
                ReferenceCache.Release(logNodes.Dequeue());
            }
        }

        private string GetLogString(LogNode logNode)
        {
            Color32 color = GetLogStringColor(logNode.LogType);
            return StringUtils.Format("<color=#{}{}{}{}>[{}][{}] {}</color>",
                color.r.ToString("x2"), color.g.ToString("x2"), color.b.ToString("x2"), color.a.ToString("x2"),
                logNode.LogTime.ToString("HH:mm:ss.fff"), logNode.LogFrameCount.ToString(), logNode.LogMessage);
        }

        public Color32 GetLogStringColor(LogType logType)
        {
            Color32 color = Color.white;
            switch (logType)
            {
                case LogType.Log:
                    color = infoColor;
                    break;

                case LogType.Warning:
                    color = warningColor;
                    break;

                case LogType.Error:
                    color = errorColor;
                    break;

                case LogType.Exception:
                    color = fatalColor;
                    break;
            }

            return color;
        }
    }
}