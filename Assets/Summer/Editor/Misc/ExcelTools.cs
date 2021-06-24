using Spring.Storage.Helper;
using UnityEditor;
using UnityEngine;

namespace Summer.Editor.Misc
{
    /// <summary>
    /// 帮助相关的实用函数。
    /// </summary>
    public class ExcelTools
    {
        [MenuItem("Summer/Github Source Code", false, 90)]
        public static void ShowGithub()
        {
            Application.OpenURL("https://github.com/EllanJiang");
        }

        
        [MenuItem("Summer/Excel To Json Files", false, 92)]
        public static void ExcelToJsonFiles()
        {
            ExcelStorageHelper.ExcelToJsonFiles();
        }
        
        [MenuItem("Summer/Clear All Json Files", false, 92)]
        public static void ClearAllJsonFiles()
        {
            ExcelStorageHelper.ClearAllJsonFiles();
        }
    }
}
