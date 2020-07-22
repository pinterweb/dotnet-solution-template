namespace BusinessApp.App.IntegrationTest
{
    using System;
    using System.IO;
    using FakeItEasy;
    using BusinessApp.Domain;
    using Xunit;

    [Collection(CommonFixture.Name)]
    public class ConsoleLoggerTests
    {
        private readonly ConsoleLogger sut;
        private readonly ILogEntryFormatter formatter;

        public ConsoleLoggerTests()
        {
            formatter = A.Fake<ILogEntryFormatter>();

            sut = new ConsoleLogger(formatter);
        }

        public class Constructor : ConsoleLoggerTests
        {
            [Fact]
            public void InvalidCtorArgs_ExceptionThrown()
            {
                /* Arrange */
                void shouldThrow() => new ConsoleLogger(null);

                /* Act */
                var ex = Record.Exception(shouldThrow);

                /* Assert */
                Assert.IsType<ArgumentNullException>(ex);
            }
        }

        public class Log : ConsoleLoggerTests, IDisposable
        {
            StringWriter stderr;
            StringWriter stdout;

            public Log()
            {
                stderr = new StringWriter();
                stdout = new StringWriter();
                Console.SetError(stderr);
                Console.SetOut(stdout);
            }

            [Theory]
            [InlineData(LogSeverity.Error)]
            [InlineData(LogSeverity.Critical)]
            public void AsErrorAndAbove_CallsStdErr(LogSeverity severity)
            {
                /* Arrange */
                var entry = new LogEntry(severity, "foo");
                A.CallTo(() => formatter.Format(entry)).Returns("foobar");

                /* Act */
                sut.Log(entry);

                /* Assert */
                Assert.Equal("foobar", stderr.ToString());
                Assert.Empty(stdout.ToString());
            }

            [Theory]
            [InlineData(LogSeverity.Debug)]
            [InlineData(LogSeverity.Info)]
            [InlineData(LogSeverity.Warning)]
            public void AsWarningAndBelow_CallsStdOut(LogSeverity severity)
            {
                /* Arrange */
                var entry = new LogEntry(severity, "foo");
                A.CallTo(() => formatter.Format(entry)).Returns("foobar");

                /* Act */
                sut.Log(entry);

                /* Assert */
                Assert.Equal("foobar", stdout.ToString());
                Assert.Empty(stderr.ToString());
            }

            public void Dispose()
            {
                stderr.Dispose();
                stdout.Dispose();
            }
        }
    }
}
