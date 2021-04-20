namespace BusinessApp.Infrastructure
{
    /// <summary>
    /// The supported operators for <see cref="IOperationQuery{T}"/>
    /// </summary>
    public static class QueryOperators
    {
        public const string Equal = "eq";
        public const string NotEqual = "neq";
        public const string Contains = "in";
        public const string GreaterThanOrEqualTo = "gte";
        public const string LessThanOrEqualTo = "lte";
        public const string GreaterThan = "gt";
        public const string LessThan = "lt";
        public const string StartsWith = "sw";
    }
}
