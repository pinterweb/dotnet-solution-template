namespace BusinessApp.Domain
{
    using System;

    /// <summary>
    /// Custom exception to throw when a exception occurrs in the core logic
    /// , wrapping the actual unhandled exception
    /// </summary>
    /// <remarks>All custom exceptions should inherit from this</remarks>
    [Serializable]
    public class BusinessAppException : Exception
    {
        public BusinessAppException(string message, Exception? inner = null)
            :base(message, inner)
        { }
    }
}
