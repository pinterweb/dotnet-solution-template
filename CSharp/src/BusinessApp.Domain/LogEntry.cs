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

        public LogSeverity Severity { get; }
        public string Message { get; }
        public object Data { get; }
        public Exception Exception { get; }
        public DateTimeOffset Logged { get; } = DateTimeOffset.UtcNow;

        public override bool Equals(object obj)
        {
            if (obj is LogEntry other)
            {
                return Severity.Equals(other.Severity) &&
                    Message == other.Message &&
                    (Data?.Equals(other.Data) ?? other.Data == null) &&
                    (Exception?.Equals(other.Exception) ?? other.Exception == null) &&
                    Logged.Equals(other.Logged);
            }

            return base.Equals(obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                // Choose large primes to avoid hashing collisions
                const int HashingBase = (int)2166136261;
                const int HashingMultiplier = 16777619;
                int hash = (HashingBase * HashingMultiplier) ^ Severity.GetHashCode();
                hash = (hash * HashingMultiplier) ^ Message?.GetHashCode() ?? 0;
                hash = (hash * HashingMultiplier) ^ Data?.GetHashCode() ?? 0;
                hash = (hash * HashingMultiplier) ^ Exception?.GetHashCode() ?? 0;
                hash = (hash * HashingMultiplier) ^ Logged.GetHashCode();

                return hash;
            }
        }

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
