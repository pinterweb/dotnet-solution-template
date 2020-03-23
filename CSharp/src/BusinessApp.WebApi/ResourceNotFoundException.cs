namespace BusinessApp.WebApi
{
    using System;

    /// <summary>
    /// Exception when a resource was expected, but not found
    /// </summary>
    [Serializable]
    public class ResourceNotFoundException : Exception
    {
        public ResourceNotFoundException(string message = null, Exception inner = null)
            : base(message ?? "The resource you are looking for does not exist")
        { }
    }
}
