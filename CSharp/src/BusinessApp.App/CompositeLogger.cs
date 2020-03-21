namespace BusinessApp.App
{
    using System.Collections.Generic;
    using BusinessApp.Domain;

    /// <summary>
    /// Composite logger that converts <see cref="LogEntry"/> to string and calls
    /// inner loggers
    /// </summary>
    public class CompositeLogger : ILogger
    {
        private readonly IEnumerable<ILogger> loggers;

        public CompositeLogger(IEnumerable<ILogger> loggers)
        {
            this.loggers = GuardAgainst.Null(loggers, nameof(loggers));
        }

        public virtual void Log(LogEntry entry)
        {
            foreach (var logger in loggers)
            {
                logger.Log(entry);
            }
        }
    }
}
