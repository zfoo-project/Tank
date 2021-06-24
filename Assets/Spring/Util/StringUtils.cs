using System;
using System.Collections.Generic;
using System.Text;

namespace Spring.Util
{
    /// <summary>
    /// 字符相关的实用函数。
    /// </summary>
    public abstract class StringUtils
    {
        [ThreadStatic]
        private static StringBuilder cachedStringBuilder;

        [ThreadStatic]
        private static object[] oneArgs;

        [ThreadStatic]
        private static object[] twoArgs;

        [ThreadStatic]
        private static object[] threeArgs;

        public static readonly string EMPTY = "";

        public static readonly string NULL_OBJECT_string = "null";

        public static readonly string[] EMPTY_STRING_ARRAY = new string[] { };

        public static readonly object[] ONE_OBJECT_ARRAY = {NULL_OBJECT_string};

        public static readonly string COMMA = ","; // [com·ma || 'kɒmə] n.  逗点; 逗号
        public static readonly string COMMA_REGEX = ",|，";

        public static readonly string PERIOD = "."; // 句号

        public static readonly string LEFT_SQUARE_BRACKET = "["; //左方括号

        public static readonly string RIGHT_SQUARE_BRACKET = "]"; //右方括号

        public static readonly string COLON = ":"; //冒号[co·lon || 'kəʊlən]
        public static readonly string COLON_REGEX = ":|：";

        public static readonly string SEMICOLON = ";"; //分号['semi'kәulәn]
        public static readonly string SEMICOLON_REGEX = ";|；";

        public static readonly string QUOTATION_MARK = "\""; //引号[quo·ta·tion || kwəʊ'teɪʃn]

        public static readonly string ELLIPSIS = "..."; //省略号

        public static readonly string EXCLAMATION_POINT = "!"; //感叹号

        public static readonly string DASH = "-"; //破折号

        public static readonly string QUESTION_MARK = "?"; //问好

        public static readonly string HYPHEN = "-"; //连接号，连接号与破折号的区别是，连接号的两头不用空格

        public static readonly string SLASH = "/"; //斜线号

        public static readonly string BACK_SLASH = "\\"; //反斜线号

        public static readonly string VERTICAL_BAR = "|"; // 竖线
        public static readonly string VERTICAL_BAR_REGEX = "\\|";

        public static readonly string SHARP = "#";
        public static readonly string SHARP_REGEX = "\\#";

        public static readonly string DOLLAR = "$"; // 美元符号

        public static readonly string EMPTY_JSON = "{}";

        public static readonly string MULTIPLE_HYPHENS = "-----------------------------------------------------------------------";


        public static readonly int INDEX_NOT_FOUND = -1; //Represents a failed index search.

        public static readonly string DEFAULT_CHARSET = "UTF-8";

        /**
         * 用于随机选的数字
         */
        public static readonly string ARAB_NUMBER = "0123456789";

        /**
         * 用于随机选的字符
         */
        public static readonly string ENGLISH_CHAR = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ";

        public static readonly HashSet<char> ENGLISH_SET = new HashSet<char>()
        {
            'a', 'b', 'c', 'd', 'e', 'f', 'g', 'h', 'i', 'j', 'k', 'l', 'm', 'n', 'o', 'p', 'q', 'r', 's', 't', 'u', 'v', 'w', 'x', 'y', 'z',
            'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H', 'I', 'J', 'K', 'L', 'M', 'N', 'O', 'P', 'Q', 'R', 'S', 'T', 'U', 'V', 'W', 'X', 'Y', 'Z'
        };

        /**
     * Checks if a CharSequence is empty ("") or null.
     * <pre>
     * StringUtils.isEmpty(null)      = true
     * StringUtils.isEmpty("")        = true
     * StringUtils.isEmpty(" ")       = false
     * StringUtils.isEmpty("bob")     = false
     * </pre>
     * It no longer trims the CharSequence.
     * That functionality is available in isBlank().
     *
     * @param cs the CharSequence to check, may be null
     * @return {@code true} if the CharSequence is empty or null
     */
        public static bool IsEmpty(string cs)
        {
            return cs == null || cs.Length == 0;
        }


        /**
     * StringUtils.isBlank(null)=true
     * StringUtils.isBlank("")=true
     * StringUtils.isBlank("    ")=true
     * StringUtils.isBlank(" b ")=false
     *
     * @param cs 要检查的字符串
     * @return 是否为空的字符串
     */
        public static bool IsBlank(string cs)
        {
            if (IsEmpty(cs))
            {
                return true;
            }

            var length = cs.Length;
            var charArray = cs.ToCharArray();
            for (var i = 0; i < length; i++)
            {
                if (char.IsWhiteSpace(charArray[i]) == false)
                {
                    return false;
                }
            }

            return true;
        }

