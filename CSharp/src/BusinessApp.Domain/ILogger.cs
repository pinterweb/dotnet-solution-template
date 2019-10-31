namespace BusinessApp.Domain
{
    /// <summary>
    /// General logger
    /// </summary>
    public interface ILogger
    {
        void Log(LogEntry entry);
    }
}
