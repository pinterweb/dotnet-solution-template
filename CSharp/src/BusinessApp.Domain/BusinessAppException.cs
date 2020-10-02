namespace BusinessApp.Domain
{
    using System;
    using System.Globalization;

    /// <summary>
    /// Custom exception to throw when a exception occurrs in the core logic
    /// , wrapping the actual unhandled exception
    /// </summary>
    /// <remarks>All custom exceptions should inherit from this</remarks>
    [Serializable]
    public class BusinessAppException : Exception, IFormattable
    {
        public BusinessAppException(string message, Exception inner = null)
            :base(message, inner)
        { }

        public string ToString(string format, IFormatProvider formatProvider)
        {
            if (formatProvider == null) formatProvider = CultureInfo.CurrentCulture;

            return Message.ToString(formatProvider);
        }

        public override string ToString()
        {
            return ToString("G", null);
        }
    }
}
