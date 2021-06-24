using System;
using System.Linq;

namespace Spring.Util
{
    public abstract class TimeUtils
    {
        // 一秒钟对应的纳秒数
        public static readonly long NANO_PER_SECOND = 1_000_000_000;

        // 一秒钟对应的毫秒数
        public static readonly long MILLIS_PER_SECOND = 1 * 1000;

        // 一分钟对应的毫秒数
        public static readonly long MILLIS_PER_MINUTE = 1 * 60 * MILLIS_PER_SECOND;

        // 一个小时对应的毫秒数
        public static readonly long MILLIS_PER_HOUR = 1 * 60 * MILLIS_PER_MINUTE;

        // 一天对应的毫秒数
        public static readonly long MILLIS_PER_DAY = 1 * 24 * MILLIS_PER_HOUR;

        /// <summary>
        /// DateTimeHelper
        /// </summary>
        /// <summary>
        /// Unix时间起始时间
        /// </summary>
        public static readonly DateTime START_TIME_LOCAL = new DateTime(1970, 1, 1, 0, 0, 0);

        public static readonly DateTime START_TIME_UTC = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        /// <summary>
        /// 常用日期格式
        /// </summary>
        public static readonly string COMMON_DATE_FORMAT = "yyyy-MM-dd HH:mm:ss.fff";

        /// <summary>
        /// 周未定义
        /// </summary>
        public static readonly DayOfWeek[] WEEKEND = {DayOfWeek.Saturday, DayOfWeek.Sunday};

        private static long timestamp = CurrentTimeMillis();

        /**
         * 使用服务器的时间戳，会有一些误差
         * 
         * 获取最多只有一秒延迟的粗略时间戳，适用于对时间精度要求不高的场景，最多只有1-2秒误差
         * <p>
         * 比CurrentTimeMillis()的性能高10倍
         */
        public static long Now()
        {
            return timestamp;
        }

        public static void SetNow(long time)
        {
            timestamp = time;
        }

        /**
         * 获取精确的时间戳
         */
        public static long CurrentTimeMillis()
        {
            return (DateTime.UtcNow - START_TIME_LOCAL).Ticks / 1_0000;
        }

        public static DateTime TimestampToDateTime(long time)
        {
            var startTime = TimeZoneInfo.ConvertTime(START_TIME_UTC, TimeZoneInfo.Local);
            return startTime.AddSeconds(time / (double) MILLIS_PER_SECOND);
        }


        /// <summary>
        /// 明天
        /// </summary>
        /// <param name="date"></param>
        /// <returns></returns>
        public static DateTime Tomorrow(DateTime date)
        {
            return date.AddDays(1);
        }

        /// <summary>
        /// 昨天
        /// </summary>
        /// <param name="date"></param>
        /// <returns></returns>
        public static DateTime Yesterday(DateTime date)
        {
            return date.AddDays(-1);
        }

        /// <summary>
        /// 常用日期格式化字符串
        /// </summary>
        /// <param name="date"></param>
        /// <returns></returns>
        public static string ToCommonFormat(DateTime date)
        {
            return date.ToString(COMMON_DATE_FORMAT);
        }

        /// <summary>
        /// 是否是周未
        /// </summary>
        /// <param name="date"></param>
        /// <returns></returns>
        public static bool IsWeekend(DateTime date)
        {
            return WEEKEND.Any(p => p == date.DayOfWeek);
        }

        /// <summary>
        /// 是否是工作日
        /// </summary>
        /// <param name="date"></param>
        /// <returns></returns>
        public static bool IsWeekDay(DateTime date)
        {
            return !IsWeekend(date);
        }

        /// <summary>
        /// 给定月份的第1天
        /// </summary>
        /// <param name="date"></param>
        /// <returns></returns>
        public static DateTime GetFirstDayOfMonth(DateTime date)
        {
            return new DateTime(date.Year, date.Month, 1);
        }

        /// <summary>
        /// 给定月份的最后1天
        /// </summary>
        /// <param name="date"></param>
        /// <returns></returns>
        public static DateTime GetLastDayOfMonth(DateTime date)
        {
            return GetFirstDayOfMonth(date).AddMonths(1).AddDays(-1);
        }

        /// <summary>
        /// 给定日期所在月份第1个星期几所对应的日期
        /// </summary>
        /// <param name="date">给定日期</param>
        /// <param name="dayOfWeek">星期几</param>
        /// <returns>所对应的日期</returns>
        public static DateTime GetFirstWeekDayOfMonth(DateTime date, DayOfWeek dayOfWeek)
        {
            var dt = GetFirstDayOfMonth(date);
            while (dt.DayOfWeek != dayOfWeek)
                dt = dt.AddDays(1);

            return dt;
        }

