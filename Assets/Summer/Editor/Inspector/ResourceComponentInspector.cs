using System;
using System.IO;
using System.Text;
using Summer.Base;
using Summer.Base.TaskPool;
using Summer.Resource;
using Summer.Resource.Model.Constant;
using Spring.Core;
using Spring.Util;
using UnityEditor;
using UnityEngine;

namespace Summer.Editor.Inspector
{
    [CustomEditor(typeof(ResourceComponent))]
    sealed class ResourceComponentInspector : GameFrameworkInspector
    {
        private static readonly string[] ResourceModeNames = new string[] { "Package", "Updatable" };

        private SerializedProperty resourceMode;
        private SerializedProperty readWritePathType;
        private SerializedProperty unloadUnusedAssetsInterval;
        private SerializedProperty assetAutoReleaseInterval;
        private SerializedProperty assetCapacity;
        private SerializedProperty assetExpireTime;
        private SerializedProperty assetPriority;
        private SerializedProperty resourceAutoReleaseInterval;
        private SerializedProperty resourceCapacity;
        private SerializedProperty resourceExpireTime;
        private SerializedProperty resourcePriority;
        private SerializedProperty generateReadWriteVersionListLength;
        private SerializedProperty updateRetryCount;
        private SerializedProperty loadResourceAgentHelperCount;


        private int resourceModeIndex;

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            serializedObject.Update();

            ResourceComponent t = (ResourceComponent)target;

            bool isEditorResourceMode = BaseComponent.INSTANCE.editorResourceMode;

            if (isEditorResourceMode)
            {
                EditorGUILayout.HelpBox("Editor resource mode is enabled. Some options are disabled.", MessageType.Warning);
            }

            EditorGUI.BeginDisabledGroup(EditorApplication.isPlayingOrWillChangePlaymode);
            {
                if (EditorApplication.isPlaying && IsPrefabInHierarchy(t.gameObject))
                {
                    EditorGUILayout.EnumPopup("Resource Mode", SpringContext.GetBean<IResourceManager>().ResourceMode);
                }
                else
                {
                    int selectedIndex = EditorGUILayout.Popup("Resource Mode", resourceModeIndex, ResourceModeNames);
                    if (selectedIndex != resourceModeIndex)
                    {
                        resourceModeIndex = selectedIndex;
                        resourceMode.enumValueIndex = selectedIndex + 1;
                    }
                }

                readWritePathType.enumValueIndex = (int)(ReadWritePathType)EditorGUILayout.EnumPopup("Read Write Path Type", t.readWritePathType);
            }
            EditorGUI.EndDisabledGroup();

