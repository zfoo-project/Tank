using System.Collections.Generic;
using System.Linq;
using Summer.Procedure;
using Spring.Core;
using Spring.Util;
using UnityEditor;
using UnityEngine;

namespace Summer.Editor.Inspector
{
    [CustomEditor(typeof(ProcedureComponent))]
    sealed class ProcedureComponentInspector : GameFrameworkInspector
    {
        private SerializedProperty availableProcedureTypeNames = null;
        private SerializedProperty entranceProcedureTypeName = null;

        private string[] m_ProcedureTypeNames = null;
        private List<string> m_CurrentAvailableProcedureTypeNames = null;
        private int m_EntranceProcedureIndex = -1;

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            serializedObject.Update();

            ProcedureComponent t = (ProcedureComponent)target;

            if (string.IsNullOrEmpty(entranceProcedureTypeName.stringValue))
            {
                EditorGUILayout.HelpBox("Entrance procedure is invalid.", MessageType.Error);
            }
            else if (EditorApplication.isPlaying)
            {
                var currentProcedure = SpringContext.GetBean<IProcedureFsmManager>().ProcedureFsm.CurrentState;
                EditorGUILayout.LabelField("Current Procedure", currentProcedure == null ? "None" : currentProcedure.GetType().ToString());
            }

            EditorGUI.BeginDisabledGroup(EditorApplication.isPlayingOrWillChangePlaymode);
            {
                GUILayout.Label("Available Procedures", EditorStyles.boldLabel);
                if (m_ProcedureTypeNames.Length > 0)
                {
                    EditorGUILayout.BeginVertical("box");
                    {
                        foreach (string procedureTypeName in m_ProcedureTypeNames)
                        {
                            bool selected = m_CurrentAvailableProcedureTypeNames.Contains(procedureTypeName);
                            if (selected != EditorGUILayout.ToggleLeft(procedureTypeName, selected))
                            {
                                if (!selected)
                                {
                                    m_CurrentAvailableProcedureTypeNames.Add(procedureTypeName);
                                    WriteAvailableProcedureTypeNames();
                                }
                                else if (procedureTypeName != entranceProcedureTypeName.stringValue)
                                {
                                    m_CurrentAvailableProcedureTypeNames.Remove(procedureTypeName);
                                    WriteAvailableProcedureTypeNames();
                                }
                            }
                        }
                    }
                    EditorGUILayout.EndVertical();
                }
                else
                {
                    EditorGUILayout.HelpBox("There is no available procedure.", MessageType.Warning);
                }

                if (m_CurrentAvailableProcedureTypeNames.Count > 0)
                {
                    EditorGUILayout.Separator();

                    int selectedIndex = EditorGUILayout.Popup("Entrance Procedure", m_EntranceProcedureIndex, m_CurrentAvailableProcedureTypeNames.ToArray());
                    if (selectedIndex != m_EntranceProcedureIndex)
                    {
                        m_EntranceProcedureIndex = selectedIndex;
                        entranceProcedureTypeName.stringValue = m_CurrentAvailableProcedureTypeNames[selectedIndex];
                    }
                }
                else
                {
                    EditorGUILayout.HelpBox("Select available procedures first.", MessageType.Info);
                }
            }
            EditorGUI.EndDisabledGroup();

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
            availableProcedureTypeNames = serializedObject.FindProperty("availableProcedureTypeNames");
            entranceProcedureTypeName = serializedObject.FindProperty("entranceProcedureTypeName");

            RefreshTypeNames();
        }

        private void RefreshTypeNames()
        {
            m_ProcedureTypeNames = AssemblyUtils.GetAllSubClassNames(typeof(FsmState<IProcedureFsmManager>));
            ReadAvailableProcedureTypeNames();
            int oldCount = m_CurrentAvailableProcedureTypeNames.Count;
            m_CurrentAvailableProcedureTypeNames = m_CurrentAvailableProcedureTypeNames.Where(x => m_ProcedureTypeNames.Contains(x)).ToList();
            if (m_CurrentAvailableProcedureTypeNames.Count != oldCount)
            {
                WriteAvailableProcedureTypeNames();
            }
            else if (!string.IsNullOrEmpty(entranceProcedureTypeName.stringValue))
            {
                m_EntranceProcedureIndex = m_CurrentAvailableProcedureTypeNames.IndexOf(entranceProcedureTypeName.stringValue);
                if (m_EntranceProcedureIndex < 0)
                {
                    entranceProcedureTypeName.stringValue = null;
                }
            }

            serializedObject.ApplyModifiedProperties();
        }

        private void ReadAvailableProcedureTypeNames()
        {
            m_CurrentAvailableProcedureTypeNames = new List<string>();
            int count = availableProcedureTypeNames.arraySize;
            for (int i = 0; i < count; i++)
            {
                m_CurrentAvailableProcedureTypeNames.Add(availableProcedureTypeNames.GetArrayElementAtIndex(i).stringValue);
            }
        }

        private void WriteAvailableProcedureTypeNames()
        {
            availableProcedureTypeNames.ClearArray();
            if (m_CurrentAvailableProcedureTypeNames == null)
            {
                return;
            }

            m_CurrentAvailableProcedureTypeNames.Sort();
            int count = m_CurrentAvailableProcedureTypeNames.Count;
            for (int i = 0; i < count; i++)
            {
                availableProcedureTypeNames.InsertArrayElementAtIndex(i);
                availableProcedureTypeNames.GetArrayElementAtIndex(i).stringValue = m_CurrentAvailableProcedureTypeNames[i];
            }

            if (!string.IsNullOrEmpty(entranceProcedureTypeName.stringValue))
            {
                m_EntranceProcedureIndex = m_CurrentAvailableProcedureTypeNames.IndexOf(entranceProcedureTypeName.stringValue);
                if (m_EntranceProcedureIndex < 0)
                {
                    entranceProcedureTypeName.stringValue = null;
                }
            }
        }
    }
}
