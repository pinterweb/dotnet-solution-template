using System;
using System.Collections.Generic;
using System.Collections;
using FakeItEasy;
using BusinessApp.Domain;
using BusinessApp.Test.Shared;
using Xunit;
using System.Linq;

namespace BusinessApp.App.UnitTest
{
    public class SerializedLogEntryFormatterTests
    {
        private readonly SerializedLogEntryFormatter sut;
        private readonly ISerializer serializer;

        public SerializedLogEntryFormatterTests()
        {
            serializer = A.Fake<ISerializer>();

            sut = new SerializedLogEntryFormatter(serializer);
        }

        public class Constructor : SerializedLogEntryFormatterTests
        {
            public static IEnumerable<object[]> InvalidCtorArgs => new[]
            {
                new object[] { null }
            };

            [Theory, MemberData(nameof(InvalidCtorArgs))]
            public void InvalidCtorArgs_ExceptionThrown(ISerializer s)
            {
                void shouldThrow() => new SerializedLogEntryFormatter(s);

                var ex = Record.Exception(shouldThrow);

                Assert.IsType<BusinessAppException>(ex);
            }
        }

        public class Format : SerializedLogEntryFormatterTests
        {
            private object serializedObj;

            public Format()
            {
                serializedObj = null;

                A.CallTo(serializer)
                    .Where(ctx => ctx.Method.Name == "Serialize")
                    .Invokes(ctx => serializedObj = ctx.GetArgument<object>(0));
            }

            [Fact]
            public void WithOutEntry_ExceptionThrown()
            {
                var ex = Record.Exception(() => sut.Format(null));

                Assert.IsType<BusinessAppException>(ex);
            }

            [Fact]
            public void WithEntry_SeveritySerialized()
            {
                var entry = new LogEntry(LogSeverity.Info, "foobar");

                var formatted = sut.Format(entry);

                Assert.Equal("Info", serializedObj.GetProp("Severity"));
            }

            [Fact]
            public void WithEntry_MessageSerialized()
            {
                var entry = new LogEntry(LogSeverity.Info, "foobar");

                var formatted = sut.Format(entry);

                Assert.Equal("foobar", serializedObj.GetProp("Message"));
            }

            [Fact]
            public void WithEntry_DataSerialized()
            {
                object data = new {};
                var entry = new LogEntry(LogSeverity.Info, "foobar")
                {
                    Data = data
                };

                var formatted = sut.Format(entry);

                Assert.Same(data, serializedObj.GetProp("Data"));
            }

            [Fact]
            public void WithOneExceptionEntry_ExceptionMessageSerialized()
            {
                var exception = new Exception("foobar");
                var entry = new LogEntry(LogSeverity.Info, "foobar")
                {
                    Exception = exception
                };

                var formatted = sut.Format(entry);

                var exceptions = Assert.IsAssignableFrom<IEnumerable>(serializedObj.GetProp("Exceptions"));
                var targetException = Assert.Single(exceptions);
                Assert.Same("foobar", targetException.GetProp("Message"));
            }

            [Fact]
            public void WithOneExceptionEntry_ExceptionHResultSerialized()
            {
                var exception = new Exception();
                var entry = new LogEntry(LogSeverity.Info, "foobar")
                {
                    Exception = exception
                };

                var formatted = sut.Format(entry);

                var exceptions = Assert.IsAssignableFrom<IEnumerable>(serializedObj.GetProp("Exceptions"));
                var targetException = Assert.Single(exceptions);
                Assert.NotNull(targetException.GetProp("HResult"));
            }

            [Fact]
            public void WithOneExceptionEntry_ExceptionSourceSerialized()
            {
                var exception = new Exception();
                var entry = new LogEntry(LogSeverity.Info, "foobar")
                {
                    Exception = exception
                };

                var formatted = sut.Format(entry);

                var exceptions = Assert.IsAssignableFrom<IEnumerable>(serializedObj.GetProp("Exceptions"));
                var targetException = Assert.Single(exceptions);
                Assert.Contains(
                    "Source",
                    targetException.GetType().GetProperties().Select(p => p.Name));
            }

            [Fact]
            public void WithOneExceptionEntry_ExceptionStackTraceSerialized()
            {
                var exception = new Exception();
                var entry = new LogEntry(LogSeverity.Info, "foobar")
                {
                    Exception = exception
                };

                var formatted = sut.Format(entry);

                var exceptions = Assert.IsAssignableFrom<IEnumerable>(serializedObj.GetProp("Exceptions"));
                var targetException = Assert.Single(exceptions);
                Assert.Contains(
                    "StackTrace",
                    targetException.GetType().GetProperties().Select(p => p.Name));
            }

            [Fact]
            public void WithInnerExceptionEntry_ExceptionSerialized()
            {
                var inner = new Exception("bar");
                var exception = new Exception("foo", inner);
                var entry = new LogEntry(LogSeverity.Info, "foobar")
                {
                    Exception = exception
                };

                var formatted = sut.Format(entry);

                var exceptions = Assert.IsAssignableFrom<IEnumerable>(serializedObj.GetProp("Exceptions"));
                Assert.Collection(exceptions.Cast<object>(),
                    outer => Assert.Equal("foo", outer.GetProp("Message")),
                    next => Assert.Equal("bar", next.GetProp("Message")));
            }

            [Fact]
            public void WithEntry_LoggedSerialized()
            {
                var entry = new LogEntry(LogSeverity.Info, "foobar");

                var formatted = sut.Format(entry);

                Assert.Equal(entry.Logged, serializedObj.GetProp("Logged"));
            }

            [Fact]
            public void WithEntry_ThreadIdSerialized()
            {
                var entry = new LogEntry(LogSeverity.Info, "foobar");

                var formatted = sut.Format(entry);

                Assert.NotNull(serializedObj.GetProp("ManagedThreadId"));
            }

            [Fact]
            public void WithMultipleEntries_SeriazliedOnce()
            {
                var entry = new LogEntry(LogSeverity.Info, "foo")
                {
                    Exception = new Exception(),
                    Data = new {}
                };

                sut.Format(entry);
                sut.Format(entry);

                A.CallTo(serializer)
                    .Where(ctx => ctx.Method.Name == "Serialize")
                    .MustHaveHappenedOnceExactly();
            }

            [Fact]
            public void SerializationFails_LogEntryToStringLogged()
            {
                var entry = new LogEntry(LogSeverity.Info, "foo");
                A.CallTo(serializer)
                    .Where(ctx => ctx.Method.Name == "Serialize")
                    .Throws<Exception>();

                var formatted = sut.Format(entry);

                Assert.Contains(entry.ToString(), formatted);
            }

            [Fact]
            public void SerializationFails_SerializationErrorLogged()
            {
                var ex = new Exception("lorem ipsum dolar");
                var entry = new LogEntry(LogSeverity.Info, "foo");
                A.CallTo(serializer)
                    .Where(ctx => ctx.Method.Name == "Serialize")
                    .Throws(ex);

                var formatted = sut.Format(entry);

                Assert.Contains("lorem ipsum dolar", formatted);
            }
        }
    }
}
