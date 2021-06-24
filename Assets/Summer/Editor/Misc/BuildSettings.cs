using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using Spring.Util;
using UnityEditor;
using UnityEngine;

namespace Summer.Editor.Misc
{
    /// <summary>
    /// 构建配置相关的实用函数。
    /// </summary>
    static class BuildSettings
    {
        private static readonly string configurationPath = null;
        private static readonly List<string> defaultSceneNames = new List<string>();
        private static readonly List<string> searchScenePaths = new List<string>();

        static BuildSettings()
        {
            configurationPath = (string) AssemblyUtils.GetAllFieldsByAttribute<BuildSettingsConfigPathAttribute>().First().GetValue(null);
            defaultSceneNames.Clear();
            searchScenePaths.Clear();

            if (!File.Exists(configurationPath))
            {
                return;
            }

            try
            {
                XmlDocument xmlDocument = new XmlDocument();
                xmlDocument.Load(configurationPath);
                XmlNode xmlRoot = xmlDocument.SelectSingleNode("Summer");
                XmlNode xmlBuildSettings = xmlRoot.SelectSingleNode("BuildSettings");
                XmlNode xmlDefaultScenes = xmlBuildSettings.SelectSingleNode("DefaultScenes");
                XmlNode xmlSearchScenePaths = xmlBuildSettings.SelectSingleNode("SearchScenePaths");

                XmlNodeList xmlNodeList = null;
                XmlNode xmlNode = null;

                xmlNodeList = xmlDefaultScenes.ChildNodes;
                for (int i = 0; i < xmlNodeList.Count; i++)
                {
                    xmlNode = xmlNodeList.Item(i);
                    if (xmlNode.Name != "DefaultScene")
                    {
                        continue;
                    }

                    string defaultSceneName = xmlNode.Attributes.GetNamedItem("Name").Value;
                    defaultSceneNames.Add(defaultSceneName);
                }

                xmlNodeList = xmlSearchScenePaths.ChildNodes;
                for (int i = 0; i < xmlNodeList.Count; i++)
                {
                    xmlNode = xmlNodeList.Item(i);
                    if (xmlNode.Name != "SearchScenePath")
                    {
                        continue;
                    }

                    string searchScenePath = xmlNode.Attributes.GetNamedItem("Path").Value;
                    searchScenePaths.Add(searchScenePath);
                }
            }
            catch
            {
            }
        }

        /// <summary>
        /// 将构建场景设置为默认。
        /// </summary>
        [MenuItem("Summer/Scenes in Build Settings/Default Scenes", false, 20)]
        public static void DefaultScenes()
        {
            HashSet<string> sceneNames = new HashSet<string>();
            foreach (string sceneName in defaultSceneNames)
            {
                sceneNames.Add(sceneName);
            }

            List<EditorBuildSettingsScene> scenes = new List<EditorBuildSettingsScene>();
            foreach (string sceneName in sceneNames)
            {
                scenes.Add(new EditorBuildSettingsScene(sceneName, true));
            }

            EditorBuildSettings.scenes = scenes.ToArray();

            Debug.Log("Set scenes of build settings to default scenes.");
        }

        /// <summary>
        /// 将构建场景设置为所有。
        /// </summary>
        [MenuItem("Summer/Scenes in Build Settings/All Scenes", false, 21)]
        public static void AllScenes()
        {
            HashSet<string> sceneNames = new HashSet<string>();
            foreach (string sceneName in defaultSceneNames)
            {
                sceneNames.Add(sceneName);
            }

            string[] sceneGuids = AssetDatabase.FindAssets("t:Scene", searchScenePaths.ToArray());
            foreach (string sceneGuid in sceneGuids)
            {
                string sceneName = AssetDatabase.GUIDToAssetPath(sceneGuid);
                sceneNames.Add(sceneName);
            }

            List<EditorBuildSettingsScene> scenes = new List<EditorBuildSettingsScene>();
            foreach (string sceneName in sceneNames)
            {
                scenes.Add(new EditorBuildSettingsScene(sceneName, true));
            }

            EditorBuildSettings.scenes = scenes.ToArray();

            Debug.Log("Set scenes of build settings to all scenes.");
        }
    }
}
