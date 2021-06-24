using UnityEditor;
using UnityEngine;

namespace Summer.Editor.Misc
{
    public class GameTools
    {
        [MenuItem("Summer/Clear All Prefs", false, 1000)]
        public static void ClearAllPrefs()
        {
            PlayerPrefs.DeleteAll();
        }

        [MenuItem("Summer/Clear Missing Script", false, 1001)]
        public static void ClearMissingScript()
        {
            if (Selection.activeGameObject == null)
            {
                return;
            }

            var items = Selection.activeGameObject.GetComponentsInChildren<Transform>(true);
            foreach (var item in items)
            {
                GameObjectUtility.RemoveMonoBehavioursWithMissingScript(item.gameObject);
            }

            AssetDatabase.Refresh();
            Debug.Log("清理完成!");
        }
    }
}