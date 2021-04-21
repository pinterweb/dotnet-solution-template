using FakeItEasy;

namespace BusinessApp.Infrastructure.UnitTest
{
    public class LogEntryDummyFactory : DummyFactory<LogEntry>
    {
        protected override LogEntry Create() => new LogEntry(LogSeverity.Info, "Dummy Entry");
    }
}
