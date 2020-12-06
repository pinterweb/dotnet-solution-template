namespace BusinessApp.Domain
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;

    [DebuggerStepThrough]
    public static partial class ResultFactories
    {
        public static Result<T, IFormattable> NotNull<T>(this T value)
        {
            if (value == null)
            {
                return Result<T, IFormattable>.Error($"Value cannot be null");
            }
            else
            {
                return Result<T, IFormattable>.Ok(value);
            }
        }

        public static Result<string, IFormattable> NotEmpty(this string value)
        {
            var nullResult = value.NotNull();

            return nullResult.AndThen(okVal =>
            {
                if (okVal.Trim().Length == 0)
                {
                    return Result<string, IFormattable>.Error($"String value cannot be empty");
                }

                return nullResult;
            });
        }

        public static Result<T, IFormattable> NotDefault<T>(this T value)
        {
            T defaultVal = default;
            if (EqualityComparer<T>.Default.Equals(value, defaultVal))
            {
                // null ToString = "";
                return Result<T, IFormattable>.Error($"Value cannot be equal to '{defaultVal?.ToString() ?? "null"}'");
            }

            return Result<T, IFormattable>.Ok(value);
        }

        public static Result<IEnumerable<T>, IFormattable> NotEmpty<T>(this IEnumerable<T> value)
        {
            var nullResult = value.NotNull();

            return nullResult.AndThen(okVal =>
            {
                if (okVal.Count() == 0)
                {
                    return Result<IEnumerable<T>, IFormattable>.Error($"Collection cannot be empty");
                }

                return nullResult;
            });
        }

        public static Result<T, IFormattable> Valid<T>(this T value, bool isValid)
        {
            if (isValid)
            {
                return Result<T, IFormattable>.Ok(value);
            }

            return Result<T, IFormattable>.Error($"Test did not pass");
        }
    }
}
