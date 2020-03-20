namespace ShelLife.Domain.UnitTest
{
    using System;
    using Xunit;
    using BusinessApp.Domain;

    public class LogEntryTests
    {
        [Fact]
        public void Constructor_AllArgsValid_AllPropsSet()
        {
            /* Arrange */
            var ex = new Exception();
            var data = new {};

            /* Act */
            var sut = new LogEntry(LogSeverity.Debug, "foo", ex, data);

            /* Assert */
            Assert.Equal(LogSeverity.Debug, sut.Severity);
            Assert.Equal("foo", sut.Message);
            Assert.Same(ex, sut.Exception);
            Assert.Same(data, sut.Data);
        }

        [Fact]
        public void ToString_NoException_FormattedForDisplay()
        {
            /* Arrange */
            var sut = new LogEntry(LogSeverity.Debug, "foo");

            /* Act */
            var display = sut.ToString();

            /* Assert */
            Assert.Matches("[0-9]{2}:[0-9]{2} [[]Debug[]] foo", display);
        }

        [Fact]
        public void ToString_WithException_FormattedForDisplay()
        {
            /* Arrange */
            var sut = new LogEntry(LogSeverity.Debug, "foo", new ExceptionStub("bar", "lorem"));
            var line = Environment.NewLine;

            /* Act */
            var display = sut.ToString();

            /* Assert */
            Assert.Matches($"[0-9]{{2}}:[0-9]{{2}} [[]Debug[]] foo bar{line}lorem{line}", display);
        }

        [Fact]
        public void ToString_WithInnerExceptions_FormattedForDisplay()
        {
            /* Arrange */
            var sut = new LogEntry(LogSeverity.Debug, "foo", new ExceptionStub("bar", "lorem", new ExceptionStub("lorem", "ipsum")));
            var line = Environment.NewLine;

            /* Act */
            var display = sut.ToString();

            /* Assert */
            Assert.Matches(
                $"[0-9]{{2}}:[0-9]{{2}} [[]Debug[]] foo bar{line}lorem{line}" +
                $" lorem{line}ipsum{line}",
                display);
        }

        private class ExceptionStub : Exception
        {
            private string oldStackTrace;

            public ExceptionStub(string message, string stackTrace, Exception inner = null) : base(message, inner)
            {
                this.oldStackTrace = stackTrace;
            }


            public override string StackTrace
            {
                get
                {
                    return this.oldStackTrace;
                }
            }
        }
    }
}
