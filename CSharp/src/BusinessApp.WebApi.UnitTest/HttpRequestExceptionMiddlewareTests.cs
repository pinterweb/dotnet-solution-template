namespace BusinessApp.WebApi.UnitTest
{
    using FakeItEasy;
    using Microsoft.AspNetCore.Http;
    using BusinessApp.Domain;
    using Xunit;
    using System;
    using System.Threading.Tasks;

    public class HttpRequestExceptionMiddlewareTests
    {
        private readonly ILogger logger;
        public readonly HttpRequestExceptionMiddleware sut;

        public HttpRequestExceptionMiddlewareTests()
        {
            logger = A.Fake<ILogger>();
            sut = new HttpRequestExceptionMiddleware(logger);
        }

        public class InvokeAsync : HttpRequestExceptionMiddlewareTests
        {
            [Fact]
            public async Task NoException_CalledNext()
            {
                /* Arrange */
                var context = A.Dummy<HttpContext>();
                var next = A.Fake<RequestDelegate>();

                /* Act */
                await sut.InvokeAsync(context, next);

                /* Assert */
                A.CallTo(() => next.Invoke(context)).MustHaveHappenedOnceExactly();
            }

            [Fact]
            public async Task NoException_DoesNotLog()
            {
                /* Arrange */
                var context = A.Dummy<HttpContext>();
                var next = A.Dummy<RequestDelegate>();

                /* Act */
                await sut.InvokeAsync(context, next);

                /* Assert */
                A.CallTo(() => logger.Log(A<LogEntry>._)).MustNotHaveHappened();
            }

            [Fact]
            public async Task Exception_LogsIt()
            {
                /* Arrange */
                var context = A.Dummy<HttpContext>();
                var next = A.Fake<RequestDelegate>();
                A.CallTo(() => next.Invoke(context)).Throws<Exception>();

                /* Act */
                await sut.InvokeAsync(context, next);

                /* Assert */
                A.CallTo(() => logger.Log(A<LogEntry>._)).MustHaveHappened();
            }

            [Fact]
            public async Task Exception_LogsError()
            {
                /* Arrange */
                var context = A.Dummy<HttpContext>();
                var next = A.Dummy<RequestDelegate>();
                LogEntry entry = null;
                A.CallTo(() => next.Invoke(context)).Throws<Exception>();
                A.CallTo(() => logger.Log(A<LogEntry>._))
                    .Invokes(ctx => entry = ctx.GetArgument<LogEntry>(0));

                /* Act */
                await sut.InvokeAsync(context, next);

                /* Assert */
                Assert.Equal(LogSeverity.Error, entry.Severity);
            }

            [Fact]
            public async Task Exception_LogsExceptionMessage()
            {
                /* Arrange */
                var context = A.Dummy<HttpContext>();
                var next = A.Dummy<RequestDelegate>();
                var exception = new Exception("foobar");
                LogEntry entry = null;
                A.CallTo(() => next.Invoke(context)).Throws(exception);
                A.CallTo(() => logger.Log(A<LogEntry>._))
                    .Invokes(ctx => entry = ctx.GetArgument<LogEntry>(0));

                /* Act */
                await sut.InvokeAsync(context, next);

                /* Assert */
                Assert.Equal("foobar", exception.Message);
            }

            [Fact]
            public async Task Exception_LogsException()
            {
                /* Arrange */
                var context = A.Dummy<HttpContext>();
                var next = A.Dummy<RequestDelegate>();
                var exception = new Exception("foobar");
                LogEntry entry = null;
                A.CallTo(() => next.Invoke(context)).Throws(exception);
                A.CallTo(() => logger.Log(A<LogEntry>._))
                    .Invokes(ctx => entry = ctx.GetArgument<LogEntry>(0));

                /* Act */
                await sut.InvokeAsync(context, next);

                /* Assert */
                Assert.Same(exception, entry.Exception);
            }
        }
    }
}
