namespace BusinessApp.App.UnitTest
{
    using System;
    using System.Collections.Generic;
    using BusinessApp.Domain;
    using FakeItEasy;
    using Xunit;

    public class CompositeLoggerTests
    {
        private readonly List<ILogger> loggers;
        private readonly CompositeLogger sut;

        public CompositeLoggerTests()
        {
            loggers = new List<ILogger>();
            sut = new CompositeLogger(loggers);
        }

        public class Constructor : CompositeLoggerTests
        {
            public static IEnumerable<object[]> InvalidCtorArgs => new[]
            {
                new object[] { null },
            };

            [Theory, MemberData(nameof(InvalidCtorArgs))]
            public void InvalidCtorArgs_ExceptionThrown(IEnumerable<ILogger> l)
            {
                /* Arrange */
                void shouldThrow() => new CompositeLogger(l);

                /* Act */
                var ex = Record.Exception(shouldThrow);

                /* Assert */
                Assert.IsType<ArgumentNullException>(ex);
            }
        }

        public class Log : CompositeLoggerTests
        {
            private readonly LogEntry entry;

            public Log()
            {
                entry = A.Dummy<LogEntry>();
            }

            [Fact]
            public void WithInnerLoggers_LogsMessage()
            {
                /* Arrange */
                var firstLogger = A.Fake<ILogger>();
                var secondLogger = A.Fake<ILogger>();
                loggers.Add(firstLogger);
                loggers.Add(secondLogger);

                /* Act */
                sut.Log(entry);

                /* Assert */
                A.CallTo(() => firstLogger.Log(entry)).MustHaveHappenedOnceExactly();
                A.CallTo(() => secondLogger.Log(entry)).MustHaveHappenedOnceExactly();
            }
        }
    }
}
