using Summer.Debugger;
using Spring.Core;
using Summer.Debugger.Manager;
using UnityEditor;
using UnityEngine;
using DebuggerComponent = Summer.Debugger.DebuggerComponent;

namespace Summer.Editor.Inspector
{
    [CustomEditor(typeof(DebuggerComponent))]
    sealed class DebuggerComponentInspector : GameFrameworkInspector
    {
        private SerializedProperty guiSkin = null;
        private SerializedProperty activeWindow = null;
        private SerializedProperty showFullWindow = null;
        private SerializedProperty consoleWindow = null;

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            serializedObject.Update();

            DebuggerComponent t = (DebuggerComponent)target;

            EditorGUILayout.PropertyField(guiSkin);

            if (EditorApplication.isPlaying && IsPrefabInHierarchy(t.gameObject))
            {
                var debuggerManager = SpringContext.GetBean<IDebuggerManager>();
                bool activeWindow = EditorGUILayout.Toggle("Active Window", debuggerManager.ActiveWindow);
                if (activeWindow != debuggerManager.ActiveWindow)
                {
                    debuggerManager.ActiveWindow = activeWindow;
                }
            }
            else
            {
                EditorGUILayout.PropertyField(activeWindow);
            }

            EditorGUILayout.PropertyField(showFullWindow);

            if (EditorApplication.isPlaying)
            {
                if (GUILayout.Button("Reset Layout"))
                {
                    t.ResetLayout();
                }
            }

            EditorGUILayout.PropertyField(consoleWindow, true);

            serializedObject.ApplyModifiedProperties();
        }

        private void OnEnable()
        {
            guiSkin = serializedObject.FindProperty("guiSkin");
            activeWindow = serializedObject.FindProperty("activeWindow");
            showFullWindow = serializedObject.FindProperty("showFullWindow");
            consoleWindow = serializedObject.FindProperty("consoleWindow");
        }
    }
}
