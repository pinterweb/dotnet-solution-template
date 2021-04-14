namespace BusinessApp.Domain.UnitTest
{
    using System;
    using Xunit;
    using BusinessApp.Domain;
    using FakeItEasy;

    public class LogEntryTests
    {
        public class Constructor : LogEntryTests
        {
            [Fact]
            public void SetsSeverityProp()
            {
                /* Arrange */
                var severity = LogSeverity.Debug;

                /* Act */
                var sut = new LogEntry(severity, "foo");

                /* Assert */
                Assert.Equal(LogSeverity.Debug, sut.Severity);
            }

            [Theory]
            [InlineData(null)]
            [InlineData("")]
            [InlineData(" ")]
            public void MsgPropMissing_Throws(string msg)
            {
                /* Act */
                var ex = Record.Exception(() => new LogEntry(A.Dummy<LogSeverity>(), msg));

                /* Assert */
                Assert.IsType<BusinessAppException>(ex);
            }

            [Fact]
            public void SetsMessageProp()
            {
                /* Arrange */
                var msg = "foo";

                /* Act */
                var sut = new LogEntry(A.Dummy<LogSeverity>(), msg);

                /* Assert */
                Assert.Equal("foo", sut.Message);
            }
        }

        public class FromException : LogEntryTests
        {
            [Fact]
            public void ExceptionMissing_Throws()
            {
                /* Arrange */
                Exception e = null;

                /* Act */
                var ex = Record.Exception(() => LogEntry.FromException(e));

                /* Assert */
                Assert.IsType<BusinessAppException>(ex);
                Assert.Equal(
                    "Logging from an exception cannot be null: Value cannot be null",
                    ex.Message);
            }

            [Fact]
            public void SetsErrorSeverity()
            {
                /* Arrange */
                var e = new Exception();

                /* Act */
                var sut = LogEntry.FromException(e);

                /* Assert */
                Assert.Equal(LogSeverity.Error, sut.Severity);
            }

            [Fact]
            public void SetsErrorMessage()
            {
                /* Arrange */
                var e = new Exception("foo");

                /* Act */
                var sut = LogEntry.FromException(e);

                /* Assert */
                Assert.Equal("foo", sut.Message);
            }

            [Fact]
            public void SetsExceptionProperty()
            {
                /* Arrange */
                var e = new Exception();

                /* Act */
                var sut = LogEntry.FromException(e);

                /* Assert */
                Assert.Same(e, sut.Exception);
            }

            [Fact]
            public void SetsDataProperty()
            {
                /* Arrange */
                var e = new Exception();

                /* Act */
                var sut = LogEntry.FromException(e);

                /* Assert */
                Assert.Same(e.Data, sut.Exception.Data);
            }
        }

        public class ToStringOverride : LogEntryTests
        {
            [Fact]
            public void NoException_FormattedForDisplay()
            {
                /* Arrange */
                var sut = new LogEntry(LogSeverity.Debug, "foo");

                /* Act */
                var display = sut.ToString();

                /* Assert */
                Assert.Matches("[0-9]{2}:[0-9]{2} [[]Debug[]] foo", display);
            }

            [Fact]
            public void WithException_FormattedForDisplay()
            {
                /* Arrange */
                var sut = new LogEntry(LogSeverity.Debug, "foo")
                {
                    Exception = new ExceptionStub("bar", "lorem")
                };
                var line = Environment.NewLine;

                /* Act */
                var display = sut.ToString();

                /* Assert */
                Assert.Matches($"[0-9]{{2}}:[0-9]{{2}} [[]Debug[]] foo bar{line}lorem{line}", display);
            }

            [Fact]
            public void WithInnerExceptions_FormattedForDisplay()
            {
                /* Arrange */
                var sut = new LogEntry(LogSeverity.Debug, "foo")
                {
                    Exception = new ExceptionStub("bar", "lorem", new ExceptionStub("lorem", "ipsum"))
                };
                var line = Environment.NewLine;

                /* Act */
                var display = sut.ToString();

                /* Assert */
                Assert.Matches(
                    $"[0-9]{{2}}:[0-9]{{2}} [[]Debug[]] foo bar{line}lorem{line}" +
                    $" lorem{line}ipsum{line}",
                    display);
            }
        }

        private class ExceptionStub : Exception
        {
            private string oldStackTrace;

            public ExceptionStub(string message, string stackTrace, Exception inner = null)
                : base(message, inner)
            {
                this.oldStackTrace = stackTrace;
            }

            public override string StackTrace
            {
                get => this.oldStackTrace;
            }
        }
    }
}
