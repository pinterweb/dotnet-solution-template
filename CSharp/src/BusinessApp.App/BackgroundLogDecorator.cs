namespace BusinessApp.App
{
    using System;
    using System.Linq;
    using BusinessApp.Domain;

    /// <summary>
    /// Logs the entry in a Producer/Consumer background thread
    /// </summary>
    /// <remarks>
    /// Use for expensive work, such as writing a file to disk
    /// </remarks>
    public class BackgroundLogDecorator : BackgroundWorker<LogEntry>, ILogger
    {
        private readonly ILogger logger;
        private readonly ConsoleLogger fallback;

        public BackgroundLogDecorator(ILogger logger,
            ConsoleLogger fallback)
        {
            this.logger = logger.NotNull().Expect(nameof(logger));
            this.fallback = fallback.NotNull().Expect(nameof(fallback));
        }

        public void Log(LogEntry entry) => EnQueue(entry);

        protected override void OnAddFailed(LogEntry entry) => fallback.Log(entry);

        protected override void Consume(LogEntry entry)
        {
            try
            {
                logger.Log(entry);
            }
            catch (Exception ex)
            {
                var aggregate = new AggregateException(
                    new[] { ex, entry.Exception }.Where(e => e != null));

                fallback.Log(new LogEntry(
                    entry.Severity,
                    entry.Message,
                    aggregate,
                    entry.Data));
            }
        }

        protected override void EnqueueFailed(Exception ex)
        {
            var wrapped = new Exception("Unable to log message. Falling back to Console", ex);

            fallback.Log(new LogEntry(LogSeverity.Critical,
                "Unable to process async log queue",
                wrapped,
                null));
        }
    }
}
