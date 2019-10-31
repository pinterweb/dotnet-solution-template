namespace BusinessApp.Domain
{
    using System;

    /// <summary>
    /// Holds the logged data
    /// </summary>
    public class LogEntry
    {
        public LogEntry(LogSeverity severity, string message, Exception e = null)
        {
            Severity = severity;
            Message = message;
            Exception = e;
        }

        public LogSeverity Severity { get; private set; }
        public string Message { get; private set; }
        public Exception Exception { get; private set; }
    }
}
