using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Summer.ObjectPool;
using Spring.Core;
using Spring.Util;
using UnityEditor;
using UnityEngine;

namespace Summer.Editor.Inspector
{
    [CustomEditor(typeof(ObjectPoolComponent))]
    sealed class ObjectPoolComponentInspector : GameFrameworkInspector
    {
        private readonly HashSet<string> m_OpenedItems = new HashSet<string>();

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            if (!EditorApplication.isPlaying)
            {
                EditorGUILayout.HelpBox("Available during runtime only.", MessageType.Info);
                return;
            }

            ObjectPoolComponent t = (ObjectPoolComponent)target;

            if (IsPrefabInHierarchy(t.gameObject))
            {
                var objectPoolManager = SpringContext.GetBean<IObjectPoolManager>();
                EditorGUILayout.LabelField("Object Pool Count", objectPoolManager.Count.ToString());

                ObjectPoolBase[] objectPools = objectPoolManager.GetAllObjectPools(true);
                foreach (ObjectPoolBase objectPool in objectPools)
                {
                    DrawObjectPool(objectPool);
                }
            }

            Repaint();
        }

        private void OnEnable()
        {
        }

        private void DrawObjectPool(ObjectPoolBase objectPool)
        {
            bool lastState = m_OpenedItems.Contains(objectPool.FullName);
            bool currentState = EditorGUILayout.Foldout(lastState, objectPool.FullName);
            if (currentState != lastState)
            {
                if (currentState)
                {
                    m_OpenedItems.Add(objectPool.FullName);
                }
                else
                {
                    m_OpenedItems.Remove(objectPool.FullName);
                }
            }

            if (currentState)
            {
                EditorGUILayout.BeginVertical("box");
                {
                    EditorGUILayout.LabelField("Name", objectPool.Name);
                    EditorGUILayout.LabelField("Type", objectPool.ObjectType.FullName);
                    EditorGUILayout.LabelField("Auto Release Interval", objectPool.AutoReleaseInterval.ToString());
                    EditorGUILayout.LabelField("Capacity", objectPool.Capacity.ToString());
                    EditorGUILayout.LabelField("Used Count", objectPool.Count.ToString());
                    EditorGUILayout.LabelField("Can Release Count", objectPool.CanReleaseCount.ToString());
                    EditorGUILayout.LabelField("Expire Time", objectPool.ExpireTime.ToString());
                    EditorGUILayout.LabelField("Priority", objectPool.Priority.ToString());
                    ObjectInfo[] objectInfos = objectPool.GetAllObjectInfos();
                    if (objectInfos.Length > 0)
                    {
                        EditorGUILayout.LabelField("Name", objectPool.AllowMultiSpawn ? "Locked\tCount\tFlag\tPriority\tLast Use Time" : "Locked\tIn Use\tFlag\tPriority\tLast Use Time");
                        foreach (ObjectInfo objectInfo in objectInfos)
                        {
                            EditorGUILayout.LabelField(string.IsNullOrEmpty(objectInfo.Name) ? "<None>" : objectInfo.Name, StringUtils.Format("{}\t{}\t{}\t{}\t{}", objectInfo.Locked.ToString(), objectPool.AllowMultiSpawn ? objectInfo.SpawnCount.ToString() : objectInfo.IsInUse.ToString(), objectInfo.CustomCanReleaseFlag.ToString(), objectInfo.Priority.ToString(), objectInfo.LastUseTime.ToString("yyyy-MM-dd HH:mm:ss")));
                        }

                        if (GUILayout.Button("Release"))
                        {
                            objectPool.Release();
                        }

                        if (GUILayout.Button("Release All Unused"))
                        {
                            objectPool.ReleaseAllUnused();
                        }

                        if (GUILayout.Button("Export CSV Data"))
                        {
                            string exportFileName = EditorUtility.SaveFilePanel("Export CSV Data", string.Empty, StringUtils.Format("Object Pool Data - {}.csv", objectPool.Name), string.Empty);
                            if (!string.IsNullOrEmpty(exportFileName))
                            {
                                try
                                {
                                    int index = 0;
                                    string[] data = new string[objectInfos.Length + 1];
                                    data[index++] = StringUtils.Format("Name,Locked,{},Custom Can Release Flag,Priority,Last Use Time", objectPool.AllowMultiSpawn ? "Count" : "In Use");
                                    foreach (ObjectInfo objectInfo in objectInfos)
                                    {
                                        data[index++] = StringUtils.Format("{},{},{},{},{},{}"
                                            , objectInfo.Name, objectInfo.Locked.ToString()
                                            , objectPool.AllowMultiSpawn ? objectInfo.SpawnCount.ToString() : objectInfo.IsInUse.ToString()
                                            , objectInfo.CustomCanReleaseFlag.ToString(), objectInfo.Priority.ToString()
                                            , objectInfo.LastUseTime.ToString("yyyy-MM-dd HH:mm:ss"));
                                    }

                                    File.WriteAllLines(exportFileName, data, Encoding.UTF8);
                                    Debug.Log(StringUtils.Format("Export object pool CSV data to '{}' success.", exportFileName));
                                }
                                catch (Exception exception)
                                {
                                    Debug.LogError(StringUtils.Format("Export object pool CSV data to '{}' failure, exception is '{}'.", exportFileName, exception.ToString()));
                                }
                            }
                        }
                    }
                    else
                    {
                        GUILayout.Label("Object Pool is Empty ...");
                    }
                }
                EditorGUILayout.EndVertical();

                EditorGUILayout.Separator();
            }
        }
    }
}
