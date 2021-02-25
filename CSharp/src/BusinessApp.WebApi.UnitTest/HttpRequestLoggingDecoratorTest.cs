namespace BusinessApp.WebApi.UnitTest
{
    using FakeItEasy;
    using Microsoft.AspNetCore.Http;
    using BusinessApp.Domain;
    using Xunit;
    using System;
    using System.Threading.Tasks;
    using System.Collections.Generic;
    using System.Threading;

    public class HttpRequestLoggingHandlerTests
    {
        private readonly IHttpRequestHandler<RequestStub, ResponseStub> inner;
        private readonly ILogger logger;
        private readonly HttpRequestLoggingDecorator<RequestStub, ResponseStub> sut;

        public HttpRequestLoggingHandlerTests()
        {
            logger = A.Fake<ILogger>();
            inner = A.Fake<IHttpRequestHandler<RequestStub, ResponseStub>>();
            sut = new HttpRequestLoggingDecorator<RequestStub, ResponseStub>(inner, logger);
        }

        public class Constructor : HttpRequestLoggingHandlerTests
        {
            public static IEnumerable<object[]> InvalidCtorArgs => new[]
            {
                new object[]
                {
                    A.Dummy<IHttpRequestHandler<RequestStub, ResponseStub>>(),
                    null,
                },
                new object[] { null, A.Dummy<ILogger>() },
            };

            [Theory, MemberData(nameof(InvalidCtorArgs))]
            public void InvalidCtorArgs_ExceptionThrown(
                IHttpRequestHandler<RequestStub, ResponseStub> i,
                ILogger l)
            {
                /* Arrange */
                void shouldThrow() =>
                    new HttpRequestLoggingDecorator<RequestStub, ResponseStub>(i, l);

                /* Act */
                var ex = Record.Exception(shouldThrow);

                /* Assert */
                Assert.NotNull(ex);
            }
        }

        public class HandleAsync : HttpRequestLoggingHandlerTests
        {
            private readonly CancellationToken cancelToken;
            private readonly HttpContext context;

            public HandleAsync()
            {
                context = A.Dummy<HttpContext>();
                cancelToken = A.Dummy<CancellationToken>();
            }

            [Fact]
            public async Task NoException_NotLogged()
            {
                /* Act */
                await sut.HandleAsync(context, cancelToken);

                /* Assert */
                A.CallTo(() => logger.Log(A<LogEntry>._)).MustNotHaveHappened();
            }

            [Fact]
            public async Task Exception_LogsItOnce()
            {
                /* Arrange */
                var context = A.Dummy<HttpContext>();
                A.CallTo(() => inner.HandleAsync(context, cancelToken)).Throws<Exception>();

                /* Act */
                var ex = await Record.ExceptionAsync(() => sut.HandleAsync(context, cancelToken));

                /* Assert */
                A.CallTo(() => logger.Log(A<LogEntry>._)).MustHaveHappened();
            }

            [Fact]
            public async Task Exception_LogsSeverity()
            {
                /* Arrange */
                var context = A.Dummy<HttpContext>();
                LogEntry entry = null;
                A.CallTo(() => inner.HandleAsync(context, cancelToken)).Throws<Exception>();
                A.CallTo(() => logger.Log(A<LogEntry>._))
                    .Invokes(ctx => entry = ctx.GetArgument<LogEntry>(0));

                /* Act */
                var ex = await Record.ExceptionAsync(() => sut.HandleAsync(context, cancelToken));

                /* Assert */
                Assert.Equal(LogSeverity.Error, entry.Severity);
            }

            [Fact]
            public async Task Exception_LogsExceptionMessage()
            {
                /* Arrange */
                var context = A.Dummy<HttpContext>();
                var exception = new Exception("foobar");
                LogEntry entry = null;
                A.CallTo(() => inner.HandleAsync(context, cancelToken)).Throws(exception);
                A.CallTo(() => logger.Log(A<LogEntry>._))
                    .Invokes(ctx => entry = ctx.GetArgument<LogEntry>(0));

                /* Act */
                var ex = await Record.ExceptionAsync(() => sut.HandleAsync(context, cancelToken));

                /* Assert */
                Assert.Equal("foobar", exception.Message);
            }

            [Fact]
            public async Task Exception_LogsException()
            {
                /* Arrange */
                var context = A.Dummy<HttpContext>();
                var exception = new Exception("foobar");
                LogEntry entry = null;
                A.CallTo(() => inner.HandleAsync(context, cancelToken)).Throws(exception);
                A.CallTo(() => logger.Log(A<LogEntry>._))
                    .Invokes(ctx => entry = ctx.GetArgument<LogEntry>(0));

                /* Act */
                var ex = await Record.ExceptionAsync(() => sut.HandleAsync(context, cancelToken));

                /* Assert */
                Assert.Same(exception, entry.Exception);
            }

            [Fact]
            public async Task Exception_RethrowsOriginalException()
            {
                /* Arrange */
                var context = A.Dummy<HttpContext>();
                var exception = new Exception();
                A.CallTo(() => inner.HandleAsync(context, cancelToken)).Throws(exception);

                /* Act */
                var ex = await Record.ExceptionAsync(() => sut.HandleAsync(context, cancelToken));

                /* Assert */
                Assert.Same(exception, ex);
            }
        }
    }
}
