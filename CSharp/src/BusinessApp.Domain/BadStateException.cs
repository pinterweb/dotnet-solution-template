namespace BusinessApp.Domain
{
    using System;

    /// <summary>
    /// An exception to facilitate messaging when an object invariants are broken
    /// </summary>
    [Serializable]
    public class BadStateException : Exception
    {
        public BadStateException(string message, Exception? innerException = null)
            : base(message, innerException)
        {
            Data[""] = message;
        }
    }
}
