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
    using BusinessApp.WebApi.Json;
    using Newtonsoft.Json;

    // TODO wrong name
    public class NewtonsoftJsonExceptionHandlerTests
    {
        private readonly IHttpRequestHandler<RequestStub, ResponseStub> inner;
        private readonly ILogger logger;
        private readonly NewtonsoftJsonExceptionDecorator<RequestStub, ResponseStub> sut;

        public NewtonsoftJsonExceptionHandlerTests()
        {
            logger = A.Fake<ILogger>();
            inner = A.Fake<IHttpRequestHandler<RequestStub, ResponseStub>>();
            sut = new NewtonsoftJsonExceptionDecorator<RequestStub, ResponseStub>(inner, logger);
        }

        public class Constructor : NewtonsoftJsonExceptionHandlerTests
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
                    new NewtonsoftJsonExceptionDecorator<RequestStub, ResponseStub>(i, l);

                /* Act */
                var ex = Record.Exception(shouldThrow);

                /* Assert */
                Assert.IsType<BusinessAppException>(ex);
            }
        }

        public class HandleAsync : NewtonsoftJsonExceptionHandlerTests
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
            public async Task GeneralException_NotLogged()
            {
                /* Arrange */
                A.CallTo(() => inner.HandleAsync(context, cancelToken))
                    .Throws<Exception>();

                /* Act */
                var ex = await Record.ExceptionAsync(() => sut.HandleAsync(context, cancelToken));

                /* Assert */
                A.CallTo(() => logger.Log(A<LogEntry>._)).MustNotHaveHappened();
            }


            [Fact]
            public async Task JsonSerializationException_LogsItOnce()
            {
                /* Arrange */
                A.CallTo(() => inner.HandleAsync(context, cancelToken))
                    .Throws<JsonSerializationException>();

                /* Act */
                var _ = await sut.HandleAsync(context, cancelToken);

                /* Assert */
                A.CallTo(() => logger.Log(A<LogEntry>._)).MustHaveHappened();
            }

            [Fact]
            public async Task JsonSerializationException_LogsSeverity()
            {
                /* Arrange */
                LogEntry entry = null;
                var e = new JsonSerializationException();
                A.CallTo(() => inner.HandleAsync(context, cancelToken)).Throws(e);
                A.CallTo(() => logger.Log(A<LogEntry>._))
                    .Invokes(ctx => entry = ctx.GetArgument<LogEntry>(0));

                /* Act */
                var _ = await sut.HandleAsync(context, cancelToken);

                /* Assert */
                Assert.Equal(LogSeverity.Error, entry.Severity);
            }

            [Fact]
            public async Task JsonSerializationException_LogsExceptionMessage()
            {
                /* Arrange */
                var exception = new JsonSerializationException("foobar");
                LogEntry entry = null;
                A.CallTo(() => inner.HandleAsync(context, cancelToken)).Throws(exception);
                A.CallTo(() => logger.Log(A<LogEntry>._))
                    .Invokes(ctx => entry = ctx.GetArgument<LogEntry>(0));

                /* Act */
                var _ = await sut.HandleAsync(context, cancelToken);

                /* Assert */
                Assert.Equal("foobar", exception.Message);
            }

            [Fact]
            public async Task JsonSerializationException_LogsException()
            {
                /* Arrange */
                var exception = new JsonSerializationException();
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
            public async Task JsonSerializationException_ReturnsErrorResult()
            {
                /* Arrange */
                var expectedMsg = "Your request could not be read because " +
                    "your payload is in an invalid format. Please review your data " +
                    "and try again";
                A.CallTo(() => inner.HandleAsync(context, cancelToken))
                    .Throws<JsonSerializationException>();

                /* Act */
                var result = await sut.HandleAsync(context, cancelToken);

                /* Assert */
                var error = Assert.IsType<BadStateException>(result.UnwrapError());
                Assert.Equal(expectedMsg, error.Message);
            }
        }
    }
}
