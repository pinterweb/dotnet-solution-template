using FakeItEasy;
using Microsoft.AspNetCore.Http;
using BusinessApp.Kernel;
using Xunit;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace BusinessApp.WebApi.UnitTest
{
    public class HttpRequestLoggingDecoratorTest
    {
        private readonly IHttpRequestHandler inner;
        private readonly ILogger logger;
        private readonly HttpRequestLoggingDecorator sut;

        public HttpRequestLoggingDecoratorTest()
        {
            logger = A.Fake<ILogger>();
            inner = A.Fake<IHttpRequestHandler>();
            sut = new HttpRequestLoggingDecorator(inner, logger);
        }

        public class Constructor : HttpRequestLoggingDecoratorTest
        {
            public static IEnumerable<object[]> InvalidCtorArgs => new[]
            {
                new object[] { A.Dummy<IHttpRequestHandler>(), null, },
                new object[] { null, A.Dummy<ILogger>() },
            };

            [Theory, MemberData(nameof(InvalidCtorArgs))]
            public void InvalidCtorArgs_ExceptionThrown(IHttpRequestHandler i, ILogger l)
            {
                /* Arrange */
                void shouldThrow() => new HttpRequestLoggingDecorator(i, l);

                /* Act */
                var ex = Record.Exception(shouldThrow);

                /* Assert */
                Assert.IsType<BusinessAppException>(ex);
            }
        }

        public class HandleAsync : HttpRequestLoggingDecoratorTest
        {
            private readonly HttpContext context;

            public HandleAsync()
            {
                context = A.Dummy<HttpContext>();
            }

            public static IEnumerable<object[]> Exceptions => new[]
            {
                new object[] { new Exception("foobar") },
                new object[] { new ArgumentException("foobar", new FormatException("b")) },
            };

            [Fact]
            public async Task NoException_NotLogged()
            {
                /* Act */
                await sut.HandleAsync<RequestStub, ResponseStub>(context);

                /* Assert */
                A.CallTo(() => logger.Log(A<LogEntry>._)).MustNotHaveHappened();
            }

            [Theory, MemberData(nameof(Exceptions))]
            public async Task Exception_LogsItOnce(Exception e)
            {
                /* Arrange */
                A.CallTo(() => inner.HandleAsync<RequestStub, ResponseStub>(context)).Throws(e);

                /* Act */
                var ex = await Record.ExceptionAsync(() =>
                    sut.HandleAsync<RequestStub, ResponseStub>(context));

                /* Assert */
                A.CallTo(() => logger.Log(A<LogEntry>._)).MustHaveHappenedOnceExactly();
            }

            [Theory, MemberData(nameof(Exceptions))]
            public async Task Exception_LogsSeverity(Exception e)
            {
                /* Arrange */
                LogEntry entry = null;
                A.CallTo(() => inner.HandleAsync<RequestStub, ResponseStub>(context)).Throws(e);
                A.CallTo(() => logger.Log(A<LogEntry>._))
                    .Invokes(ctx => entry = ctx.GetArgument<LogEntry>(0));

                /* Act */
                var ex = await Record.ExceptionAsync(() =>
                    sut.HandleAsync<RequestStub, ResponseStub>(context));

                /* Assert */
                Assert.Equal(LogSeverity.Error, entry.Severity);
            }

            [Theory, MemberData(nameof(Exceptions))]
            public async Task Exception_LogsExceptionMessage(Exception exception)
            {
                /* Arrange */
                LogEntry entry = null;
                A.CallTo(() => inner.HandleAsync<RequestStub, ResponseStub>(context))
                    .Throws(exception);
                A.CallTo(() => logger.Log(A<LogEntry>._))
                    .Invokes(ctx => entry = ctx.GetArgument<LogEntry>(0));

                /* Act */
                var ex = await Record.ExceptionAsync(() =>
                    sut.HandleAsync<RequestStub, ResponseStub>(context));

                /* Assert */
                Assert.Equal("foobar", exception.Message);
            }

            [Theory, MemberData(nameof(Exceptions))]
            public async Task Exception_LogsException(Exception exception)
            {
                /* Arrange */
                LogEntry entry = null;
                A.CallTo(() => inner.HandleAsync<RequestStub, ResponseStub>(context))
                    .Throws(exception);
                A.CallTo(() => logger.Log(A<LogEntry>._))
                    .Invokes(ctx => entry = ctx.GetArgument<LogEntry>(0));

                /* Act */
                var ex = await Record.ExceptionAsync(() =>
                    sut.HandleAsync<RequestStub, ResponseStub>(context));

                /* Assert */
                Assert.Same(exception, entry.Exception);
            }

            [Fact]
            public async Task Exception_Rethrows()
            {
                /* Arrange */
                var exception = new Exception();
                A.CallTo(() => inner.HandleAsync<RequestStub, ResponseStub>(context))
                    .Throws(exception);

                /* Act */
                var thrownException = await Record.ExceptionAsync(() =>
                    sut.HandleAsync<RequestStub, ResponseStub>(context));

                /* Assert */
                Assert.Same(exception, thrownException);
            }
        }
    }
}
