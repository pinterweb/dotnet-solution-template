namespace BusinessApp.Domain
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;

    /// <summary>
    /// Helper class to guard against invariants
    /// </summary>
    [DebuggerStepThrough]
    public static partial class Guard
    {
        public static class Against
        {
            public static Result<T, IFormattable> Null<T>(T value)
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

            public static Result<string, IFormattable> Empty(string value)
            {
                var nullResult = Guard.Against.Null(value);

                return nullResult.AndThen(okVal =>
                {
                    if (okVal.Trim().Length == 0)
                    {
                        return Result<string, IFormattable>.Error($"String value cannot be empty");
                    }

                    return nullResult;
                });
            }

            public static Result<T, IFormattable> Default<T>(T value)
            {
                T defaultVal = default;
                if (EqualityComparer<T>.Default.Equals(value, defaultVal))
                {
                    // null ToString = "";
                    return Result<T, IFormattable>.Error($"Value cannot be equal to '{defaultVal?.ToString() ?? "null"}'");
                }

                return Result<T, IFormattable>.Ok(value);
            }

            public static Result<IEnumerable<T>, IFormattable> Empty<T>(IEnumerable<T> value)
            {
                var nullResult = Guard.Against.Null(value);

                return nullResult.AndThen(okVal =>
                {
                    if (okVal.Count() == 0)
                    {
                        return Result<IEnumerable<T>, IFormattable>.Error($"Collection cannot be empty");
                    }

                    return nullResult;
                });
            }
        }
    }
}
