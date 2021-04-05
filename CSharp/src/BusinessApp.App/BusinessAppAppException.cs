namespace BusinessApp.App
{
    using System;
    using BusinessApp.Domain;

    /// <summary>
    /// Custom exception to throw when an error occurrs during application logic operations
    /// </summary>
    [Serializable]
    public class BusinessAppAppException : BusinessAppException
    {
        public BusinessAppAppException(string message, Exception? inner = null)
            :base(message, inner)
        { }
    }
}
