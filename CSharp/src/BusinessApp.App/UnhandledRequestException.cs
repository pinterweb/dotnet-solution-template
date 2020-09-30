namespace BusinessApp.App
{
    using System;
    using System.Globalization;
    using BusinessApp.Domain;

    /// <summary>
    /// Wraps any exception that is thrown during a request so it can be formatted
    /// a <see cref="Result"/> type
    /// </summary>
    [Serializable]
    public class UnhandledRequestException : Exception, IFormattable
    {
        public UnhandledRequestException(Exception original)
            : base("An unhandled exception has occurred during the request", original)
        {
        }

        public override string ToString() => ToString("G", null);

        public string ToString(string format, IFormatProvider formatProvider)
        {
            if (formatProvider == null) formatProvider = CultureInfo.CurrentCulture;

            var innerMsg = InnerException switch
            {
                IFormattable f => f.ToString(format, formatProvider),
                _ => InnerException.Message
            };

            return $"{Message}: {innerMsg}";
        }
    }
}
