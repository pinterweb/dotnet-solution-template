using FakeItEasy;

namespace BusinessApp.Domain.UnitTest
{
    public class LogEntryDummyFactory : DummyFactory<LogEntry>
    {
        protected override LogEntry Create() => new LogEntry(LogSeverity.Info, "Dummy Entry");
    }
}
