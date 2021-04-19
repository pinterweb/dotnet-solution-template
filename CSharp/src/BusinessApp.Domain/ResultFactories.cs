using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace BusinessApp.Domain
{
    [DebuggerStepThrough]
    public static partial class ResultFactories
    {
        public static Result<T, string> NotNull<T>(this T value)
        {
            if (value == null)
            {
                return Result<T, string>.Error("Value cannot be null");
            }
            else
            {
                return Result<T, string>.Ok(value);
            }
        }

        public static Result<string, string> NotEmpty(this string value)
        {
            var nullResult = value.NotNull();

            return nullResult.AndThen(okVal =>
            {
                if (okVal.Trim().Length == 0)
                {
                    return Result<string, string>.Error("String value cannot be empty");
                }

                return nullResult;
            });
        }

        public static Result<T, string> NotDefault<T>(this T value)
        {
            T? defaultVal = default;

            if (EqualityComparer<T>.Default.Equals(value, defaultVal))
            {
                // null ToString = "";
                return Result<T, string>.Error($"Value cannot be equal to '{defaultVal?.ToString() ?? "null"}'");
            }

            return Result<T, string>.Ok(value);
        }

        public static Result<IEnumerable<T>, string> NotEmpty<T>(this IEnumerable<T> value)
        {
            var nullResult = value.NotNull();

            return nullResult.AndThen(okVal =>
            {
                if (okVal.Count() == 0)
                {
                    return Result<IEnumerable<T>, string>.Error("Collection cannot be empty");
                }

                return nullResult;
            });
        }

        public static Result<T, string> Valid<T>(this T value, bool isValid)
        {
            if (isValid)
            {
                return Result<T, string>.Ok(value);
            }

            return Result<T, string>.Error("Test did not pass");
        }
    }
}
