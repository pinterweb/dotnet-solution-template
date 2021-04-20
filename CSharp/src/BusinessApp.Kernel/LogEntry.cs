using System;
using System.Linq;
using System.Text;

namespace BusinessApp.Kernel
{
    /// <summary>
    /// Holds the data to log
    /// </summary>
    public class LogEntry
    {
        public LogEntry(LogSeverity severity, string message)
        {
            Severity = severity;
            Message = message.NotEmpty().Expect("A log entry must have a message");
        }

        public static LogEntry FromException(Exception e)
        {
            e.NotNull().Expect("Logging from an exception cannot be null");

            return new LogEntry(LogSeverity.Error, e.Message)
            {
                Exception = e,
                Data = e.Data
            };
        }

        public LogSeverity Severity { get; init; }
        public string Message { get; init; }
        public object? Data { get; init; }
        public Exception? Exception { get; init; }
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
