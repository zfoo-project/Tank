using Summer.FileSystem;
using Summer.FileSystem.Model;
using Spring.Core;
using Spring.Util;
using UnityEditor;

namespace Summer.Editor.Inspector
{
    [CustomEditor(typeof(FileSystemComponent))]
    sealed class FileSystemComponentInspector : GameFrameworkInspector
    {

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            if (!EditorApplication.isPlaying)
            {
                EditorGUILayout.HelpBox("Available during runtime only.", MessageType.Info);
                return;
            }
            
            FileSystemComponent t = (FileSystemComponent)target;

            EditorGUI.EndDisabledGroup();

            if (EditorApplication.isPlaying && IsPrefabInHierarchy(t.gameObject))
            {
                var fileSystemManager = SpringContext.GetBean<IFileSystemManager>();
                EditorGUILayout.LabelField("File System Count", fileSystemManager.Count.ToString());

                IFileSystem[] fileSystems = fileSystemManager.GetAllFileSystems();
                foreach (IFileSystem fileSystem in fileSystems)
                {
                    DrawFileSystem(fileSystem);
                }
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
            RefreshTypeNames();
        }

        private void RefreshTypeNames()
        {
            serializedObject.ApplyModifiedProperties();
        }

        private void DrawFileSystem(IFileSystem fileSystem)
        {
            EditorGUILayout.LabelField(fileSystem.FullPath, StringUtils.Format("{}, {} / {} Files", fileSystem.Access.ToString(), fileSystem.FileCount.ToString(), fileSystem.MaxFileCount.ToString()));
        }
    }
}
