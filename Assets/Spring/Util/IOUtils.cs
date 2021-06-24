using System;
using System.IO;
using Spring.Logger;

namespace Spring.Util
{
    public abstract class IOUtils
    {
        // The number of bytes in a kilobyte
        public static readonly int ONE_BYTE = 1;

        // The number of bytes in a kilobyte
        public static readonly int BYTES_PER_KB = ONE_BYTE * 1024;

        // The number of bytes in a megabyte
        public static readonly int BYTES_PER_MB = BYTES_PER_KB * 1024;

        // The number of bytes in a gigabyte
        public static readonly long BYTES_PER_GB = BYTES_PER_MB * 1024;
        
        public static readonly long BYTES_PER_TB = BYTES_PER_GB * 1024;

        public static readonly int BITS_PER_BYTE = ONE_BYTE * 8;
        public static readonly int BITS_PER_KB = BYTES_PER_KB * 8;
        public static readonly int BITS_PER_MB = BYTES_PER_MB * 8;
        public static readonly long BITS_PER_GB = BYTES_PER_GB * 8L;


        public static void CloseIO(params IDisposable[] closeables)
        {
            if (closeables == null)
            {
                return;
            }

            foreach (var obj in closeables)
            {
                if (obj == null)
                {
                    continue;
                }

                try
                {
                    obj.Dispose();
                }
                catch (IOException e)
                {
                    Log.Error(e);
                }
            }
        }
    }
}