namespace BusinessApp.App.IntegrationTest
{
    using System;
    using System.IO;
    using FakeItEasy;
    using BusinessApp.Domain;
    using Xunit;

    [Collection(CommonFixture.Name)]
    public class TraceLoggerTests : IDisposable
    {
        private readonly TraceLogger sut;
        private readonly StringWriter stdout;

        public TraceLoggerTests()
        {
            stdout = new StringWriter();
            Console.SetOut(stdout);
            sut = new TraceLogger();
        }

        public class Log : TraceLoggerTests
        {

            [Fact]
            public void WithException_WritesException()
            {
                /* Arrange */
                var entry = new LogEntry(A.Dummy<LogSeverity>(), "foo", new Exception());

                /* Act */
                sut.Log(entry);

                /* Assert */
                Assert.NotEmpty(stdout.ToString());
                Assert.Contains(Environment.NewLine, stdout.ToString());
            }

            [Fact]
            public void WithMessage_WritesMessage()
            {
                /* Arrange */
                var entry = new LogEntry(A.Dummy<LogSeverity>(), "foo");

                /* Act */
                sut.Log(entry);

                /* Assert */
                Assert.Equal("foo" + Environment.NewLine, stdout.ToString());
            }

            [Fact]
            public void WithoutMessageOrException_DoesNothing()
            {
                /* Arrange */
                var entry = new LogEntry(A.Dummy<LogSeverity>(), "");

                /* Act */
                sut.Log(entry);

                /* Assert */
                Assert.Empty(stdout.ToString());
            }
        }

        public void Dispose()
        {
            stdout.Dispose();
        }
    }
}