            float unloadUnusedAssetsInterval = EditorGUILayout.Slider("Unload Unused Assets Interval", this.unloadUnusedAssetsInterval.floatValue, 0f, 3600f);
            if (unloadUnusedAssetsInterval != this.unloadUnusedAssetsInterval.floatValue)
            {
                if (EditorApplication.isPlaying)
                {
                    t.unloadUnusedAssetsInterval = unloadUnusedAssetsInterval;
                }
                else
                {
                    this.unloadUnusedAssetsInterval.floatValue = unloadUnusedAssetsInterval;
                }
            }
            EditorGUI.BeginDisabledGroup(EditorApplication.isPlaying && isEditorResourceMode);
            {
                float assetAutoReleaseInterval = EditorGUILayout.DelayedFloatField("Asset Auto Release Interval", this.assetAutoReleaseInterval.floatValue);
                if (assetAutoReleaseInterval != this.assetAutoReleaseInterval.floatValue)
                {
                    if (EditorApplication.isPlaying)
                    {
                        SpringContext.GetBean<IResourceManager>().AssetAutoReleaseInterval = assetAutoReleaseInterval;
                    }
                    else
                    {
                        this.assetAutoReleaseInterval.floatValue = assetAutoReleaseInterval;
                    }
                }

                int assetCapacity = EditorGUILayout.DelayedIntField("Asset Capacity", this.assetCapacity.intValue);
                if (assetCapacity != this.assetCapacity.intValue)
                {
                    if (EditorApplication.isPlaying)
                    {
                        SpringContext.GetBean<IResourceManager>().AssetCapacity = assetCapacity;
                    }
                    else
                    {
                        this.assetCapacity.intValue = assetCapacity;
                    }
                }

                float assetExpireTime = EditorGUILayout.DelayedFloatField("Asset Expire Time", this.assetExpireTime.floatValue);
                if (assetExpireTime != this.assetExpireTime.floatValue)
                {
                    if (EditorApplication.isPlaying)
                    {
                        SpringContext.GetBean<IResourceManager>().AssetExpireTime = assetExpireTime;
                    }
                    else
                    {
                        this.assetExpireTime.floatValue = assetExpireTime;
                    }
                }

                int assetPriority = EditorGUILayout.DelayedIntField("Asset Priority", this.assetPriority.intValue);
                if (assetPriority != this.assetPriority.intValue)
                {
                    if (EditorApplication.isPlaying)
                    {
                        SpringContext.GetBean<IResourceManager>().AssetPriority = assetPriority;
                    }
                    else
                    {
                        this.assetPriority.intValue = assetPriority;
                    }
                }

                float resourceAutoReleaseInterval = EditorGUILayout.DelayedFloatField("Resource Auto Release Interval", this.resourceAutoReleaseInterval.floatValue);
                if (resourceAutoReleaseInterval != this.resourceAutoReleaseInterval.floatValue)
                {
                    if (EditorApplication.isPlaying)
                    {
                        SpringContext.GetBean<IResourceManager>().ResourceAutoReleaseInterval = resourceAutoReleaseInterval;
                    }
                    else
                    {
                        this.resourceAutoReleaseInterval.floatValue = resourceAutoReleaseInterval;
                    }
                }

                int resourceCapacity = EditorGUILayout.DelayedIntField("Resource Capacity", this.resourceCapacity.intValue);
                if (resourceCapacity != this.resourceCapacity.intValue)
                {
                    if (EditorApplication.isPlaying)
                    {
                        SpringContext.GetBean<IResourceManager>().ResourceCapacity = resourceCapacity;
                    }
                    else
                    {
                        this.resourceCapacity.intValue = resourceCapacity;
                    }
                }

                float resourceExpireTime = EditorGUILayout.DelayedFloatField("Resource Expire Time", this.resourceExpireTime.floatValue);
                if (resourceExpireTime != this.resourceExpireTime.floatValue)
                {
                    if (EditorApplication.isPlaying)
                    {
                        SpringContext.GetBean<IResourceManager>().ResourceExpireTime = resourceExpireTime;
                    }
                    else
                    {
                        this.resourceExpireTime.floatValue = resourceExpireTime;
                    }
                }

                int resourcePriority = EditorGUILayout.DelayedIntField("Resource Priority", this.resourcePriority.intValue);
                if (resourcePriority != this.resourcePriority.intValue)
                {
                    if (EditorApplication.isPlaying)
                    {
                        SpringContext.GetBean<IResourceManager>().ResourcePriority = resourcePriority;
                    }
                    else
                    {
                        this.resourcePriority.intValue = resourcePriority;
                    }
                }

                if (resourceModeIndex > 0)
                {
                    int generateReadWriteVersionListLength = EditorGUILayout.DelayedIntField("Generate Read Write Version List Length", this.generateReadWriteVersionListLength.intValue);
                    if (generateReadWriteVersionListLength != this.generateReadWriteVersionListLength.intValue)
                    {
                        if (EditorApplication.isPlaying)
                        {
                            SpringContext.GetBean<IResourceManager>().GenerateReadWriteVersionListLength = generateReadWriteVersionListLength;
                        }
                        else
                        {
                            this.generateReadWriteVersionListLength.intValue = generateReadWriteVersionListLength;
                        }
                    }

                    int updateRetryCount = EditorGUILayout.DelayedIntField("Update Retry Count", this.updateRetryCount.intValue);
                    if (updateRetryCount != this.updateRetryCount.intValue)
                    {
                        if (EditorApplication.isPlaying)
                        {
                            SpringContext.GetBean<IResourceManager>().UpdateRetryCount = updateRetryCount;
                        }
                        else
                        {
                            this.updateRetryCount.intValue = updateRetryCount;
                        }
                    }
                }
            }
            EditorGUI.EndDisabledGroup();

