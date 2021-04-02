namespace BusinessApp.App
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading;
    using BusinessApp.Domain;

    public class SerializedLogEntryFormatter : ILogEntryFormatter
    {
        private readonly ISerializer serializer;
        private readonly ConcurrentDictionary<LogEntry, string> messageCache;

        public SerializedLogEntryFormatter(ISerializer serializer)
        {
            this.serializer = serializer.NotNull().Expect(nameof(serializer));
            messageCache = new ConcurrentDictionary<LogEntry, string>();
        }

        public string Format(LogEntry entry)
        {
            entry.NotNull().Expect(nameof(entry));

            if (messageCache.TryGetValue(entry, out string? msg))
            {
                return msg;
            }

            return messageCache.GetOrAdd(entry, GetMessage(entry));
        }

        private string GetMessage(LogEntry entry)
        {
            var exceptions = (
                entry.Exception == null ?
                new List<Exception>() :
                entry.Exception.Flatten().ToList()
            ).Select(e => new
            {
                e.Message,
                e.HResult,
                e.Source,
                e.StackTrace,
            });

            try
            {
                var data = serializer.Serialize(new
                    {
                        Severity =  entry.Severity.ToString(),
                        entry.Message,
                        entry.Data,
                        Exceptions = exceptions.ToList(),
                        entry.Logged,
                        Thread.CurrentThread.ManagedThreadId
                    });

                return string.Format("{0},{1}", Encoding.UTF8.GetString(data),
                    Environment.NewLine);
            }
            catch (Exception e)
            {
                return string.Format("Error serializing: {0}{1}{2}",
                    e.Message,
                    Environment.NewLine,
                    entry.ToString());
            }
        }
    }
}
