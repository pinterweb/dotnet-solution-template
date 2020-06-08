namespace BusinessApp.App
{
    using System;
    using BusinessApp.Domain;

    public class StringLogEntryFormatter : ILogEntryFormatter
    {
        public string Format(LogEntry entry)
        {
            if (entry == null) throw new ArgumentNullException(nameof(entry));

            return entry.ToString();
        }
    }
}
