using System;

namespace BusinessApp.WebApi.ProblemDetails
{
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

        public override bool Equals(object? obj)
        {
            if (obj is ProblemDetailOptions other)
            {
                return ProblemType?.Equals(other.ProblemType) ?? false;
            }

            return base.Equals(obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                    // Choose large primes to avoid hashing collisions
                const int HashingBase = (int)2166136261;
                const int HashingMultiplier = 16777619;
                return (HashingBase * HashingMultiplier) ^ ProblemType?.GetHashCode() ?? 0;
            }
        }
    }
}
