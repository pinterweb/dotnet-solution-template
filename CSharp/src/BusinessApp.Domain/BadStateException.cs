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
            :base(message)
        { }

        public BadStateException(string message, Exception innerException)
            :base(message, innerException)
        { }
    }
}