        /// <summary>
        /// 给定日期所在月份最后1个星期几所对应的日期
        /// </summary>
        /// <param name="date">给定日期</param>
        /// <param name="dayOfWeek">星期几</param>
        /// <returns>所对应的日期</returns>
        public static DateTime GetLastWeekDayOfMonth(DateTime date, DayOfWeek dayOfWeek)
        {
            var dt = GetLastDayOfMonth(date);
            while (dt.DayOfWeek != dayOfWeek)
                dt = dt.AddDays(-1);

            return dt;
        }

        /// <summary>
        /// 早于给定日期
        /// </summary>
        /// <param name="date"></param>
        /// <param name="other"></param>
        /// <returns></returns>
        public static bool IsBefore(DateTime date, DateTime other)
        {
            return date.CompareTo(other) < 0;
        }

        /// <summary>
        /// 晚于给定日期
        /// </summary>
        /// <param name="date"></param>
        /// <param name="other"></param>
        /// <returns></returns>
        public static bool IsAfter(DateTime date, DateTime other)
        {
            return date.CompareTo(other) > 0;
        }

        /// <summary>
        /// 给定日期最后一刻,精确到23:59:59.999
        /// </summary>
        /// <param name="date"></param>
        /// <returns></returns>
        public static DateTime EndTimeOfDay(DateTime date)
        {
            return new DateTime(date.Year, date.Month, date.Day, 23, 59, 59, 999);
        }

        /// <summary>
        ///  给定日期开始一刻,精确到0:0:0.0
        /// </summary>
        /// <param name="date"></param>
        /// <returns></returns>
        public static DateTime StartTimeOfDay(DateTime date)
        {
            return date.Date;
        }

        /// <summary>
        ///  给定日期的中午,精确到12:0:0.0
        /// </summary>
        /// <param name="date"></param>
        /// <returns></returns>
        public static DateTime NoonOfDay(DateTime date)
        {
            return new DateTime(date.Year, date.Month, date.Day, 12, 0, 0);
        }

        /// <summary>
        /// 当前日期与给定日期是否是同一天
        /// </summary>
        /// <param name="date">当前日期</param>
        /// <param name="dateToCompare">给定日期</param>
        /// <returns></returns>
        public static bool IsDateEqual(DateTime date, DateTime dateToCompare)
        {
            return (date.Date == dateToCompare.Date);
        }

        /// <summary>
        /// 判断是否为今天
        /// </summary>
        /// <param name="date"></param>
        /// <returns></returns>
        public static bool IsToday(DateTime date)
        {
            return (date.Date == DateTime.UtcNow.Date);
        }

        /// <summary>
        /// 给定日期所在月份共有多少天
        /// </summary>
        /// <param name="date"></param>
        /// <returns></returns>
        public static int GetCountDaysOfMonth(DateTime date)
        {
            return GetLastDayOfMonth(date).Day;
        }


        /// <summary>  
        /// 得到本周第一天(以星期一为第一天)  
        /// </summary>  
        /// <param name="datetime"></param>  
        /// <returns></returns>  
        public static DateTime GetWeekFirstDayMon(DateTime datetime)
        {
            //星期一为第一天  
            int weeknow = Convert.ToInt32(datetime.DayOfWeek);

            //因为是以星期一为第一天，所以要判断weeknow等于0时，要向前推6天。  
            weeknow = (weeknow == 0 ? (7 - 1) : (weeknow - 1));
            int daydiff = (-1) * weeknow;

            //本周第一天  
            string FirstDay = datetime.AddDays(daydiff).ToString("yyyy-MM-dd");
            return Convert.ToDateTime(FirstDay);
        }

        /// <summary>  
        /// 得到本周最后一天(以星期天为最后一天)  
        /// </summary>  
        /// <param name="datetime"></param>  
        /// <returns></returns>  
        public static DateTime GetWeekLastDaySun(DateTime datetime)
        {
            //星期天为最后一天  
            int weeknow = Convert.ToInt32(datetime.DayOfWeek);
            weeknow = (weeknow == 0 ? 7 : weeknow);
            int daydiff = (7 - weeknow);

            //本周最后一天  
            string LastDay = datetime.AddDays(daydiff).ToString("yyyy-MM-dd");
            return Convert.ToDateTime(LastDay);
        }
    }
}