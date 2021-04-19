using System;
using System.Diagnostics;
using BusinessApp.Domain;

namespace BusinessApp.App
{
    /// <summary>
    /// ILogger implementation that writes to stdout or stderr
    /// </summary>
    public class ConsoleLogger : ILogger
    {
        private static readonly object syncObject = new object();
        private readonly ILogEntryFormatter formatter;

        public ConsoleLogger(ILogEntryFormatter formatter)
        {
            this.formatter = formatter.NotNull().Expect(nameof(formatter));
        }

        public virtual void Log(LogEntry entry)
        {
            var console = entry.Severity >= LogSeverity.Error ? Console.Error : Console.Out;

            lock (syncObject)
            {
                var msg = formatter.Format(entry);
                console.Write(msg);
                console.Flush();
                Debug.Write(entry.Exception);
                Trace.WriteLine(entry.ToString());
                Trace.Flush();
            }
        }
    }
}
