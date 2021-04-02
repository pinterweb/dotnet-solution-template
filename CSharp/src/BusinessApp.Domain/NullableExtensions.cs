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
    }
}
