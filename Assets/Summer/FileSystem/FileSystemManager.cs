using System;
using System.Collections.Generic;
using System.IO;
using Summer.Base;
using Summer.Base.Model;
using Summer.FileSystem.Model;
using Spring.Core;
using Spring.Util;

namespace Summer.FileSystem
{
    /// <summary>
    /// 文件系统管理器。
    /// </summary>
    [Bean]
    public sealed class FileSystemManager : AbstractManager, IFileSystemManager
    {
        private readonly Dictionary<string, Model.FileSystem> m_FileSystems = new Dictionary<string, Model.FileSystem>(StringComparer.Ordinal);


        /// <summary>
        /// 获取文件系统数量。
        /// </summary>
        public int Count
        {
            get { return m_FileSystems.Count; }
        }

        /// <summary>
        /// 文件系统管理器轮询。
        /// </summary>
        /// <param name="elapseSeconds">逻辑流逝时间，以秒为单位。</param>
        /// <param name="realElapseSeconds">真实流逝时间，以秒为单位。</param>
        public override void Update(float elapseSeconds, float realElapseSeconds)
        {
        }

        /// <summary>
        /// 关闭并清理文件系统管理器。
        /// </summary>
        public override void Shutdown()
        {
            while (m_FileSystems.Count > 0)
            {
                foreach (KeyValuePair<string, Model.FileSystem> fileSystem in m_FileSystems)
                {
                    DestroyFileSystem(fileSystem.Value, false);
                    break;
                }
            }
        }

        /// <summary>
        /// 检查是否存在文件系统。
        /// </summary>
        /// <param name="fullPath">要检查的文件系统的完整路径。</param>
        /// <returns>是否存在文件系统。</returns>
        public bool HasFileSystem(string fullPath)
        {
            if (string.IsNullOrEmpty(fullPath))
            {
                throw new GameFrameworkException("Full path is invalid.");
            }

            return m_FileSystems.ContainsKey(PathUtils.GetRegularPath(fullPath));
        }

        /// <summary>
        /// 获取文件系统。
        /// </summary>
        /// <param name="fullPath">要获取的文件系统的完整路径。</param>
        /// <returns>获取的文件系统。</returns>
        public IFileSystem GetFileSystem(string fullPath)
        {
            if (string.IsNullOrEmpty(fullPath))
            {
                throw new GameFrameworkException("Full path is invalid.");
            }

            Model.FileSystem fileSystem = null;
            if (m_FileSystems.TryGetValue(PathUtils.GetRegularPath(fullPath), out fileSystem))
            {
                return fileSystem;
            }

            return null;
        }

        /// <summary>
        /// 创建文件系统。
        /// </summary>
        /// <param name="fullPath">要创建的文件系统的完整路径。</param>
        /// <param name="access">要创建的文件系统的访问方式。</param>
        /// <param name="maxFileCount">要创建的文件系统的最大文件数量。</param>
        /// <param name="maxBlockCount">要创建的文件系统的最大块数据数量。</param>
        /// <returns>创建的文件系统。</returns>
        public IFileSystem CreateFileSystem(string fullPath, FileSystemAccess access, int maxFileCount,
            int maxBlockCount)
        {
            if (string.IsNullOrEmpty(fullPath))
            {
                throw new GameFrameworkException("Full path is invalid.");
            }

            if (access == FileSystemAccess.Unspecified)
            {
                throw new GameFrameworkException("Access is invalid.");
            }

            if (access == FileSystemAccess.Read)
            {
                throw new GameFrameworkException("Access read is invalid.");
            }

            fullPath = PathUtils.GetRegularPath(fullPath);
            if (m_FileSystems.ContainsKey(fullPath))
            {
                throw new GameFrameworkException(StringUtils.Format("File system '{}' is already exist.", fullPath));
            }

            FileSystemStream fileSystemStream = CreateFileSystemStream(fullPath, access, true);
            if (fileSystemStream == null)
            {
                throw new GameFrameworkException(StringUtils.Format("Create file system stream for '{}' failure.",
                    fullPath));
            }

            Model.FileSystem fileSystem = Model.FileSystem.Create(fullPath, access, fileSystemStream, maxFileCount, maxBlockCount);
            if (fileSystem == null)
            {
                throw new GameFrameworkException(StringUtils.Format("Create file system '{}' failure.", fullPath));
            }

            m_FileSystems.Add(fullPath, fileSystem);
            return fileSystem;
        }

