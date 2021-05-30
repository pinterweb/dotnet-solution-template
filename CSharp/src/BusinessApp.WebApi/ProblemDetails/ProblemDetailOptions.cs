using System;

namespace BusinessApp.WebApi.ProblemDetails
{
    /// <summary>
    /// Options that help create <see cref="ProblemDetail" />
    /// </summary>
    public class ProblemDetailOptions
    {
        public ProblemDetailOptions(Type problemType, int statusCode)
        {
            ProblemType = problemType;
            StatusCode = statusCode;
        }

        public Type ProblemType { get; }
        public int StatusCode { get; }
        public string? AbsoluteType { get; init; }
        public string? MessageOverride { get; set; }

        public override bool Equals(object? obj) => obj is ProblemDetailOptions other
            ? ProblemType?.Equals(other.ProblemType) ?? false
            : base.Equals(obj);

        public override int GetHashCode()
        {
            unchecked
            {
                var hash = 17;
                const int multiplier = 23;

                return (hash * multiplier) + (ProblemType?.GetHashCode() ?? 0);
            }
        }
    }
}
