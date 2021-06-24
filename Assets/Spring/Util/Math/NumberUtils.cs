using System.Text;

namespace Spring.Util.Math
{
    public abstract class NumberUtils
    {
        public static string ToHex(int n)
        {
            var builder = new StringBuilder();
            return builder.AppendFormat("{0:x8}", n).ToString();
        }


        /**
         * 如果long超过最大值，则返回int的最大值
         */
        public static int LongToInt(long n)
        {
            if (n >= int.MaxValue)
            {
                return int.MaxValue;
            }

            if (n <= int.MinValue)
            {
                return int.MinValue;
            }

            return (int) n;
        }

        /**
         * 保留固定位数小数<br>
         * 采用四舍五入策略<br>
         * 例如保留2位小数：123.456789 =》 123.46
         *
         * @param number 数字值
         * @param scale  保留小数位数
         * @return 新值
         */
        public static decimal round(float number, int scale)
        {
            return decimal.Round(new decimal(number), scale);
        }

        public static decimal round(double number, int scale)
        {
            return decimal.Round(new decimal(number), scale);
        }

        public static decimal round(string numberStr, int scale)
        {
            return decimal.Round(decimal.Parse(numberStr), scale);
        }
    }
}