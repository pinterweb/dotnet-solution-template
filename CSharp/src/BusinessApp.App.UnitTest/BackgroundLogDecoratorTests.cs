namespace BusinessApp.App.UnitTest
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using FakeItEasy;
    using BusinessApp.Domain;
    using Xunit;

    public class BackgroundLogDecoratorerTests
    {
        private readonly BackgroundLogDecorator sut;
        private readonly ILogger logger;
        private readonly ConsoleLogger fallback;

        public BackgroundLogDecoratorerTests()
        {
            logger = A.Fake<ILogger>();
            fallback = A.Fake<ConsoleLogger>();

            sut = new BackgroundLogDecorator(logger, fallback);
        }

        public class Constructor : BackgroundLogDecoratorerTests
        {
            public static IEnumerable<object[]> InvalidCtorArgs => new[]
            {
                new object[] { null, A.Dummy<ConsoleLogger>() },
                new object[] { A.Dummy<ILogger>(), null },
            };

            [Theory, MemberData(nameof(InvalidCtorArgs))]
            public void InvalidCtorArgs_ExceptionThrown(ILogger l, ConsoleLogger f)
            {
                /* Arrange */
                void shouldThrow() => new BackgroundLogDecorator(l, f);

                /* Act */
                var ex = Record.Exception(shouldThrow);

                /* Assert */
                Assert.IsType<ArgumentNullException>(ex);
            }
        }

        public class Log : BackgroundLogDecoratorerTests
        {
            [Fact]
            public async Task Log_WithEntry_CallsDecoratedLogger()
            {
                /* Arrange */
                var entry = A.Dummy<LogEntry>();

                /* Act */
                sut.Log(entry);
                await Task.Delay(TimeSpan.FromSeconds(3));

                /* Assert */
                A.CallTo(() => logger.Log(entry)).MustHaveHappenedOnceExactly();
            }

            [Fact]
            public async Task Log_WhenHasMultipleEntry_LoggedInOrder()
            {
                /* Arrange */
                var entry1 = A.Dummy<LogEntry>();
                var entry2 = A.Dummy<LogEntry>();

                /* Act */
                sut.Log(entry1);
                sut.Log(entry2);
                await Task.Delay(TimeSpan.FromSeconds(3));

                /* Assert */
                A.CallTo(() => logger.Log(entry1)).MustHaveHappenedOnceExactly()
                    .Then(A.CallTo(() => logger.Log(entry2)).MustHaveHappenedOnceExactly());
            }

            [Fact]
            public async Task Log_WhenDisposed_NotLogged()
            {
                /* Arrange */
                sut.Dispose();

                /* Act */
                sut.Log(A.Dummy<LogEntry>());
                await Task.Delay(TimeSpan.FromSeconds(3));

                /* Assert */
                A.CallTo(() => logger.Log(A<LogEntry>._)).MustNotHaveHappened();
            }

            [Fact]
            public async Task Log_WhenDisposed_ConsoleLogged()
            {
                /* Arrange */
                var entry = A.Dummy<LogEntry>();
                sut.Dispose();

                /* Act */
                sut.Log(entry);
                await Task.Delay(TimeSpan.FromSeconds(3));

                /* Assert */
                A.CallTo(() => fallback.Log(entry)).MustHaveHappenedOnceExactly();
            }

            [Fact]
            public async Task Log_WhenLoggerThrows_ConsoleLogged()
            {
                /* Arrange */
                A.CallTo(() => logger.Log(A<LogEntry>._)).Throws<Exception>();

                /* Act */
                sut.Log(A.Dummy<LogEntry>());
                await Task.Delay(TimeSpan.FromSeconds(3));

                /* Assert */
                A.CallTo(() => fallback.Log(A<LogEntry>._)).MustHaveHappenedOnceExactly();
            }

            [Fact]
            public async Task Log_WhenLoggerThrows_NewLogEntryCreated()
            {
                /* Arrange */
                var data = new { };
                LogEntry consoleEntry = null;
                LogEntry originalEntry = new LogEntry(LogSeverity.Critical,
                    "lorem",
                    null,
                    data);
                A.CallTo(() => logger.Log(A<LogEntry>._)).Throws<Exception>();
                A.CallTo(() => fallback.Log(A<LogEntry>._))
                    .Invokes(ctx => consoleEntry = ctx.GetArgument<LogEntry>(0));

                /* Act */
                sut.Log(originalEntry);
                await Task.Delay(TimeSpan.FromSeconds(3));

                /* Assert */
                Assert.Equal(LogSeverity.Critical, consoleEntry.Severity);
                Assert.Equal("lorem", consoleEntry.Message);
                Assert.Same(data, consoleEntry.Data);
            }

            [Fact]
            public async Task Log_WhenLoggerThrows_NewExceptionCreated()
            {
                /* Arrange */
                var data = new { };
                var originalException = new Exception("ipsum");
                var newException = new Exception("foobar");
                LogEntry consoleEntry = null;
                LogEntry originalEntry = new LogEntry(LogSeverity.Critical,
                    "lorem",
                    originalException);
                A.CallTo(() => logger.Log(A<LogEntry>._)).Throws(newException);
                A.CallTo(() => fallback.Log(A<LogEntry>._))
                    .Invokes(ctx => consoleEntry = ctx.GetArgument<LogEntry>(0));

                /* Act */
                sut.Log(originalEntry);
                await Task.Delay(TimeSpan.FromSeconds(3));

                /* Assert */
                var aggregate = Assert.IsType<AggregateException>(consoleEntry.Exception);
                Assert.Contains(originalException, aggregate.Flatten().InnerExceptions);
                Assert.Contains(newException, aggregate.Flatten().InnerExceptions);
            }
        }
    }
}

