namespace BusinessApp.Infrastructure
{
    public interface ILogEntryFormatter
    {
        string Format(LogEntry entry);
    }
}
