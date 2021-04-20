using BusinessApp.Domain;
using MS = Microsoft.Extensions.Logging;

namespace BusinessApp.Infrastructure
{
    public sealed class MicrosoftLoggerAdapter : ILogger
    {
        private readonly MS.ILogger logger;
        private readonly ILogEntryFormatter formatter;

        public MicrosoftLoggerAdapter(MS.ILogger logger, ILogEntryFormatter formatter)
        {
            this.logger = logger.NotNull().Expect(nameof(logger));
            this.formatter = formatter.NotNull().Expect(nameof(formatter));
        }

        public void Log(LogEntry e)
            => logger.Log(ToLevel(e.Severity), 0, e, e.Exception, (s, _) => formatter.Format(s));

        private static MS.LogLevel ToLevel(LogSeverity s) =>
            s switch
            {
                LogSeverity.Info => MS.LogLevel.Information,
                LogSeverity.Warning => MS.LogLevel.Warning,
                LogSeverity.Error => MS.LogLevel.Error,
                LogSeverity.Critical => MS.LogLevel.Critical,
                _ => MS.LogLevel.Debug,
            };
    }
}
