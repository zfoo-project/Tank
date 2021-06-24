using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Spring.Util
{
    public abstract class FileUtils
    {
        public static readonly string LS = System.Environment.NewLine;
        public static readonly string UNIX_LS = "\\n";
        public static readonly string WINDOWS_LS = "\\r\\n";
        
        public static List<FileInfo> GetAllReadableFiles()
        {
            var result = new List<FileInfo>();
            result.AddRange(GetAllReadableFilesByPath(Application.dataPath));
            result.AddRange(GetAllReadableFilesByPath(Application.temporaryCachePath));
            result.AddRange(GetAllReadableFilesByPath(Application.persistentDataPath));
            result.AddRange(GetAllReadableFilesByPath(Application.streamingAssetsPath));
            return result;
        }

        public static List<FileInfo> GetAllReadableFilesByPath(string path)
        {
            var result = new List<FileInfo>();

            if (Directory.Exists(path))
            {
                var direction = new DirectoryInfo(path);
                var files = direction.GetFiles("*", SearchOption.AllDirectories);
                foreach (var fileInfo in files)
                {
                    if (fileInfo.Name.EndsWith(".meta"))
                    {
                        continue;
                    }

                    result.Add(fileInfo);

                    // Log.Info("Name : " + fileInfo.Name);
                    // Log.Info("FullName : " + fileInfo.FullName);
                    // Log.Info("DirectoryName : " + fileInfo.DirectoryName);
                }
            }

            return result;
        }


        /// <summary>
        /// Reads a file with the provided path using async or sync IO depending on the platform.
        /// </summary>
        public static byte[] ReadFile(string filePath)
        {
            return File.ReadAllBytes(filePath);
        }

        /// <summary>
        /// Writes a file's data to the provided path using async or sync IO depending on the platform.
        /// </summary>
        public static void WriteFile(string filePath, byte[] fileData)
        {
            File.WriteAllBytes(filePath, fileData);
        }

        /// <summary>
        /// Reads a text file with the provided path using async or sync IO depending on the platform.
        /// </summary>
        public static string ReadTextFile(string filePath)
        {
            return File.ReadAllText(filePath);
        }

        /// <summary>
        /// Writes a text file's data to the provided path using async or sync IO depending on the platform.
        /// </summary>
        public static void WriteTextFile(string filePath, string fileText)
        {
            File.WriteAllText(filePath, fileText);
        }

        /// <summary>
        /// Deletes file at the provided path. Will insure for correct IO on specific platforms.
        /// </summary>
        public static void DeleteFile(string filePath)
        {
            File.Delete(filePath);
        }

        /// <summary>
        /// Moves a file from <paramref name="sourceFilePath"/> to <paramref name="destFilePath"/>.
        /// Will overwrite the <paramref name="destFilePath"/> in case it exists.
        /// </summary>
        public static void MoveFile(string sourceFilePath, string destFilePath)
        {
            File.Delete(destFilePath);
            File.Move(sourceFilePath, destFilePath);
        }

        /// <summary>
        /// Creates a new directory at the provided path. Will insure for correct IO on specific platforms.
        /// </summary>
        public static void CreateDirectory(string path)
        {
            Directory.CreateDirectory(path);
        }

        /// <summary>
        /// Deletes directory at the provided path. Will insure for correct IO on specific platforms.
        /// </summary>
        public static void DeleteFile(string path, bool recursive)
        {
            if (Directory.Exists(path))
            {
                Directory.Delete(path, recursive);
            }

            if (File.Exists(path))
            {
                File.Delete(path);
            }
        }
    }
}