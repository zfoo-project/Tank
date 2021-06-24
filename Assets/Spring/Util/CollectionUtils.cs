using System.Collections.Generic;

namespace Spring.Util
{
    public abstract class CollectionUtils
    {
        public static readonly int[] EMPTY_INT_ARRAY = new int[] { };

        public static readonly byte[] EMPTY_BYTE_ARRAY = new byte[] { };
        
        public static List<T> EmptyList<T>()
        {
            return new List<T>(0);
        }
        
        public static HashSet<T> EmptyHashSet<T>()
        {
            return new HashSet<T>();
        }
        
        public static Dictionary<K, V> EmptyDictionary<K, V>()
        {
            return new Dictionary<K, V>(0);
        }
        
        public static bool IsEmpty(object[] array)
        {
            return array == null || array.Length == 0;
        }

        public static bool IsEmpty<T>(ICollection<T> collection)
        {
            return collection == null || collection.Count <= 0;
        }

        public static bool IsNotEmpty<T>(ICollection<T> collection)
        {
            return !IsEmpty(collection);
        }
    }
}