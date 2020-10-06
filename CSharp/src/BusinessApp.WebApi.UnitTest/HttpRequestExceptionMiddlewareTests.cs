namespace BusinessApp.WebApi.UnitTest
{
    using FakeItEasy;
    using Microsoft.AspNetCore.Http;
    using BusinessApp.Domain;
    using Xunit;
    using System;
    using System.Threading.Tasks;
    using System.Collections.Generic;
    using BusinessApp.App;
    using BusinessApp.WebApi.ProblemDetails;

    public class HttpRequestExceptionMiddlewareTests
    {
        private readonly ILogger logger;
        private readonly IResponseWriter writer;
        public readonly HttpRequestExceptionMiddleware sut;

        public HttpRequestExceptionMiddlewareTests()
        {
            logger = A.Fake<ILogger>();
            writer = A.Fake<IResponseWriter>();
            sut = new HttpRequestExceptionMiddleware(logger, writer);
        }

        public class Constructor : HttpRequestExceptionMiddlewareTests
        {
            public static IEnumerable<object[]> InvalidCtorArgs => new[]
            {
                new object[] { null, A.Dummy<IResponseWriter>() },
                new object[] { A.Dummy<ILogger>(), null },
            };

            [Theory, MemberData(nameof(InvalidCtorArgs))]
            public void InvalidCtorArgs_ExceptionThrown(ILogger l, IResponseWriter w)
            {
                /* Arrange */
                void shouldThrow() => new HttpRequestExceptionMiddleware(l, w);

                /* Act */
                var ex = Record.Exception(shouldThrow);

                /* Assert */
                Assert.NotNull(ex);
            }
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

            [Fact]
            public async Task ResponseNotStarted_SetsStatusTo500()
            {
                /* Arrange */
                var context = A.Fake<HttpContext>();
                var next = A.Dummy<RequestDelegate>();
                A.CallTo(() => context.Response.HasStarted).Returns(false);
                A.CallTo(() => next.Invoke(context)).Throws<Exception>();

                /* Act */
                await sut.InvokeAsync(context, next);

                /* Assert */
                A.CallToSet(() => context.Response.StatusCode).To(500)
                    .MustHaveHappenedOnceExactly();
            }

            [Fact]
            public async Task ResponseNotStarted_WritesResponse()
            {
                /* Arrange */
                var context = A.Dummy<HttpContext>();
                var next = A.Dummy<RequestDelegate>();
                A.CallTo(() => context.Response.HasStarted).Returns(false);
                A.CallTo(() => next.Invoke(context)).Throws<Exception>();

                /* Act */
                await sut.InvokeAsync(context, next);

                /* Assert */
                A.CallTo(() => writer.WriteResponseAsync(context))
                    .MustHaveHappenedOnceExactly();
            }
        }
    }
}
