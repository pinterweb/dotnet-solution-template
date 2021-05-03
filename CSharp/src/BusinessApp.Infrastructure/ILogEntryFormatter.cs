namespace BusinessApp.Infrastructure
{
    /// <summary>
    /// Formats/Serializes a log entry into a string
    /// </summary>
    public interface ILogEntryFormatter
    {
        string Format(LogEntry entry);
    }
}
