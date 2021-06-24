using System;
using System.IO;
using System.Text;
using Summer.Base.TaskPool;
using Summer.Download;
using Spring.Core;
using Spring.Util;
using UnityEditor;
using UnityEngine;

namespace Summer.Editor.Inspector
{
    [CustomEditor(typeof(DownloadComponent))]
    sealed class DownloadComponentInspector : GameFrameworkInspector
    {
        private SerializedProperty downloadAgentHelperCount;
        private SerializedProperty timeout;
        private SerializedProperty flushSize;


        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            serializedObject.Update();

            DownloadComponent t = (DownloadComponent)target;

            EditorGUI.BeginDisabledGroup(EditorApplication.isPlayingOrWillChangePlaymode);
            {
                downloadAgentHelperCount.intValue = EditorGUILayout.IntSlider("Download Agent Helper Count", downloadAgentHelperCount.intValue, 1, 16);
            }
            EditorGUI.EndDisabledGroup();

            float timeout = EditorGUILayout.Slider("Timeout", this.timeout.floatValue, 0f, 120f);
            if (timeout != this.timeout.floatValue)
            {
                if (EditorApplication.isPlaying)
                {
                    t.Timeout = timeout;
                }
                else
                {
                    this.timeout.floatValue = timeout;
                }
            }

            int flushSize = EditorGUILayout.DelayedIntField("Flush Size", this.flushSize.intValue);
            if (flushSize != this.flushSize.intValue)
            {
                if (EditorApplication.isPlaying)
                {
                    t.FlushSize = flushSize;
                }
                else
                {
                    this.flushSize.intValue = flushSize;
                }
            }

            if (EditorApplication.isPlaying && IsPrefabInHierarchy(t.gameObject))
            {
                var downloadManager = SpringContext.GetBean<IDownloadManager>();
                EditorGUILayout.LabelField("Total Agent Count", downloadManager.TotalAgentCount.ToString());
                EditorGUILayout.LabelField("Free Agent Count", downloadManager.FreeAgentCount.ToString());
                EditorGUILayout.LabelField("Working Agent Count", downloadManager.WorkingAgentCount.ToString());
                EditorGUILayout.LabelField("Waiting Agent Count", downloadManager.WaitingTaskCount.ToString());
                EditorGUILayout.LabelField("Current Speed", downloadManager.CurrentSpeed.ToString());
                EditorGUILayout.BeginVertical("box");
                {
                    TaskInfo[] downloadInfos = t.GetAllDownloadInfos();
                    if (downloadInfos.Length > 0)
                    {
                        foreach (TaskInfo downloadInfo in downloadInfos)
                        {
                            DrawDownloadInfo(downloadInfo);
                        }

                        if (GUILayout.Button("Export CSV Data"))
                        {
                            string exportFileName = EditorUtility.SaveFilePanel("Export CSV Data", string.Empty, "Download Task Data.csv", string.Empty);
                            if (!string.IsNullOrEmpty(exportFileName))
                            {
                                try
                                {
                                    int index = 0;
                                    string[] data = new string[downloadInfos.Length + 1];
                                    data[index++] = "Download Path,Serial Id,Priority,Status";
                                    foreach (TaskInfo downloadInfo in downloadInfos)
                                    {
                                        data[index++] = StringUtils.Format("{},{},{},{}", downloadInfo.Description, downloadInfo.SerialId.ToString(), downloadInfo.Priority.ToString(), downloadInfo.Status.ToString());
                                    }

                                    File.WriteAllLines(exportFileName, data, Encoding.UTF8);
                                    Debug.Log(StringUtils.Format("Export download task CSV data to '{}' success.", exportFileName));
                                }
                                catch (Exception exception)
                                {
                                    Debug.LogError(StringUtils.Format("Export download task CSV data to '{}' failure, exception is '{}'.", exportFileName, exception.ToString()));
                                }
                            }
                        }
                    }
                    else
                    {
                        GUILayout.Label("Download Task is Empty ...");
                    }
                }
                EditorGUILayout.EndVertical();
            }

            serializedObject.ApplyModifiedProperties();

            Repaint();
        }

        protected override void OnCompileComplete()
        {
            base.OnCompileComplete();

            RefreshTypeNames();
        }

        private void OnEnable()
        {
            downloadAgentHelperCount = serializedObject.FindProperty("downloadAgentHelperCount");
            timeout = serializedObject.FindProperty("timeout");
            flushSize = serializedObject.FindProperty("flushSize");

            RefreshTypeNames();
        }

        private void DrawDownloadInfo(TaskInfo downloadInfo)
        {
            EditorGUILayout.LabelField(downloadInfo.Description, StringUtils.Format("[SerialId]{} [Priority]{} [Status]{}", downloadInfo.SerialId.ToString(), downloadInfo.Priority.ToString(), downloadInfo.Status.ToString()));
        }

        private void RefreshTypeNames()
        {
            serializedObject.ApplyModifiedProperties();
        }
    }
}
