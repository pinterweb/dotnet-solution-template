namespace BusinessApp.Domain
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// Helper class to guard against invariants such as Null and Empty objects
    /// </summary>
    public static class GuardAgainst
    {
        /// <summary>
        /// Throws <see cref="ArgumentNullException" /> if null else returns the value
        /// </summary>
        /// <param name="value">the value to check</param>
        /// <param name="parameterName">the parameter name being checked</param>
        /// <exception cref="ArgumentNullException">throws when <paramref name="value" /> is null</exception>
        /// <exception cref="ArgumentException">
        /// throws when <paramref name="parameterName"/> is null or empty
        /// </exception>
        /// <returns><paramref name="value"/> when not null</returns>
        public static T Null<T>(T value, string parameterName, string msg = null)
        {
            if (value == null)
            {
                Empty(parameterName, nameof(parameterName));

                if (msg is null)
                {
                    throw new ArgumentNullException(parameterName);
                }

                throw new ArgumentNullException(parameterName, msg);
            }

            return value;
        }

        /// <summary>
        /// Throws <see cref="ArgumentException" /> if value is equal to its default value
        /// else returns the value
        /// </summary>
        /// <param name="value">the value to check</param>
        /// <param name="parameterName">the parameter name being checked</param>
        /// <exception cref="ArgumentException">
        /// throws when <paramref name="parameterName"/> the default value of {T}
        /// </exception>
        /// <returns><paramref name="value"/> when not default</returns>
        public static T Default<T>(T value, string parameterName)
        {
            if (EqualityComparer<T>.Default.Equals(value, default(T)))
            {
                throw new ArgumentException(parameterName + " should not have a value of " + default(T));
            }

            return value;
        }

        /// <summary>
        /// Throws an Exception if null/empty else returns value
        /// </summary>
        /// <param name="value">the string to check</param>
        /// <param name="parameterName">the parameter name being checked</param>
        /// <exception cref="ArgumentNullException">
        /// throws when <paramref name="value"/> is null or empty
        /// </exception>
        /// <exception cref="ArgumentException">
        /// throws when <paramref name="parameterName"/> is null or empty
        /// </exception>
        /// <returns><paramref name="value"/> when not null/empty </returns>
        public static string Empty(string value, string parameterName)
        {
            Exception e = null;

            if (value is null)
            {
                e = new ArgumentNullException(parameterName);
            }
            else if (value.Trim().Length == 0)
            {
                e = new ArgumentException(parameterName + " should not be empty");
            }

            if (e != null)
            {
                Empty(parameterName, nameof(parameterName));

                throw e;
            }

            return value;
        }

        /// <summary>
        /// Throws an exception when the value is null or  empty
        /// </summary>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="value"></param>
        /// <param name="parameterName"></param>
        /// <returns>the value or throws an exception if invalid</returns>
        public static IEnumerable<TValue> Empty<TValue>(IEnumerable<TValue> value, string parameterName)
        {
            Exception e = null;
            if (value is null)
            {
                e = new ArgumentNullException(parameterName);
            }
            else if (value.Count() == 0)
            {
                e = new ArgumentException(parameterName + " should not be empty");
            }

            if (e != null)
            {
                Empty(parameterName, nameof(parameterName));

                throw e;
            }

            return value;
        }
    }
}
