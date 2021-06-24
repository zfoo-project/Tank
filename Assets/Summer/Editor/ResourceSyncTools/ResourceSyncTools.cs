using Spring.Util;
using UnityEditor;
using UnityEngine;

namespace Summer.Editor.ResourceSyncTools
{
    /// <summary>
    /// 资源同步工具。
    /// </summary>
    sealed class ResourceSyncTools : EditorWindow
    {
        private const float ButtonHeight = 60f;
        private const float ButtonSpace = 5f;
        private ResourceSyncToolsController controller;

        [MenuItem("Summer/Resource Tools/Resource Sync Tools", false, 45)]
        private static void Open()
        {
            ResourceSyncTools window = GetWindow<ResourceSyncTools>("Resource Sync Tools", true);
            window.minSize = new Vector2(400, 195f);
        }

        private void OnEnable()
        {
            controller = new ResourceSyncToolsController();
            controller.OnLoadingResource += OnLoadingResource;
            controller.OnLoadingAsset += OnLoadingAsset;
            controller.OnCompleted += OnCompleted;
            controller.OnResourceDataChanged += OnResourceDataChanged;
        }

        private void OnGUI()
        {
            EditorGUILayout.BeginVertical(GUILayout.Width(position.width), GUILayout.Height(position.height));
            {
                GUILayout.Space(ButtonSpace);
                if (GUILayout.Button("Remove All Asset Bundle Names in Project", GUILayout.Height(ButtonHeight)))
                {
                    if (!controller.RemoveAllAssetBundleNames())
                    {
                        Debug.LogWarning("Remove All Asset Bundle Names in Project failure.");
                    }
                    else
                    {
                        Debug.Log("Remove All Asset Bundle Names in Project completed.");
                    }

                    AssetDatabase.Refresh();
                }

                GUILayout.Space(ButtonSpace);
                if (GUILayout.Button("Sync ResourceCollection.xml to Project", GUILayout.Height(ButtonHeight)))
                {
                    if (!controller.SyncToProject())
                    {
                        Debug.LogWarning("Sync ResourceCollection.xml to Project failure.");
                    }
                    else
                    {
                        Debug.Log("Sync ResourceCollection.xml to Project completed.");
                    }

                    AssetDatabase.Refresh();
                }

                GUILayout.Space(ButtonSpace);
                if (GUILayout.Button("Sync ResourceCollection.xml from Project", GUILayout.Height(ButtonHeight)))
                {
                    if (!controller.SyncFromProject())
                    {
                        Debug.LogWarning("Sync Project to ResourceCollection.xml failure.");
                    }
                    else
                    {
                        Debug.Log("Sync Project to ResourceCollection.xml completed.");
                    }

                    AssetDatabase.Refresh();
                }
            }
            EditorGUILayout.EndVertical();
        }

        private void OnLoadingResource(int index, int count)
        {
            EditorUtility.DisplayProgressBar("Loading Resources", StringUtils.Format("Loading resources, {}/{} loaded.", index.ToString(), count.ToString()), (float)index / count);
        }

        private void OnLoadingAsset(int index, int count)
        {
            EditorUtility.DisplayProgressBar("Loading Assets", StringUtils.Format("Loading assets, {}/{} loaded.", index.ToString(), count.ToString()), (float)index / count);
        }

        private void OnCompleted()
        {
            EditorUtility.ClearProgressBar();
        }

        private void OnResourceDataChanged(int index, int count, string assetName)
        {
            EditorUtility.DisplayProgressBar("Processing Assets", StringUtils.Format("({}/{}) {}", index.ToString(), count.ToString(), assetName), (float)index / count);
        }
    }
}
