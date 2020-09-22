namespace BusinessApp.App
{
    using BusinessApp.Domain;

    public class StringLogEntryFormatter : ILogEntryFormatter
    {
        public string Format(LogEntry entry)
        {
            Guard.Against.Null(entry).Expect(nameof(entry));

            return entry.ToString();
        }
    }
}
