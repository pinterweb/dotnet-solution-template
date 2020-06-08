namespace BusinessApp.App
{
    using BusinessApp.Domain;

    public class StringLogEntryFormatter : ILogEntryFormatter
    {
        public string Format(LogEntry entry) => entry.ToString();
    }
}
