using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace BusinessApp.Kernel
{
    /// <summary>
    /// Factory methods to make creating a <see cref="Result{T, TErr}" /> easier
    /// </summary>
    [DebuggerStepThrough]
    public static partial class ResultFactories
    {
        public static Result<T, string> NotNull<T>(this T? value) => value == null
            ? Result<T, string>.Error("Value cannot be null")
            : Result<T, string>.Ok(value);

        public static Result<string, string> NotEmpty(this string? value)
        {
            var nullResult = value.NotNull();

            return nullResult.AndThen(okVal => okVal.Trim().Length == 0
                ? Result<string, string>.Error("String value cannot be empty")
                : nullResult
            );
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

            return nullResult.AndThen(okVal => !okVal.Any()
                ? Result<IEnumerable<T>, string>.Error("Collection cannot be empty")
                : nullResult
            );
        }

        public static Result<T, string> Valid<T>(this T value, bool isValid) => isValid
            ? Result<T, string>.Ok(value)
            : Result<T, string>.Error("Test did not pass");
    }
}
