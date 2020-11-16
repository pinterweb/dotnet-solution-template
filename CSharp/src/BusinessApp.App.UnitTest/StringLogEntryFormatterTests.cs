namespace BusinessApp.App.UnitTest
{
    using FakeItEasy;
    using BusinessApp.Domain;
    using Xunit;

    public class StringLogEntryFormatterTests
    {
        private readonly StringLogEntryFormatter sut;

        public StringLogEntryFormatterTests()
        {
            sut = new StringLogEntryFormatter();
        }

        public class Format : StringLogEntryFormatterTests
        {
            [Fact]
            public void WithOutEntry_ExceptionThrown()
            {
                var ex = Record.Exception(() => sut.Format(null));

                Assert.IsType<BadStateException>(ex);
            }

            [Fact]
            public void WithEntry_CallsEntryToString()
            {
                var entry = A.Fake<LogEntry>(opt =>
                    opt.WithArgumentsForConstructor(new object[] { LogSeverity.Debug, "", null, null }));

                var formatted = sut.Format(entry);

                A.CallTo(() => entry.ToString()).MustHaveHappenedOnceExactly();
            }
        }
    }
}
