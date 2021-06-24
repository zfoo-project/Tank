using System;
using System.Collections;

namespace Spring.Util
{
    public abstract class AssertionUtils
    {
        public static void Equals(object a, object b)
        {
            if (a.Equals(b))
            {
                return;
            }

            throw new Exception(StringUtils.Format("[a:{}] is not equals [b:{}]", a, b));
        }

        public static void Equals<T>(T[] a, T[] b)
        {
            if (a == b)
            {
                return;
            }


            if (a != null && b != null && a.Length == b.Length)
            {
                for (var i = 0; i < a.Length; i++)
                {
                    Equals(a[i], b[i]);
                }

                return;
            }

            throw new Exception(StringUtils.Format("[a:{}] is not equals [b:{}]", a, b));
        }

        // ----------------------------------bool----------------------------------
        /**
         * Assert a bool expression, throwing {@code IllegalArgumentException}
         * if the test result is {@code false}.
         * @param expression a bool expression
         * @param message    the exception message to use if the assertion fails
         * @throws IllegalArgumentException if expression is {@code false}
         */
        public static void IsTrue(bool expression, string message)
        {
            if (!expression)
            {
                throw new Exception(message);
            }
        }

        /**
         * 可支持带参数format的类型：类{}的成员变量：{}不能有set方法：{}
         *
         * @param expression 表达式
         * @param format     格式
         * @param args       参数
         */
        public static void IsTrue(bool expression, string format, params object[] args)
        {
            if (!expression)
            {
                throw new Exception(StringUtils.Format(format, args));
            }
        }

        public static void IsTrue(bool expression)
        {
            IsTrue(expression, "[Assertion failed] - this expression must be true");
        }


        // ----------------------------------long----------------------------------

        /**
         * lt 参数1是否小于参数2
         * le 参数1是否小于等于参数2
         * gt 参数1是否大于参数2
         * ge 参数1是否大于等于参数2
         */
        public static void Ge(long x, long y)
        {
            if (x < y)
            {
                throw new Exception(StringUtils.Format("[Assertion failed] - the param [x:{}] must greater or equal [y:{}]", x, y));
            }
        }

        public static void Le(long x, long y)
        {
            if (x > y)
            {
                throw new Exception(StringUtils.Format("[Assertion failed] - the param [x:{}] must less or equal [y:{}]", x, y));
            }
        }

        public static void Ge0(long x)
        {
            Ge(x, 0);
        }

        public static void Ge1(long x)
        {
            Ge(x, 1);
        }

        public static void Le0(long x)
        {
            Le(x, 1);
        }

        public static void Le1(long x)
        {
            Le(x, 1);
        }


        // ----------------------------------collection----------------------------------

        /**
         * Assert that a collection has elements; that is, it must not be
         * {@code null} and must have at least one element.
         *
         * @param collection the collection to check
         * @param message    the exception message to use if the assertion fails
         * @throws IllegalArgumentException if the collection is {@code null} or has no elements
         */
        public static void NotEmpty(ICollection collection, string message)
        {
            if (collection == null || collection.Count <= 0)
            {
                throw new Exception(message);
            }
        }

        public static void NotEmpty(ICollection collection)
        {
            NotEmpty(collection, "[Assertion failed] - this collection must not be empty: it must contain at least 1 element");
        }


        // ----------------------------------object----------------------------------

        /**
         * Assert that an object is {@code null} .
         *
         * @param object  the object to check
         * @param message the exception message to use if the assertion fails
         * @throws IllegalArgumentException if the object is not {@code null}
         */
        public static void IsNull(object obj, string message)
        {
            if (obj != null)
            {
                throw new Exception(message);
            }
        }

        public static void IsNull(object obj, string format, params object[] args)
        {
            if (obj != null)
            {
                throw new Exception(StringUtils.Format(format, args));
            }
        }

        public static void IsNull(object obj)
        {
            IsNull(obj, "[Assertion failed] - the object argument must be null");
        }

        public static void NotNull(object obj, string message)
        {
            if (obj == null)
            {
                throw new Exception(message);
            }
        }

        public static void NotNull(object obj, string format, params object[] args)
        {
            if (obj == null)
            {
                throw new Exception(StringUtils.Format(format, args));
            }
        }

        public static void NotNull(object obj)
        {
            NotNull(obj, "[Assertion failed] - this argument is required; it must not be null");
        }

        public static void NotNull(params object[] objects)
        {
            for (int i = 0; i < objects.Length; i++)
            {
                NotNull(objects[i], "the [index:{}] of objects must not be null", i);
            }
        }

        /**
         * Assert that the provided object is an instance of the provided class.
         *
         * @param type    the type to check against
         * @param obj     the object to check
         * @param message a message which will be prepended to the message produced by
         *                the function itself, and which may be used to provide context. It should
         *                normally end in ":" or "." so that the generated message looks OK when
         *                appended to it.
         * @throws IllegalArgumentException if the object is not an instance of clazz
         * @see Class#isInstance
         */
        public static void IsInstanceOf(Type type, object obj, string message)
        {
            NotNull(type, "Type to check against must not be null");
            if (!type.IsInstanceOfType(obj))
            {
                throw new Exception(StringUtils.Format("Object of class [{}] must be an instance of [{}] [{}]", obj, type, message));
            }
        }

        public static void IsInstanceOf(Type clazz, Object obj)
        {
            IsInstanceOf(clazz, obj, StringUtils.EMPTY);
        }

        /**
         * Assert that {@code superType.isAssignableFrom(subType)} is {@code true}.
         * <pre class="code">Assert.isAssignable(Number.class, myClass);</pre>
         *
         * @param superType the super type to check against
         * @param subType   the sub type to check
         * @param message   a message which will be prepended to the message produced by
         *                  the function itself, and which may be used to provide context. It should
         *                  normally end in ":" or "." so that the generated message looks OK when
         *                  appended to it.
         * @throws IllegalArgumentException if the classes are not assignable
         */
        public static void IsAssignable(Type superType, Type subType, string message)
        {
            NotNull(superType, "Type to check against must not be null");
            if (subType == null || !superType.IsAssignableFrom(subType))
            {
                throw new Exception(StringUtils.Format("[{}] is not assignable to [{}] [{}]", subType, subType, message));
            }
        }

        public static void IsAssignable(Type superType, Type subType)
        {
            IsAssignable(superType, subType, StringUtils.EMPTY);
        }
    }
}