using System.IO;
using Spring.Util;
using Summer.Editor.Misc;
using Summer.Editor.ResourceBuilder;
using Summer.Editor.ResourceBuilder.Model;
using Summer.Editor.ResourceCollection;
using Summer.Editor.ResourceEditor;
using Summer.Editor.ResourceEditor.Model;
using UnityEngine;

namespace MiniKing.Script.Editor
{
    public class MiniKingBuilder
    {
        [BuildSettingsConfigPath]
        public static string BuildSettingsConfig = PathUtils.GetRegularPath(Path.Combine(Application.dataPath, "MiniKing/Config/Builder/BuildSettings.xml"));

        [ResourceCollectionConfigPath]
        public static string ResourceCollectionConfig = PathUtils.GetRegularPath(Path.Combine(Application.dataPath, "MiniKing/Config/Builder/ResourceCollection.xml"));

        [ResourceEditorConfigPath]
        public static string ResourceEditorConfig = PathUtils.GetRegularPath(Path.Combine(Application.dataPath, "MiniKing/Config/Builder/ResourceEditor.xml"));

        [ResourceBuilderConfigPath]
        public static string ResourceBuilderConfig = PathUtils.GetRegularPath(Path.Combine(Application.dataPath, "MiniKing/Config/Builder/ResourceBuilder.xml"));
    }
}