namespace BusinessApp.Domain
{
    using System;
    using System.Globalization;

    /// <summary>
    /// An exception to facilitate messaging when an object invariants are broken
    /// </summary>
    [Serializable]
    public class BadStateException : Exception, IFormattable
    {
        public BadStateException(string message)
            : this(message, null)
        { }

        public BadStateException(string message, Exception innerException)
            : base(message, innerException)
        {
            Data[""] = message;
        }

        public override string ToString()
        {
            return this.ToString("", CultureInfo.CurrentCulture);
        }

        public string ToString(string format, IFormatProvider formatProvider)
        {
            return Message;
        }
    }
}
