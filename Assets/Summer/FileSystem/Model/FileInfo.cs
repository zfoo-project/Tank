using System.Runtime.InteropServices;
using Summer.Base.Model;

namespace Summer.FileSystem.Model
{
    /// <summary>
    /// 文件信息。
    /// </summary>
    [StructLayout(LayoutKind.Auto)]
    public struct FileInfo
    {
        private readonly string name;
        private readonly long offset;
        private readonly int length;

        /// <summary>
        /// 初始化文件信息的新实例。
        /// </summary>
        /// <param name="name">文件名称。</param>
        /// <param name="offset">文件偏移。</param>
        /// <param name="length">文件长度。</param>
        public FileInfo(string name, long offset, int length)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new GameFrameworkException("Name is invalid.");
            }

            if (offset < 0L)
            {
                throw new GameFrameworkException("Offset is invalid.");
            }

            if (length < 0)
            {
                throw new GameFrameworkException("Length is invalid.");
            }

            this.name = name;
            this.offset = offset;
            this.length = length;
        }

        /// <summary>
        /// 获取文件信息是否有效。
        /// </summary>
        public bool IsValid
        {
            get
            {
                return !string.IsNullOrEmpty(name) && offset >= 0L && length >= 0;
            }
        }

        /// <summary>
        /// 获取文件名称。
        /// </summary>
        public string Name
        {
            get
            {
                return name;
            }
        }

        /// <summary>
        /// 获取文件偏移。
        /// </summary>
        public long Offset
        {
            get
            {
                return offset;
            }
        }

        /// <summary>
        /// 获取文件长度。
        /// </summary>
        public int Length
        {
            get
            {
                return length;
            }
        }
    }
}
