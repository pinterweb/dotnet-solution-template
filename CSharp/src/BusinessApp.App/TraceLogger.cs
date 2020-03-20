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

        public void Log(LogEntry entry) => Trace.WriteLine(entry.Exception);
    }
}
