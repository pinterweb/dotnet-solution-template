namespace BusinessApp.Kernel
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
            => obj ?? throw new BusinessAppException("Object cannot be access because it is null");

        /// <summary>
        /// Returns the underlying object or throws an exception with the messge
        /// </summary>
        public static T Expect<T>(this T? obj, string message)
            => obj ?? throw new BusinessAppException($"{message}: object cannot be null");
    }
}
