namespace BusinessApp.App
{
    using System;
    using System.Globalization;
    using System.Security;
    using BusinessApp.Domain;

    public class SecurityResourceException : SecurityException, IFormattable
    {
        public SecurityResourceException(string resourceName, string message, Exception inner = null)
            :base(message, inner)
        {
            ResourceName = resourceName.NotEmpty().Expect(nameof(resourceName));

            Data.Add(ResourceName, Message);
        }

        public string ResourceName { get; }

        public string ToString(string format, IFormatProvider formatProvider)
        {
            if (formatProvider == null) formatProvider = CultureInfo.CurrentCulture;

            return Message.ToString(formatProvider);
        }
    }
}
