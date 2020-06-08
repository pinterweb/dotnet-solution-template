namespace BusinessApp.App.UnitTest
{
    using System;
    using System.IO;
    using FakeItEasy;
    using BusinessApp.Domain;
    using Xunit;
    using System.Diagnostics;

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

            [Fact]
            public void WithTrace_WritesToTrace()
            {
                /* Arrange */
                var listener = A.Fake<TraceListener>();
                Trace.Listeners.Add(listener);
                var trace = new TraceSwitch("foo", "bar");
                var entry = new LogEntry(A.Dummy<LogSeverity>(), "foo");
                A.CallTo(() => formatter.Format(entry)).Returns("foobar");

                /* Act */
                sut.Log(entry);

                /* Assert */
                A.CallTo(() => listener.WriteLine(entry.ToString())).MustHaveHappenedOnceExactly();
                A.CallTo(() => listener.Flush()).MustHaveHappenedOnceExactly();
            }

            public void Dispose()
            {
                stderr.Dispose();
                stdout.Dispose();
            }
        }
    }
}
