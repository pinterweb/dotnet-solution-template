namespace BusinessApp.Domain
{
    using System;

    /// <summary>
    /// An exception to facilitate messaging when an object invariants are broken
    /// </summary>
    [Serializable]
    public class BadStateException : BusinessAppException
    {
        public BadStateException(string message, Exception? innerException = null)
            : base(message, innerException)
        { }
    }
}
