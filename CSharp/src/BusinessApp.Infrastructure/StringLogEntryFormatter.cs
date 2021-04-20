using BusinessApp.Kernel;

namespace BusinessApp.Infrastructure
{
    public class StringLogEntryFormatter : ILogEntryFormatter
    {
        public string Format(LogEntry entry)
        {
            entry.NotNull().Expect(nameof(entry));

            return entry.ToString();
        }
    }
}
