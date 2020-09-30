namespace BusinessApp.WebApi.ProblemDetails
{
    using System;

    public class ProblemDetailOptions
    {
        public Type ProblemType { get; set; }
        public int StatusCode { get; set; }
        public string AbsoluteType { get; set; }
        public string MessageOverride { get; set; }

        public override bool Equals(object obj)
        {
            if (obj is ProblemDetailOptions other)
            {
                return ProblemType.Equals(other.ProblemType);
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
