using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Summer.ReferencePool;
using Spring.Collection.Reference;
using Spring.Util;
using UnityEditor;
using UnityEngine;

namespace Summer.Editor.Inspector
{
    [CustomEditor(typeof(ReferencePoolComponent))]
    sealed class ReferencePoolComponentInspector : GameFrameworkInspector
    {
        private readonly Dictionary<string, List<ReferenceCacheInfo>> m_ReferencePoolInfos = new Dictionary<string, List<ReferenceCacheInfo>>(StringComparer.Ordinal);
        private readonly HashSet<string> m_OpenedItems = new HashSet<string>();

        private bool m_ShowFullClassName = false;

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            if (!EditorApplication.isPlaying)
            {
                EditorGUILayout.HelpBox("Available during runtime only.", MessageType.Info);
                return;
            }
            
            serializedObject.Update();

            ReferencePoolComponent t = (ReferencePoolComponent)target;

            if (EditorApplication.isPlaying && IsPrefabInHierarchy(t.gameObject))
            {

                EditorGUILayout.LabelField("Reference Pool Count", ReferenceCache.Count.ToString());
                m_ShowFullClassName = EditorGUILayout.Toggle("Show Full Class Name", m_ShowFullClassName);
                m_ReferencePoolInfos.Clear();
                ReferenceCacheInfo[] referencePoolInfos = ReferenceCache.GetAllReferencePoolInfos();
                foreach (ReferenceCacheInfo referencePoolInfo in referencePoolInfos)
                {
                    string assemblyName = referencePoolInfo.Type.Assembly.GetName().Name;
                    List<ReferenceCacheInfo> results = null;
                    if (!m_ReferencePoolInfos.TryGetValue(assemblyName, out results))
                    {
                        results = new List<ReferenceCacheInfo>();
                        m_ReferencePoolInfos.Add(assemblyName, results);
                    }

                    results.Add(referencePoolInfo);
                }

                foreach (KeyValuePair<string, List<ReferenceCacheInfo>> assemblyReferencePoolInfo in m_ReferencePoolInfos)
                {
                    bool lastState = m_OpenedItems.Contains(assemblyReferencePoolInfo.Key);
                    bool currentState = EditorGUILayout.Foldout(lastState, assemblyReferencePoolInfo.Key);
                    if (currentState != lastState)
                    {
                        if (currentState)
                        {
                            m_OpenedItems.Add(assemblyReferencePoolInfo.Key);
                        }
                        else
                        {
                            m_OpenedItems.Remove(assemblyReferencePoolInfo.Key);
                        }
                    }

                    if (currentState)
                    {
                        EditorGUILayout.BeginVertical("box");
                        {
                            EditorGUILayout.LabelField(m_ShowFullClassName ? "Full Class Name" : "Class Name", "Unused\tUsing\tAcquire\tRelease\tAdd\tRemove");
                            assemblyReferencePoolInfo.Value.Sort(Comparison);
                            foreach (ReferenceCacheInfo referencePoolInfo in assemblyReferencePoolInfo.Value)
                            {
                                DrawReferencePoolInfo(referencePoolInfo);
                            }

                            if (GUILayout.Button("Export CSV Data"))
                            {
                                string exportFileName = EditorUtility.SaveFilePanel("Export CSV Data", string.Empty, StringUtils.Format("Reference Pool Data - {}.csv", assemblyReferencePoolInfo.Key), string.Empty);
                                if (!string.IsNullOrEmpty(exportFileName))
                                {
                                    try
                                    {
                                        int index = 0;
                                        string[] data = new string[assemblyReferencePoolInfo.Value.Count + 1];
                                        data[index++] = "Class Name,Full Class Name,Unused,Using,Acquire,Release,Add,Remove";
                                        foreach (ReferenceCacheInfo referencePoolInfo in assemblyReferencePoolInfo.Value)
                                        {
                                            data[index++] = StringUtils.Format("{},{},{},{},{},{},{},{}", referencePoolInfo.Type.Name, referencePoolInfo.Type.FullName, referencePoolInfo.UnusedReferenceCount.ToString(), referencePoolInfo.UsingReferenceCount.ToString(), referencePoolInfo.AcquireReferenceCount.ToString(), referencePoolInfo.ReleaseReferenceCount.ToString(), referencePoolInfo.AddReferenceCount.ToString(), referencePoolInfo.RemoveReferenceCount.ToString());
                                        }

                                        File.WriteAllLines(exportFileName, data, Encoding.UTF8);
                                        Debug.Log(StringUtils.Format("Export reference pool CSV data to '{}' success.", exportFileName));
                                    }
                                    catch (Exception exception)
                                    {
                                        Debug.LogError(StringUtils.Format("Export reference pool CSV data to '{}' failure, exception is '{}'.", exportFileName, exception.ToString()));
                                    }
                                }
                            }
                        }
                        EditorGUILayout.EndVertical();

                        EditorGUILayout.Separator();
                    }
                }
            }

            serializedObject.ApplyModifiedProperties();

            Repaint();
        }

        private void DrawReferencePoolInfo(ReferenceCacheInfo referenceCacheInfo)
        {
            EditorGUILayout.LabelField(m_ShowFullClassName ? referenceCacheInfo.Type.FullName : referenceCacheInfo.Type.Name, StringUtils.Format("{}\t{}\t{}\t{}\t{}\t{}", referenceCacheInfo.UnusedReferenceCount.ToString(), referenceCacheInfo.UsingReferenceCount.ToString(), referenceCacheInfo.AcquireReferenceCount.ToString(), referenceCacheInfo.ReleaseReferenceCount.ToString(), referenceCacheInfo.AddReferenceCount.ToString(), referenceCacheInfo.RemoveReferenceCount.ToString()));
        }

        private int Comparison(ReferenceCacheInfo a, ReferenceCacheInfo b)
        {
            if (m_ShowFullClassName)
            {
                return a.Type.FullName.CompareTo(b.Type.FullName);
            }
            else
            {
                return a.Type.Name.CompareTo(b.Type.Name);
            }
        }
    }
}
