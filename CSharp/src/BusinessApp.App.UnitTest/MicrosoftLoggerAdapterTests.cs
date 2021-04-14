using BusinessApp.Domain;

namespace BusinessApp.App.UnitTest
{
    using System;
    using FakeItEasy;
    using Xunit;
    using System.Collections.Generic;
    using Microsoft.Extensions.Logging;

    public class MicrosoftLoggerAdapterTests
    {
        private readonly MicrosoftLoggerAdapter sut;
        private readonly ILogEntryFormatter formatter;
        private readonly ILogger inner;

        public MicrosoftLoggerAdapterTests()
        {
            inner = A.Fake<ILogger>();
            formatter = A.Fake<ILogEntryFormatter>();

            sut = new MicrosoftLoggerAdapter(inner, formatter);
        }

        public class Constructor : MicrosoftLoggerAdapterTests
        {
            public static IEnumerable<object[]> InvalidArgs => new[]
            {
                new object[] { A.Dummy<ILogger>(), null },
                new object[] { null, A.Dummy<ILogEntryFormatter>() }
            };

            [Theory, MemberData(nameof(InvalidArgs))]
            public void InvalidArgs_ExceptionThrown(Microsoft.Extensions.Logging.ILogger l,
                ILogEntryFormatter f)
            {
                /* Arrange */
                void shouldThrow() => new MicrosoftLoggerAdapter(l, f);

                /* Act */
                var ex = Record.Exception(shouldThrow);

                /* Assert */
                Assert.IsType<BusinessAppException>(ex);
            }
        }

        public class Log : MicrosoftLoggerAdapterTests
        {
            [Theory]
            [InlineData(LogLevel.Debug, LogSeverity.Debug)]
            [InlineData(LogLevel.Warning, LogSeverity.Warning)]
            [InlineData(LogLevel.Error, LogSeverity.Error)]
            [InlineData(LogLevel.Critical, LogSeverity.Critical)]
            public void ConvertsToMicrosoftLogLevel(LogLevel msLevel, LogSeverity appSeverity)
            {
                /* Arrange */
                LogLevel? loggedLevel = null;
                var entry = new LogEntry(appSeverity, "foo");
                A.CallTo(() => inner.Log(A<LogLevel>._, A<EventId>._, A<LogEntry>._,
                        A<Exception>._, A<Func<LogEntry, Exception, string>>._))
                    .Invokes(ctx => loggedLevel = ctx.GetArgument<LogLevel>(0));

                /* Act */
                sut.Log(entry);

                /* Assert */
                Assert.Equal(msLevel, loggedLevel);
            }

            [Fact]
            public void DoesNotLogEventId()
            {
                /* Arrange */
                EventId eventId = default;
                var entry = new LogEntry(LogSeverity.Debug, "foo");
                A.CallTo(() => inner.Log(A<LogLevel>._, A<EventId>._, A<LogEntry>._,
                        A<Exception>._, A<Func<LogEntry, Exception, string>>._))
                    .Invokes(ctx => eventId = ctx.GetArgument<EventId>(1));

                /* Act */
                sut.Log(entry);

                /* Assert */
                Assert.Equal(0, eventId.Id);
            }

            [Fact]
            public void LogsException()
            {
                /* Arrange */
                Exception e = null;
                var entryException = new Exception();
                var entry = LogEntry.FromException(entryException);
                A.CallTo(() => inner.Log(A<LogLevel>._, A<EventId>._, A<LogEntry>._,
                        A<Exception>._, A<Func<LogEntry, Exception, string>>._))
                    .Invokes(ctx => e = ctx.GetArgument<Exception>(3));

                /* Act */
                sut.Log(entry);

                /* Assert */
                Assert.Same(entry.Exception, e);
            }

            [Fact]
            public void FormatsTheLogEntryArg()
            {
                /* Arrange */
                Func<LogEntry, Exception, string> format = null;
                var entry = new LogEntry(LogSeverity.Debug, "foo");
                A.CallTo(() => inner.Log(A<LogLevel>._, A<EventId>._, A<LogEntry>._,
                        A<Exception>._, A<Func<LogEntry, Exception, string>>._))
                    .Invokes(ctx => format = ctx.GetArgument<Func<LogEntry, Exception, string>>(4));
                sut.Log(entry);

                /* Act */
                format(entry, null);

                /* Assert */
                A.CallTo(() => formatter.Format(entry)).MustHaveHappenedOnceExactly();
            }
        }
    }
}