        public static string Format(string format, object arg0)
        {
            if (format == null)
            {
                throw new Exception("Format is invalid.");
            }

            if (oneArgs == null)
            {
                oneArgs = new object[1];
            }

            oneArgs[0] = arg0;
            return Format(format, oneArgs);
        }

        public static string Format(string format, object arg0, object arg1)
        {
            if (format == null)
            {
                throw new Exception("Format is invalid.");
            }

            if (twoArgs == null)
            {
                twoArgs = new object[2];
            }

            twoArgs[0] = arg0;
            twoArgs[1] = arg1;
            return Format(format, twoArgs);
        }

        public static string Format(string format, object arg0, object arg1, object arg2)
        {
            if (format == null)
            {
                throw new Exception("Format is invalid.");
            }

            if (threeArgs == null)
            {
                threeArgs = new object[3];
            }

            threeArgs[0] = arg0;
            threeArgs[1] = arg1;
            threeArgs[2] = arg2;
            return Format(format, threeArgs);
        }

        public static string Format(string template, params object[] args)
        {
            if (string.IsNullOrEmpty(template))
            {
                return template;
            }

            // 初始化定义好的长度以获得更好的性能
            CachedStringBuilder();

            // 记录已经处理到的位置
            var readIndex = 0;
            for (var i = 0; i < args.Length; i++)
            {
                // 占位符所在位置
                var placeholderIndex = template.IndexOf(EMPTY_JSON, readIndex);
                // 剩余部分无占位符
                if (placeholderIndex == -1)
                {
                    // 不带占位符的模板直接返回
                    if (readIndex == 0)
                    {
                        return template;
                    }

                    break;
                }

                cachedStringBuilder.Append(template, readIndex, placeholderIndex - readIndex);
                cachedStringBuilder.Append(args[i]);
                readIndex = placeholderIndex + 2;
            }

            // 字符串模板剩余部分不再包含占位符，加入剩余部分后返回结果
            cachedStringBuilder.Append(template, readIndex, template.Length - readIndex);
            return cachedStringBuilder.ToString();
        }

        public static string SubstringAfterFirst(string str, string separator)
        {
            if (string.IsNullOrEmpty(str))
            {
                return str;
            }

            if (separator == null)
            {
                return EMPTY;
            }

            var pos = str.IndexOf(separator);
            if (pos < 0)
            {
                return EMPTY;
            }

            return str.Substring(pos + separator.Length);
        }

        public static string SubstringBeforeFirst(string str, string separator)
        {
            if (string.IsNullOrEmpty(str) || separator == null)
            {
                return str;
            }

            if (string.IsNullOrEmpty(separator))
            {
                return EMPTY;
            }

            var pos = str.IndexOf(separator);
            if (pos < 0)
            {
                return str;
            }

            return str.Substring(0, pos);
        }

        public static StringBuilder CachedStringBuilder()
        {
            if (cachedStringBuilder == null)
            {
                cachedStringBuilder = new StringBuilder(1024);
            }

            cachedStringBuilder.Clear();
            return cachedStringBuilder;
        }

        // Joining
        //-----------------------------------------------------------------------

        public static string DefaultString(string str, string defaultStr)
        {
            return str == null ? defaultStr : str;
        }

        public static byte[] Bytes(string str)
        {
            try
            {
                return ConverterUtils.GetBytes(str);
            }
            catch (Exception e)
            {
                return CollectionUtils.EMPTY_BYTE_ARRAY;
            }
        }

        public static string BytesToString(byte[] bytes)
        {
            try
            {
                return ConverterUtils.GetString(bytes);
            }
            catch (Exception e)
            {
                return EMPTY;
            }
        }

        /**
         * Joins the elements of the provided varargs into a single String containing the provided elements.
         * No delimiter is added before or after the list.
         * null elements and separator are treated as empty Strings ("").
         *
         * <pre>
         * StringUtils.joinWith(",", {"a", "b"})        = "a,b"
         * StringUtils.joinWith(",", {"a", "b",""})     = "a,b,"
         * StringUtils.joinWith(",", {"a", null, "b"})  = "a,,b"
         * StringUtils.joinWith(null, {"a", "b"})       = "ab"
         * </pre>
         *
         * @param separator the separator character to use, null treated as ""
         * @param objects   the varargs providing the values to join together. {@code null} elements are treated as ""
         * @return the joined String.
         * @throws java.lang.IllegalArgumentException if a null varargs is provided
         */
        public static string JoinWith(string separator, params object[] objects)
        {
            if (objects == null)
            {
                throw new Exception("Object varargs must not be null");
            }

            var sanitizedSeparator = DefaultString(separator, EMPTY);

            var builder = CachedStringBuilder();
            for (var i = 0; i < objects.Length; i++)
            {
                var value = objects[i].ToString();
                builder.Append(value);

                if (i < objects.Length - 1)
                {
                    builder.Append(sanitizedSeparator);
                }
            }

            return builder.ToString();
        }
    }
}