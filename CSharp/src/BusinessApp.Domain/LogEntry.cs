namespace BusinessApp.Domain
{
    using System;
    using System.Linq;
    using System.Text;

    /// <summary>
    /// Holds the data to log
    /// </summary>
    public class LogEntry
    {
        public LogEntry(LogSeverity severity,
            string message,
            Exception e = null,
            object data = null)
        {
            Severity = severity;
            Message = message;
            Exception = e;
            Data = data;
        }

        public static LogEntry FromException(Exception e)
        {
            return new LogEntry(LogSeverity.Error, e?.Message, e, e.Data);
        }

        public LogSeverity Severity { get; }
        public string Message { get; }
        public object Data { get; }
        public Exception Exception { get; }
        public DateTimeOffset Logged { get; } = DateTimeOffset.UtcNow;

        public override string ToString()
        {
            var exceptions = Exception?.Flatten();
            var nl = Environment.NewLine;
            var sb = new StringBuilder();
            var header = new StringBuilder($"{Logged.ToString("HH:mm")} [{Severity.ToString()}] {Message}");

            sb.Append(header);

            if (exceptions != null && exceptions.Any())
            {
                foreach (var ex in exceptions)
                {
                    sb.Append(" ");
                    sb.Append(ex.Message);
                    sb.Append(nl);
                    sb.Append(ex.StackTrace);
                    sb.Append(nl);
                }
            }
            else
            {
                sb.Append(nl);
            }

            return sb.ToString();
        }
    }
}
