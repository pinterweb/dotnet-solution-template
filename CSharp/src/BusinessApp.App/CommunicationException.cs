namespace BusinessApp.App
{
    using System;
    using System.Globalization;

    [Serializable]
    public class CommunicationException : Exception, IFormattable
    {
        public CommunicationException(string message, Exception inner = null)
            :base(message, inner)
        {
            Data.Add("", message);
        }

        public override string ToString() => ToString("G", null);

        public string ToString(string format, IFormatProvider formatProvider)
        {
            if (formatProvider == null) formatProvider = CultureInfo.CurrentCulture;

            return Message.ToString(formatProvider);
        }
    }
}
