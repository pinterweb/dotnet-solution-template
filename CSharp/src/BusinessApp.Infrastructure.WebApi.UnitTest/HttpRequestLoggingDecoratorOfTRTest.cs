using FakeItEasy;
using Microsoft.AspNetCore.Http;
using BusinessApp.Kernel;
using Xunit;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Threading;
using BusinessApp.Infrastructure;

namespace BusinessApp.Infrastructure.WebApi.UnitTest
{
    public class HttpRequestLoggingDecoratorOfTRTest
    {
        private readonly IHttpRequestHandler<RequestStub, ResponseStub> inner;
        private readonly ILogger logger;
        private readonly HttpRequestLoggingDecorator<RequestStub, ResponseStub> sut;

        public HttpRequestLoggingDecoratorOfTRTest()
        {
            logger = A.Fake<ILogger>();
            inner = A.Fake<IHttpRequestHandler<RequestStub, ResponseStub>>();
            sut = new HttpRequestLoggingDecorator<RequestStub, ResponseStub>(inner, logger);
        }

        public class Constructor : HttpRequestLoggingDecoratorOfTRTest
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
                Assert.IsType<BusinessAppException>(ex);
            }
        }

        public class HandleAsync : HttpRequestLoggingDecoratorOfTRTest
        {
            private readonly CancellationToken cancelToken;
            private readonly HttpContext context;

            public HandleAsync()
            {
                context = A.Dummy<HttpContext>();
                cancelToken = A.Dummy<CancellationToken>();
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
                await sut.HandleAsync(context, cancelToken);

                /* Assert */
                A.CallTo(() => logger.Log(A<LogEntry>._)).MustNotHaveHappened();
            }

            [Theory, MemberData(nameof(Exceptions))]
            public async Task Exception_LogsItOnce(Exception e)
            {
                /* Arrange */
                A.CallTo(() => inner.HandleAsync(context, cancelToken)).Throws(e);

                /* Act */
                var _ = await sut.HandleAsync(context, cancelToken);

                /* Assert */
                A.CallTo(() => logger.Log(A<LogEntry>._)).MustHaveHappenedOnceExactly();
            }

            [Theory, MemberData(nameof(Exceptions))]
            public async Task Exception_LogsSeverity(Exception e)
            {
                /* Arrange */
                LogEntry entry = null;
                A.CallTo(() => inner.HandleAsync(context, cancelToken)).Throws(e);
                A.CallTo(() => logger.Log(A<LogEntry>._))
                    .Invokes(ctx => entry = ctx.GetArgument<LogEntry>(0));

                /* Act */
                var _ = await sut.HandleAsync(context, cancelToken);

                /* Assert */
                Assert.Equal(LogSeverity.Error, entry.Severity);
            }

            [Theory, MemberData(nameof(Exceptions))]
            public async Task Exception_LogsExceptionMessage(Exception exception)
            {
                /* Arrange */
                LogEntry entry = null;
                A.CallTo(() => inner.HandleAsync(context, cancelToken)).Throws(exception);
                A.CallTo(() => logger.Log(A<LogEntry>._))
                    .Invokes(ctx => entry = ctx.GetArgument<LogEntry>(0));

                /* Act */
                var _ = await sut.HandleAsync(context, cancelToken);

                /* Assert */
                Assert.Equal("foobar", exception.Message);
            }

            [Theory, MemberData(nameof(Exceptions))]
            public async Task Exception_LogsException(Exception exception)
            {
                /* Arrange */
                LogEntry entry = null;
                A.CallTo(() => inner.HandleAsync(context, cancelToken)).Throws(exception);
                A.CallTo(() => logger.Log(A<LogEntry>._))
                    .Invokes(ctx => entry = ctx.GetArgument<LogEntry>(0));

                /* Act */
                var _ = await sut.HandleAsync(context, cancelToken);

                /* Assert */
                Assert.Same(exception, entry.Exception);
            }

            [Fact]
            public async Task UnknownException_ReturnsErrorResult()
            {
                /* Arrange */
                var expectedMsg = "An unknown error occurred while processing your " +
                    "request. Please try again or contact support if this continues";
                A.CallTo(() => inner.HandleAsync(context, cancelToken)).Throws<Exception>();

                /* Act */
                var result = await sut.HandleAsync(context, cancelToken);

                /* Assert */
                var error = Assert.IsType<BusinessAppException>(result.UnwrapError());
                Assert.Equal(expectedMsg, error.Message);
            }

            [Fact]
            public async Task FormatException_ReturnsErrorResult()
            {
                /* Arrange */
                var expectedMsg = "Your request could not be read because some " +
                    "arguments may be in the wrong format. Please review your request " +
                    "and try again";
                var formatError = new FormatException();
                var exception = new ArgumentException("foo", formatError);
                A.CallTo(() => inner.HandleAsync(context, cancelToken)).Throws(exception);

                /* Act */
                var result = await sut.HandleAsync(context, cancelToken);

                /* Assert */
                var error = Assert.IsType<BadStateException>(result.UnwrapError());
                Assert.Equal(expectedMsg, error.Message);
            }
        }
    }
}
