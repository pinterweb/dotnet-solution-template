namespace BusinessApp.Domain
{
    public interface ILogEntryFormatter
    {
        string Format(LogEntry entry);
    }
}
