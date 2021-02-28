namespace BusinessApp.Domain
{
    using System;

    /// <summary>
    /// An exception to facilitate messaging when an object invariants are broken
    /// </summary>
    [Serializable]
    public class BadStateException : Exception
    {
        public BadStateException(string message)
            : this(message, null)
        { }

        public BadStateException(string message, Exception innerException)
            : base(message, innerException)
        {
            Data[""] = message;
        }
    }
}
