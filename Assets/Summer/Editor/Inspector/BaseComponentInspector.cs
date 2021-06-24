using System.Collections.Generic;
using Summer.Base;
using Spring.Logger;
using Spring.Storage.Helper;
using Spring.Util;
using Spring.Util.Json;
using Spring.Util.Zip;
using UnityEditor;
using UnityEngine;

namespace Summer.Editor.Inspector
{
    [CustomEditor(typeof(BaseComponent))]
    sealed class BaseComponentInspector : GameFrameworkInspector
    {
        public static readonly string NoneOptionName = "<None>";
        private static readonly float[] GameSpeed = new float[] {0f, 0.01f, 0.1f, 0.25f, 0.5f, 1f, 1.5f, 2f, 4f, 8f};
        private static readonly string[] GameSpeedForDisplay = new string[] {"0x", "0.01x", "0.1x", "0.25x", "0.5x", "1x", "1.5x", "2x", "4x", "8x"};

        private SerializedProperty editorResourceMode;
        private SerializedProperty logHelperTypeName;
        private SerializedProperty zipHelperTypeName;
        private SerializedProperty jsonHelperTypeName;
        private SerializedProperty storageHelperTypeName;
        private SerializedProperty frameRate;
        private SerializedProperty gameSpeed;
        private SerializedProperty runInBackground;
        private SerializedProperty neverSleep;

        private string[] logHelperTypeNames;
        private int logHelperTypeNameIndex;
        private string[] zipHelperTypeNames;
        private int zipHelperTypeNameIndex;
        private string[] jsonHelperTypeNames;
        private int jsonHelperTypeNameIndex;
        private string[] storageHelperTypeNames;
        private int storageHelperTypeNameIndex;

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            serializedObject.Update();

            BaseComponent t = (BaseComponent) target;

            EditorGUI.BeginDisabledGroup(EditorApplication.isPlayingOrWillChangePlaymode);
            {
                editorResourceMode.boolValue = EditorGUILayout.BeginToggleGroup("Editor Resource Mode", editorResourceMode.boolValue);
                EditorGUILayout.EndToggleGroup();

                EditorGUILayout.BeginVertical("box");
                {
                    EditorGUILayout.LabelField("Global Helpers", EditorStyles.boldLabel);

                    int logHelperSelectedIndex = EditorGUILayout.Popup("Log Helper", logHelperTypeNameIndex, logHelperTypeNames);
                    if (logHelperSelectedIndex != logHelperTypeNameIndex)
                    {
                        logHelperTypeNameIndex = logHelperSelectedIndex;
                        logHelperTypeName.stringValue = logHelperSelectedIndex <= 0 ? null : logHelperTypeNames[logHelperSelectedIndex];
                    }

                    int zipHelperSelectedIndex = EditorGUILayout.Popup("Zip Helper", zipHelperTypeNameIndex, zipHelperTypeNames);
                    if (zipHelperSelectedIndex != zipHelperTypeNameIndex)
                    {
                        zipHelperTypeNameIndex = zipHelperSelectedIndex;
                        zipHelperTypeName.stringValue = zipHelperSelectedIndex <= 0 ? null : zipHelperTypeNames[zipHelperSelectedIndex];
                    }

                    int jsonHelperSelectedIndex = EditorGUILayout.Popup("JSON Helper", jsonHelperTypeNameIndex, jsonHelperTypeNames);
                    if (jsonHelperSelectedIndex != jsonHelperTypeNameIndex)
                    {
                        jsonHelperTypeNameIndex = jsonHelperSelectedIndex;
                        jsonHelperTypeName.stringValue = jsonHelperSelectedIndex <= 0 ? null : jsonHelperTypeNames[jsonHelperSelectedIndex];
                    }

                    int storageHelperSelectedIndex = EditorGUILayout.Popup("Storage Helper", storageHelperTypeNameIndex, storageHelperTypeNames);
                    if (storageHelperSelectedIndex != storageHelperTypeNameIndex)
                    {
                        storageHelperTypeNameIndex = storageHelperSelectedIndex;
                        storageHelperTypeName.stringValue = storageHelperSelectedIndex <= 0 ? null : storageHelperTypeNames[storageHelperSelectedIndex];
                    }
                }
                EditorGUILayout.EndVertical();
            }
            EditorGUI.EndDisabledGroup();

            int frameRate = EditorGUILayout.IntSlider("Frame Rate", this.frameRate.intValue, 1, 120);
            if (frameRate != this.frameRate.intValue)
            {
                if (EditorApplication.isPlaying)
                {
                    t.FrameRate = frameRate;
                }
                else
                {
                    this.frameRate.intValue = frameRate;
                }
            }

            EditorGUILayout.BeginVertical("box");
            {
                float gameSpeed = EditorGUILayout.Slider("Game Speed", this.gameSpeed.floatValue, 0f, 8f);
                int selectedGameSpeed = GUILayout.SelectionGrid(GetSelectedGameSpeed(gameSpeed), GameSpeedForDisplay, 5);
                if (selectedGameSpeed >= 0)
                {
                    gameSpeed = GetGameSpeed(selectedGameSpeed);
                }

                if (gameSpeed != this.gameSpeed.floatValue)
                {
                    if (EditorApplication.isPlaying)
                    {
                        t.GameSpeed = gameSpeed;
                    }
                    else
                    {
                        this.gameSpeed.floatValue = gameSpeed;
                    }
                }
            }
            EditorGUILayout.EndVertical();

