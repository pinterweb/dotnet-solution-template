namespace BusinessApp.Kernel
{
    public interface ILogEntryFormatter
    {
        string Format(LogEntry entry);
    }
}
