namespace BusinessApp.App.UnitTest
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using FakeItEasy;
    using Xunit;
    using System.Threading;
    using BusinessApp.Domain;
    using System;

    public class RequestExceptionDecoratorTests
    {
        private readonly CancellationToken cancelToken;
        private readonly RequestExceptionDecorator<QueryStub, ResponseStub> sut;
        private readonly IRequestHandler<QueryStub, ResponseStub> inner;
        private readonly ILogger logger;
        private readonly QueryStub request;

        public RequestExceptionDecoratorTests()
        {
            cancelToken = A.Dummy<CancellationToken>();
            inner = A.Fake<IRequestHandler<QueryStub, ResponseStub>>();
            logger = A.Fake<ILogger>();
            request = A.Dummy<QueryStub>();

            sut = new RequestExceptionDecorator<QueryStub, ResponseStub>(inner, logger);
        }

        public class Constructor : RequestExceptionDecoratorTests
        {
            public static IEnumerable<object[]> InvalidCtorArgs => new[]
            {
                new object[]
                {
                    null,
                    A.Dummy<ILogger>(),
                },
                new object[]
                {
                    A.Dummy<IRequestHandler<QueryStub, ResponseStub>>(),
                    null
                }

            };

            [Theory, MemberData(nameof(InvalidCtorArgs))]
            public void InvalidCtorArgs_ExceptionThrown(
                IRequestHandler<QueryStub, ResponseStub> q, ILogger l)
            {
                /* Arrange */
                void shouldThrow() => new RequestExceptionDecorator<QueryStub, ResponseStub>(q, l);

                /* Act */
                var ex = Record.Exception(shouldThrow);

                /* Assert */
                Assert.IsType<BadStateException>(ex);
            }
        }

        public class HandleAsync : RequestExceptionDecoratorTests
        {
            [Fact]
            public async Task ExceptionCaught_CriticalErrorLogged()
            {
                /* Arrange */
                LogEntry entry = null;
                A.CallTo(() => inner.HandleAsync(A<QueryStub>._, A<CancellationToken>._))
                    .Throws<Exception>();
                A.CallTo(() => logger.Log(A<LogEntry>._))
                    .Invokes(ctx => entry = ctx.GetArgument<LogEntry>(0));

                /* Act */
                var _ = await sut.HandleAsync(request, cancelToken);

                /* Assert */
                Assert.Equal(LogSeverity.Critical, entry.Severity);
            }

            [Fact]
            public async Task ExceptionCaught_ExceptionMessageLogged()
            {
                /* Arrange */
                var ex = new Exception("thrown");
                LogEntry entry = null;
                A.CallTo(() => inner.HandleAsync(A<QueryStub>._, A<CancellationToken>._))
                    .Throws(ex);
                A.CallTo(() => logger.Log(A<LogEntry>._))
                    .Invokes(ctx => entry = ctx.GetArgument<LogEntry>(0));

                /* Act */
                var _ = await sut.HandleAsync(request, cancelToken);

                /* Assert */
                Assert.Equal("thrown", entry.Message);
            }

            [Fact]
            public async Task ExceptionCaught_ExceptionLogged()
            {
                /* Arrange */
                var ex = new Exception();
                LogEntry entry = null;
                A.CallTo(() => inner.HandleAsync(A<QueryStub>._, A<CancellationToken>._))
                    .Throws(ex);
                A.CallTo(() => logger.Log(A<LogEntry>._))
                    .Invokes(ctx => entry = ctx.GetArgument<LogEntry>(0));

                /* Act */
                var _ = await sut.HandleAsync(request, cancelToken);

                /* Assert */
                Assert.Same(ex, entry.Exception);
            }

            [Fact]
            public async Task ExceptionCaught_RequestLogged()
            {
                /* Arrange */
                LogEntry entry = null;
                A.CallTo(() => inner.HandleAsync(A<QueryStub>._, A<CancellationToken>._))
                    .Throws<Exception>();
                A.CallTo(() => logger.Log(A<LogEntry>._))
                    .Invokes(ctx => entry = ctx.GetArgument<LogEntry>(0));

                /* Act */
                var _ = await sut.HandleAsync(request, cancelToken);

                /* Assert */
                Assert.Same(request, entry.Data);
            }

            [Fact]
            public async Task ExceptionCaught_ErrorTypeReturned()
            {
                /* Arrange */
                var error = new BadStateException("foo");
                A.CallTo(() => inner.HandleAsync(A<QueryStub>._, A<CancellationToken>._))
                    .Throws(error);

                /* Act */
                var result = await sut.HandleAsync(request, cancelToken);

                /* Assert */
                Assert.Equal(Result.Error<ResponseStub>(error), result);
            }

            [Fact]
            public async Task NoException_InnerResultReturned()
            {
                /* Arrange */
                var innerResult = Result.Ok<ResponseStub>(new ResponseStub());
                A.CallTo(() => inner.HandleAsync(A<QueryStub>._, A<CancellationToken>._))
                    .Returns(innerResult);

                /* Act */
                var result = await sut.HandleAsync(request, cancelToken);

                /* Assert */
                Assert.Equal(innerResult, result);
            }
        }
    }
}
