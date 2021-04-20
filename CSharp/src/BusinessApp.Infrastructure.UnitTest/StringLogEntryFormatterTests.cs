using FakeItEasy;
using BusinessApp.Kernel;
using Xunit;

namespace BusinessApp.Infrastructure.UnitTest
{
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

                Assert.IsType<BusinessAppException>(ex);
            }

            [Fact]
            public void WithEntry_CallsEntryToString()
            {
                var entry = A.Fake<LogEntry>(opt =>
                    opt.WithArgumentsForConstructor(new object[] { LogSeverity.Debug, "msg" }));

                var formatted = sut.Format(entry);

                A.CallTo(() => entry.ToString()).MustHaveHappenedOnceExactly();
            }
        }
    }
}
