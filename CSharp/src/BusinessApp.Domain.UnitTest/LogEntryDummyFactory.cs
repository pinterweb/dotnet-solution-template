namespace BusinessApp.Domain.UnitTest
{
    using FakeItEasy;

    public class LogEntryDummyFactory : DummyFactory<LogEntry>
    {
        protected override LogEntry Create() => new LogEntry(LogSeverity.Info, "Dummy Entry");
    }
}
