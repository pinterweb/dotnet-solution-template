namespace BusinessApp.WebApi
{
    /// <summary>
    /// How a decorator is applied over multiple scopes
    /// </summary>
    /// <remarks>
    /// There can be many scopes depending on the request type.
    /// IEnumerable requests, macro requests and single requests all create
    /// scope(s) during a request.
    /// </remarks>
    public enum ScopeBehavior
    {
        /// <summary>
        /// The decorator is applied everytime a request to the handler is called
        /// </summary>
        All,
        /// <summary>
        /// The decorator is only applied the first time the handler is called
        /// </summary>
        Outer,
        /// <summary>
        /// The decorator is only applied the last time the handler is called
        /// </summary>
        Inner,
    }
}

