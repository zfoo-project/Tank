using Summer.Sound;
using Spring.Core;
using UnityEditor;

namespace Summer.Editor.Inspector
{
    [CustomEditor(typeof(SoundComponent))]
    sealed class SoundComponentInspector : GameFrameworkInspector
    {
        private SerializedProperty audioMixer = null;
        private SerializedProperty soundGroups = null;

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            serializedObject.Update();

            SoundComponent t = (SoundComponent)target;

            EditorGUI.BeginDisabledGroup(EditorApplication.isPlayingOrWillChangePlaymode);
            {
                EditorGUILayout.PropertyField(audioMixer);
                EditorGUILayout.PropertyField(soundGroups, true);
            }
            EditorGUI.EndDisabledGroup();

            if (EditorApplication.isPlaying && IsPrefabInHierarchy(t.gameObject))
            {
                var soundManager = SpringContext.GetBean<ISoundManager>();
                EditorGUILayout.LabelField("Sound Group Count", soundManager.SoundGroupCount.ToString());
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
            audioMixer = serializedObject.FindProperty("audioMixer");
            soundGroups = serializedObject.FindProperty("soundGroups");
            RefreshTypeNames();
        }

        private void RefreshTypeNames()
        {
            serializedObject.ApplyModifiedProperties();
        }
    }
}
