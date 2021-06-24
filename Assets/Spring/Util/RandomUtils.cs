using System;
using System.Collections.Generic;
using NPOI.SS.Formula.Functions;

namespace Spring.Util
{
    /// <summary>
    /// 随机相关的实用函数。
    /// </summary>
    public abstract class RandomUtils
    {
        [ThreadStatic]
        private static Random random;


        public static Random GetRandom()
        {
            if (random == null)
            {
                random = new Random((int) DateTime.UtcNow.Ticks);
            }

            return random;
        }


        /// <summary>
        /// 返回非负随机数。
        /// </summary>
        /// <returns>大于等于零且小于 System.Int32.MaxValue 的 32 位带符号整数。</returns>
        public static int RandomInt()
        {
            return GetRandom().Next();
        }


        /// <summary>
        /// 获得指定范围内的随机数 [0,limit)
        /// </summary>
        /// <param name="limit">要生成的随机数的上界（随机数不能取该上界值）。maxValue 必须大于等于零。</param>
        /// <returns>大于等于零且小于 maxValue 的 32 位带符号整数，即：返回值的范围通常包括零但不包括 maxValue。不过，如果 maxValue 等于零，则返回 maxValue。</returns>
        public static int RandomInt(int limit)
        {
            return GetRandom().Next(limit);
        }

        /// <summary>
        /// 返回一个指定范围内的随机数。
        /// </summary>
        /// <param name="minValue">返回的随机数的下界（随机数可取该下界值）。</param>
        /// <param name="maxValue">返回的随机数的上界（随机数不能取该上界值）。maxValue 必须大于等于 minValue。</param>
        /// <returns>一个大于等于 minValue 且小于 maxValue 的 32 位带符号整数，即：返回的值范围包括 minValue 但不包括 maxValue。如果 minValue 等于 maxValue，则返回 minValue。</returns>
        public static int RandomInt(int minValue, int maxValue)
        {
            return GetRandom().Next(minValue, maxValue);
        }

        /// <summary>
        /// 返回一个介于 0.0 和 1.0 之间的随机数。
        /// </summary>
        /// <returns>大于等于 0.0 并且小于 1.0 的双精度浮点数。</returns>
        public static double GetRandomDouble()
        {
            return GetRandom().NextDouble();
        }

        /// <summary>
        /// 用随机数填充指定字节数组的元素。
        /// </summary>
        /// <param name="buffer">包含随机数的字节数组。</param>
        public static void GetRandomBytes(byte[] buffer)
        {
            GetRandom().NextBytes(buffer);
        }


        /**
         * 随机获得列表中的元素
         *
         * @param <T>  元素类型
         * @param list 列表
         * @return 随机元素
         */
        public static T RandomEle<T>(List<T> list)
        {
            return RandomEle(list, list.Count);
        }
        public static T RandomEle<T>(List<T> list, int limit)
        {
            return list[RandomInt(limit)];
        }

        /**
         * 随机获得数组中的元素
         *
         * @param <T>   元素类型
         * @param array 列表
         * @return 随机元素
         */
        public static T RandomEle(T[] array)
        {
            return RandomEle(array, array.Length);
        }

        /**
         * 随机获得数组中的元素
         *
         * @param <T>   元素类型
         * @param array 列表
         * @param limit 限制列表的前N项
         * @return 随机元素
         */
        public static T RandomEle(T[] array, int limit)
        {
            return array[RandomInt(limit)];
        }

        /**
         * 随机获得列表中的一定量元素
         *
         * @param <T>   元素类型
         * @param list  列表
         * @param count 随机取出的个数
         * @return 随机元素
         */
        public static List<T> RandomEles<T>(List<T> list, int count)
        {
            var result = new List<T>(count);
            var limit = list.Count;
            while (result.Count < count)
            {
                result.Add(RandomEle(list, limit));
            }

            return result;
        }

        /**
         * 随机获得列表中的一定量的不重复元素，返回Set
         *
         * @param <T>        元素类型
         * @param collection 列表
         * @param count      随机取出的个数
         * @return 随机元素
         * @throws IllegalArgumentException 需要的长度大于给定集合非重复总数
         */
        public static HashSet<T> RandomEleSet<T>(ICollection<T> collection, int count)
        {
            var source = new List<T>(new HashSet<T>(collection));
            if (count > source.Count)
            {
                throw new Exception("Count is larger than collection distinct size !");
            }

            var result = new HashSet<T>();
            int limit = collection.Count;
            while (result.Count < count)
            {
                result.Add(RandomEle(source, limit));
            }

            return result;
        }
    }
}