        /// <summary>
        /// 加载文件系统。
        /// </summary>
        /// <param name="fullPath">要加载的文件系统的完整路径。</param>
        /// <param name="access">要加载的文件系统的访问方式。</param>
        /// <returns>加载的文件系统。</returns>
        public IFileSystem LoadFileSystem(string fullPath, FileSystemAccess access)
        {
            if (string.IsNullOrEmpty(fullPath))
            {
                throw new GameFrameworkException("Full path is invalid.");
            }

            if (access == FileSystemAccess.Unspecified)
            {
                throw new GameFrameworkException("Access is invalid.");
            }

            fullPath = PathUtils.GetRegularPath(fullPath);
            if (m_FileSystems.ContainsKey(fullPath))
            {
                throw new GameFrameworkException(StringUtils.Format("File system '{}' is already exist.", fullPath));
            }

            FileSystemStream fileSystemStream = CreateFileSystemStream(fullPath, access, false);
            if (fileSystemStream == null)
            {
                throw new GameFrameworkException(StringUtils.Format("Create file system stream for '{}' failure.",
                    fullPath));
            }

            Model.FileSystem fileSystem = Model.FileSystem.Load(fullPath, access, fileSystemStream);
            if (fileSystem == null)
            {
                throw new GameFrameworkException(StringUtils.Format("Load file system '{}' failure.", fullPath));
            }

            m_FileSystems.Add(fullPath, fileSystem);
            return fileSystem;
        }

        /// <summary>
        /// 销毁文件系统。
        /// </summary>
        /// <param name="fileSystem">要销毁的文件系统。</param>
        /// <param name="deletePhysicalFile">是否删除文件系统对应的物理文件。</param>
        public void DestroyFileSystem(IFileSystem fileSystem, bool deletePhysicalFile)
        {
            if (fileSystem == null)
            {
                throw new GameFrameworkException("File system is invalid.");
            }

            string fullPath = fileSystem.FullPath;
            ((Model.FileSystem) fileSystem).Shutdown();
            m_FileSystems.Remove(fullPath);

            if (deletePhysicalFile && File.Exists(fullPath))
            {
                File.Delete(fullPath);
            }
        }

        /// <summary>
        /// 获取所有文件系统集合。
        /// </summary>
        /// <returns>获取的所有文件系统集合。</returns>
        public IFileSystem[] GetAllFileSystems()
        {
            int index = 0;
            IFileSystem[] results = new IFileSystem[m_FileSystems.Count];
            foreach (KeyValuePair<string, Model.FileSystem> fileSystem in m_FileSystems)
            {
                results[index++] = fileSystem.Value;
            }

            return results;
        }

        /// <summary>
        /// 获取所有文件系统集合。
        /// </summary>
        /// <param name="results">获取的所有文件系统集合。</param>
        public void GetAllFileSystems(List<IFileSystem> results)
        {
            if (results == null)
            {
                throw new GameFrameworkException("Results is invalid.");
            }

            results.Clear();
            foreach (KeyValuePair<string, Model.FileSystem> fileSystem in m_FileSystems)
            {
                results.Add(fileSystem.Value);
            }
        }
        
        private const string AndroidFileSystemPrefixString = "jar:";

        /// <summary>
        /// 创建文件系统流。
        /// </summary>
        /// <param name="fullPath">要加载的文件系统的完整路径。</param>
        /// <param name="access">要加载的文件系统的访问方式。</param>
        /// <param name="createNew">是否创建新的文件系统流。</param>
        /// <returns>创建的文件系统流。</returns>
        public FileSystemStream CreateFileSystemStream(string fullPath, FileSystemAccess access, bool createNew)
        {
            if (fullPath.StartsWith(AndroidFileSystemPrefixString, StringComparison.Ordinal))
            {
                return new AndroidFileSystemStream(fullPath, access, createNew);
            }
            else
            {
                return new CommonFileSystemStream(fullPath, access, createNew);
            }
        }
    }
}