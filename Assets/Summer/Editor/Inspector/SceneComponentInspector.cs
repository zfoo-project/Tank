using Summer.Scene;
using Spring.Core;
using UnityEditor;
using UnityEngine;

namespace Summer.Editor.Inspector
{
    [CustomEditor(typeof(SceneComponent))]
    sealed class SceneComponentInspector : GameFrameworkInspector
    {

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            
            if (!EditorApplication.isPlaying)
            {
                EditorGUILayout.HelpBox("Available during runtime only.", MessageType.Info);
                return;
            }

            serializedObject.Update();

            SceneComponent t = (SceneComponent)target;

            EditorGUI.EndDisabledGroup();

            serializedObject.ApplyModifiedProperties();

            if (EditorApplication.isPlaying && IsPrefabInHierarchy(t.gameObject))
            {
                var sceneManager = SpringContext.GetBean<ISceneManager>();
                EditorGUILayout.LabelField("Loaded Scene Asset Names", GetSceneNameString(sceneManager.GetLoadedSceneAssetNames()));
                EditorGUILayout.LabelField("Loading Scene Asset Names", GetSceneNameString(sceneManager.GetLoadingSceneAssetNames()));
                EditorGUILayout.LabelField("Unloading Scene Asset Names", GetSceneNameString(sceneManager.GetUnloadingSceneAssetNames()));
                EditorGUILayout.ObjectField("Main Camera", t.MainCamera, typeof(Camera), true);

                Repaint();
            }
        }


        private string GetSceneNameString(string[] sceneAssetNames)
        {
            if (sceneAssetNames == null || sceneAssetNames.Length <= 0)
            {
                return "<Empty>";
            }

            string sceneNameString = string.Empty;
            foreach (string sceneAssetName in sceneAssetNames)
            {
                if (!string.IsNullOrEmpty(sceneNameString))
                {
                    sceneNameString += ", ";
                }

                sceneNameString += SceneComponent.GetSceneName(sceneAssetName);
            }

            return sceneNameString;
        }
    }
}
