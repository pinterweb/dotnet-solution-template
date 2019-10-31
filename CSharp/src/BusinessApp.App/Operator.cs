namespace BusinessApp.App
{
    /// <summary>
    /// The supported operators for <see cref="IOperationQuery{T}"/>
    /// </summary>
    public static class Operator
    {
        public const string Contains = "in";
        public const string Exclusion = "excl";
        public const string Inclusion = "incl";
        public const string GreaterThanOrEqualTo = "gte";
        public const string LessThanOrEqualTo = "lte";
        public const string GreaterThan = "gt";
        public const string LessThan = "lt";
    }
}
