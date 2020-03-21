namespace BusinessApp.App
{
    using System.Diagnostics;
    using BusinessApp.Domain;

    public sealed class TraceLogger : ILogger
    {
        public TraceLogger()
        {
            Trace.Listeners.Add(new TextWriterTraceListener(System.Console.Out));
        }

        public void Log(LogEntry entry)
        {
            if (entry.Exception != null)
            {
                Trace.WriteLine(entry.Exception);
                Trace.Flush();
            }
            else if (!string.IsNullOrWhiteSpace(entry.Message))
            {
                Trace.WriteLine(entry.Message);
                Trace.Flush();
            }
        }
    }
}
