namespace BusinessApp.Domain
{
    /// <summary>
    /// Functional extensions to handle null references
    /// </summary>
    public static class NullableExtensions
    {
        /// <summary>
        /// Returns the underlying object or throws if null
        /// </summary>
        public static T Unwrap<T>(this T? obj)
        {
            return obj ?? throw new BadStateException("Object cannot be access because it is null");
        }

        public static T Expect<T>(this T? obj, string message)
        {
            return obj ?? throw new BadStateException($"{message}: object cannot be null");
        }
    }
}