            EditorGUI.BeginDisabledGroup(EditorApplication.isPlayingOrWillChangePlaymode);
            {
                loadResourceAgentHelperCount.intValue = EditorGUILayout.IntSlider("Load Resource Agent Helper Count", loadResourceAgentHelperCount.intValue, 1, 128);
            }
            EditorGUI.EndDisabledGroup();

            if (EditorApplication.isPlaying && IsPrefabInHierarchy(t.gameObject))
            {
                var resourceManager = SpringContext.GetBean<IResourceManager>();
                EditorGUILayout.LabelField("Read Only Path", resourceManager.ReadOnlyPath);
                EditorGUILayout.LabelField("Read Write Path", resourceManager.ReadWritePath);
                EditorGUILayout.LabelField("Applicable Game Version", isEditorResourceMode ? "N/A" :resourceManager.ApplicableGameVersion ?? "<Unknwon>");
                EditorGUILayout.LabelField("Resource Version", isEditorResourceMode ? "N/A" : resourceManager.InternalResourceVersion.ToString());
                EditorGUILayout.LabelField("Asset Count", isEditorResourceMode ? "N/A" : resourceManager.AssetCount.ToString());
                EditorGUILayout.LabelField("Resource Count", isEditorResourceMode ? "N/A" : resourceManager.ResourceCount.ToString());
                EditorGUILayout.LabelField("Resource Group Count", isEditorResourceMode ? "N/A" : resourceManager.ResourceGroupCount.ToString());
                if (resourceModeIndex > 0)
                {
                    EditorGUILayout.LabelField("Applying Resource Pack Path", isEditorResourceMode ? "N/A" : resourceManager.ApplyingResourcePackPath ?? "<Unknwon>");
                    EditorGUILayout.LabelField("Apply Waiting Count", isEditorResourceMode ? "N/A" : resourceManager.ApplyWaitingCount.ToString());
                    EditorGUILayout.LabelField("Updating Resource Group", isEditorResourceMode ? "N/A" : resourceManager.UpdatingResourceGroup != null ? resourceManager.UpdatingResourceGroup.Name : "<Unknwon>");
                    EditorGUILayout.LabelField("Update Waiting Count", isEditorResourceMode ? "N/A" : resourceManager.UpdateWaitingCount.ToString());
                    EditorGUILayout.LabelField("Update Candidate Count", isEditorResourceMode ? "N/A" : resourceManager.UpdateCandidateCount.ToString());
                    EditorGUILayout.LabelField("Updating Count", isEditorResourceMode ? "N/A" : resourceManager.UpdatingCount.ToString());
                }
                EditorGUILayout.LabelField("Load Total Agent Count", isEditorResourceMode ? "N/A" : resourceManager.LoadTotalAgentCount.ToString());
                EditorGUILayout.LabelField("Load Free Agent Count", isEditorResourceMode ? "N/A" : resourceManager.LoadFreeAgentCount.ToString());
                EditorGUILayout.LabelField("Load Working Agent Count", isEditorResourceMode ? "N/A" : resourceManager.LoadWorkingAgentCount.ToString());
                EditorGUILayout.LabelField("Load Waiting Task Count", isEditorResourceMode ? "N/A" : resourceManager.LoadWaitingTaskCount.ToString());
                if (!isEditorResourceMode)
                {
                    EditorGUILayout.BeginVertical("box");
                    {
                        TaskInfo[] loadAssetInfos = SpringContext.GetBean<IResourceManager>().GetAllLoadAssetInfos();
                        if (loadAssetInfos.Length > 0)
                        {
                            foreach (TaskInfo loadAssetInfo in loadAssetInfos)
                            {
                                DrawLoadAssetInfo(loadAssetInfo);
                            }

                            if (GUILayout.Button("Export CSV Data"))
                            {
                                string exportFileName = EditorUtility.SaveFilePanel("Export CSV Data", string.Empty, "Load Asset Task Data.csv", string.Empty);
                                if (!string.IsNullOrEmpty(exportFileName))
                                {
                                    try
                                    {
                                        int index = 0;
                                        string[] data = new string[loadAssetInfos.Length + 1];
                                        data[index++] = "Load Asset Name,Serial Id,Priority,Status";
                                        foreach (TaskInfo loadAssetInfo in loadAssetInfos)
                                        {
                                            data[index++] = StringUtils.Format("{},{},{},{}", loadAssetInfo.Description, loadAssetInfo.SerialId.ToString(), loadAssetInfo.Priority.ToString(), loadAssetInfo.Status.ToString());
                                        }

                                        File.WriteAllLines(exportFileName, data, Encoding.UTF8);
                                        Debug.Log(StringUtils.Format("Export load asset task CSV data to '{}' success.", exportFileName));
                                    }
                                    catch (Exception exception)
                                    {
                                        Debug.LogError(StringUtils.Format("Export load asset task CSV data to '{}' failure, exception is '{}'.", exportFileName, exception.ToString()));
                                    }
                                }
                            }
                        }
                        else
                        {
                            GUILayout.Label("Load Asset Task is Empty ...");
                        }
                    }
                    EditorGUILayout.EndVertical();
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
            resourceMode = serializedObject.FindProperty("resourceMode");
            readWritePathType = serializedObject.FindProperty("readWritePathType");
            unloadUnusedAssetsInterval = serializedObject.FindProperty("unloadUnusedAssetsInterval");
            assetAutoReleaseInterval = serializedObject.FindProperty("assetAutoReleaseInterval");
            assetCapacity = serializedObject.FindProperty("assetCapacity");
            assetExpireTime = serializedObject.FindProperty("assetExpireTime");
            assetPriority = serializedObject.FindProperty("assetPriority");
            resourceAutoReleaseInterval = serializedObject.FindProperty("resourceAutoReleaseInterval");
            resourceCapacity = serializedObject.FindProperty("resourceCapacity");
            resourceExpireTime = serializedObject.FindProperty("resourceExpireTime");
            resourcePriority = serializedObject.FindProperty("resourcePriority");
            generateReadWriteVersionListLength = serializedObject.FindProperty("generateReadWriteVersionListLength");
            updateRetryCount = serializedObject.FindProperty("updateRetryCount");
            loadResourceAgentHelperCount = serializedObject.FindProperty("loadResourceAgentHelperCount");

            RefreshModes();
            RefreshTypeNames();
        }


        private void DrawLoadAssetInfo(TaskInfo loadAssetInfo)
        {
            EditorGUILayout.LabelField(loadAssetInfo.Description, StringUtils.Format("[SerialId]{} [Priority]{} [Status]{}", loadAssetInfo.SerialId.ToString(), loadAssetInfo.Priority.ToString(), loadAssetInfo.Status.ToString()));
        }

        private void RefreshModes()
        {
            resourceModeIndex = resourceMode.enumValueIndex > 0 ? resourceMode.enumValueIndex - 1 : 0;
        }

        private void RefreshTypeNames()
        {
            serializedObject.ApplyModifiedProperties();
        }
    }
}
