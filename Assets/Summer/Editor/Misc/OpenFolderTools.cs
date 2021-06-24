using System.Diagnostics;
using Summer.Base.Model;
using Spring.Util;
using UnityEditor;
using UnityEngine;

namespace Summer.Editor.Misc
{
    /// <summary>
    /// 打开文件夹相关的实用函数。
    /// </summary>
    public class OpenFolderTools
    {
        /// <summary>
        /// 打开 Data Path 文件夹。
        /// </summary>
        [MenuItem("Summer/Open Folder/Data Path", false, 10)]
        public static void OpenFolderDataPath()
        {
            Execute(Application.dataPath);
        }

        /// <summary>
        /// 打开 Persistent Data Path 文件夹。
        /// </summary>
        [MenuItem("Summer/Open Folder/Persistent Data Path", false, 11)]
        public static void OpenFolderPersistentDataPath()
        {
            Execute(Application.persistentDataPath);
        }

        /// <summary>
        /// 打开 Streaming Assets Path 文件夹。
        /// </summary>
        [MenuItem("Summer/Open Folder/Streaming Assets Path", false, 12)]
        public static void OpenFolderStreamingAssetsPath()
        {
            Execute(Application.streamingAssetsPath);
        }

        /// <summary>
        /// 打开 Temporary Cache Path 文件夹。
        /// </summary>
        [MenuItem("Summer/Open Folder/Temporary Cache Path", false, 13)]
        public static void OpenFolderTemporaryCachePath()
        {
            Execute(Application.temporaryCachePath);
        }
        
        
        /// <summary>
        /// 打开 Console Log Path 文件夹。
        /// </summary>
        [MenuItem("Summer/Open Folder/Console Log Path", false, 14)]
        public static void OpenFolderConsoleLogPath()
        {
            Execute(System.IO.Path.GetDirectoryName(Application.consoleLogPath));
        }

        /// <summary>
        /// 打开指定路径的文件夹。
        /// </summary>
        /// <param name="folder">要打开的文件夹的路径。</param>
        public static void Execute(string folder)
        {
            folder = StringUtils.Format("\"{}\"", folder);
            switch (Application.platform)
            {
                case RuntimePlatform.WindowsEditor:
                    Process.Start("Explorer.exe", folder.Replace('/', '\\'));
                    break;

                case RuntimePlatform.OSXEditor:
                    Process.Start("open", folder);
                    break;

                default:
                    throw new GameFrameworkException(StringUtils.Format("Not support open folder on '{}' platform.", Application.platform.ToString()));
            }
        }
    }
}
