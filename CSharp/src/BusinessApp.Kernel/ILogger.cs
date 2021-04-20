namespace BusinessApp.Kernel
{
    /// <summary>
    /// General logger
    /// </summary>
    public interface ILogger
    {
        void Log(LogEntry entry);
    }
}