            bool runInBackground = EditorGUILayout.Toggle("Run in Background", this.runInBackground.boolValue);
            if (runInBackground != this.runInBackground.boolValue)
            {
                if (EditorApplication.isPlaying)
                {
                    t.RunInBackground = runInBackground;
                }
                else
                {
                    this.runInBackground.boolValue = runInBackground;
                }
            }

            bool neverSleep = EditorGUILayout.Toggle("Never Sleep", this.neverSleep.boolValue);
            if (neverSleep != this.neverSleep.boolValue)
            {
                if (EditorApplication.isPlaying)
                {
                    t.NeverSleep = neverSleep;
                }
                else
                {
                    this.neverSleep.boolValue = neverSleep;
                }
            }

            serializedObject.ApplyModifiedProperties();
        }

        protected override void OnCompileComplete()
        {
            base.OnCompileComplete();

            RefreshTypeNames();
        }

        private void OnEnable()
        {
            editorResourceMode = serializedObject.FindProperty("editorResourceMode");
            logHelperTypeName = serializedObject.FindProperty("logHelperTypeName");
            zipHelperTypeName = serializedObject.FindProperty("zipHelperTypeName");
            jsonHelperTypeName = serializedObject.FindProperty("jsonHelperTypeName");
            storageHelperTypeName = serializedObject.FindProperty("storageHelperTypeName");
            frameRate = serializedObject.FindProperty("frameRate");
            gameSpeed = serializedObject.FindProperty("gameSpeed");
            runInBackground = serializedObject.FindProperty("runInBackground");
            neverSleep = serializedObject.FindProperty("neverSleep");

            RefreshTypeNames();
        }

        private void RefreshTypeNames()
        {
            // log helper
            List<string> logHelperTypeNameList = new List<string>
            {
                NoneOptionName
            };

            logHelperTypeNameList.AddRange(AssemblyUtils.GetAllSubClassNames(typeof(ILogHelper)));
            logHelperTypeNames = logHelperTypeNameList.ToArray();
            logHelperTypeNameIndex = 0;
            if (!string.IsNullOrEmpty(logHelperTypeName.stringValue))
            {
                logHelperTypeNameIndex = logHelperTypeNameList.IndexOf(logHelperTypeName.stringValue);
                if (logHelperTypeNameIndex <= 0)
                {
                    logHelperTypeNameIndex = 0;
                    logHelperTypeName.stringValue = null;
                }
            }

            // zip helper
            List<string> zipHelperTypeNameList = new List<string>
            {
                NoneOptionName
            };

            zipHelperTypeNameList.AddRange(AssemblyUtils.GetAllSubClassNames(typeof(IZipHelper)));
            zipHelperTypeNames = zipHelperTypeNameList.ToArray();
            zipHelperTypeNameIndex = 0;
            if (!string.IsNullOrEmpty(zipHelperTypeName.stringValue))
            {
                zipHelperTypeNameIndex = zipHelperTypeNameList.IndexOf(zipHelperTypeName.stringValue);
                if (zipHelperTypeNameIndex <= 0)
                {
                    zipHelperTypeNameIndex = 0;
                    zipHelperTypeName.stringValue = null;
                }
            }

            // json helper
            List<string> jsonHelperTypeNameList = new List<string>
            {
                NoneOptionName
            };

            jsonHelperTypeNameList.AddRange(AssemblyUtils.GetAllSubClassNames(typeof(IJsonHelper)));
            jsonHelperTypeNames = jsonHelperTypeNameList.ToArray();
            jsonHelperTypeNameIndex = 0;
            if (!string.IsNullOrEmpty(jsonHelperTypeName.stringValue))
            {
                jsonHelperTypeNameIndex = jsonHelperTypeNameList.IndexOf(jsonHelperTypeName.stringValue);
                if (jsonHelperTypeNameIndex <= 0)
                {
                    jsonHelperTypeNameIndex = 0;
                    jsonHelperTypeName.stringValue = null;
                }
            }

            // storage helper
            List<string> storageHelperTypeNameList = new List<string>
            {
                NoneOptionName
            };

            storageHelperTypeNameList.AddRange(AssemblyUtils.GetAllSubClassNames(typeof(IStorageHelper)));
            storageHelperTypeNames = storageHelperTypeNameList.ToArray();
            storageHelperTypeNameIndex = 0;
            if (!string.IsNullOrEmpty(storageHelperTypeName.stringValue))
            {
                storageHelperTypeNameIndex = storageHelperTypeNameList.IndexOf(storageHelperTypeName.stringValue);
                if (storageHelperTypeNameIndex <= 0)
                {
                    storageHelperTypeNameIndex = 0;
                    storageHelperTypeName.stringValue = null;
                }
            }

            serializedObject.ApplyModifiedProperties();
        }

        private float GetGameSpeed(int selectedGameSpeed)
        {
            if (selectedGameSpeed < 0)
            {
                return GameSpeed[0];
            }

            if (selectedGameSpeed >= GameSpeed.Length)
            {
                return GameSpeed[GameSpeed.Length - 1];
            }

            return GameSpeed[selectedGameSpeed];
        }

        private int GetSelectedGameSpeed(float gameSpeed)
        {
            for (int i = 0; i < GameSpeed.Length; i++)
            {
                if (gameSpeed == GameSpeed[i])
                {
                    return i;
                }
            }

            return -1;
        }
    }
}