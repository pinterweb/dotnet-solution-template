namespace BusinessApp.App
{
    using System;
    using System.Globalization;

    /// <summary>
    /// Exception to throw when an entity is not found, but was expected
    /// </summary>
    [Serializable]
    public class EntityNotFoundException : Exception, IFormattable
    {
        public EntityNotFoundException(string message)
            :base(message)
        {
            Data.Add("", message);
        }

        public EntityNotFoundException(string entityName, string message = null)
            :base(message ?? $"{entityName} not found")
        {
            Data.Add(entityName, message);
        }

        public override string ToString() => ToString("G", null);

        public string ToString(string format, IFormatProvider formatProvider)
        {
            if (formatProvider == null) formatProvider = CultureInfo.CurrentCulture;

            return Message.ToString(formatProvider);
        }
    }
}
