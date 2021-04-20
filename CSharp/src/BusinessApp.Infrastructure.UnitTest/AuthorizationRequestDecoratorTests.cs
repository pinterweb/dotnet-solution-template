using System;
using System.Collections.Generic;
using System.Security;
using System.Security.Principal;
using System.Threading;
using System.Threading.Tasks;
using BusinessApp.Kernel;
using FakeItEasy;
using Xunit;

namespace BusinessApp.Infrastructure.UnitTest
{
    public class AuthorizationRequestDecoratorTests
    {
        private readonly AuthorizationRequestDecorator<QueryStub, ResponseStub> sut;
        private readonly IRequestHandler<QueryStub, ResponseStub> decorated;
        private readonly IAuthorizer<QueryStub> authorizer;
        private readonly IPrincipal user;
        private readonly ILogger logger;

        public AuthorizationRequestDecoratorTests()
        {
            decorated = A.Fake<IRequestHandler<QueryStub, ResponseStub>>();
            authorizer = A.Fake<IAuthorizer<QueryStub>>();
            user = A.Fake<IPrincipal>();
            logger = A.Fake<ILogger>();

            sut = new AuthorizationRequestDecorator<QueryStub, ResponseStub>(decorated,
                authorizer, user, logger);
        }

        public class Constructor : AuthorizationRequestDecoratorTests
        {
            public static IEnumerable<object[]> InvalidCtorArgs
            {
                get
                {
                    return new []
                    {
                        new object[]
                        {
                            null,
                            A.Dummy<IAuthorizer<QueryStub>>(),
                            A.Dummy<IPrincipal>(),
                            A.Dummy<ILogger>(),
                        },
                        new object[]
                        {
                            A.Dummy<IRequestHandler<QueryStub, ResponseStub>>(),
                            null,
                            A.Dummy<IPrincipal>(),
                            A.Dummy<ILogger>(),
                        },
                        new object[]
                        {
                            A.Dummy<IRequestHandler<QueryStub, ResponseStub>>(),
                            A.Dummy<IAuthorizer<QueryStub>>(),
                            null,
                            A.Dummy<ILogger>(),
                        },
                        new object[]
                        {
                            A.Dummy<IRequestHandler<QueryStub, ResponseStub>>(),
                            A.Dummy<IAuthorizer<QueryStub>>(),
                            A.Dummy<IPrincipal>(),
                            null
                        }
                    };
                }
            }

            [Theory, MemberData(nameof(InvalidCtorArgs))]
            public void WithAnyNullService_ExceptionThrown(
                IRequestHandler<QueryStub, ResponseStub> d, IAuthorizer<QueryStub> a,
                IPrincipal p, ILogger l)
            {
                /* Arrange */
                Action create = () => new AuthorizationRequestDecorator<QueryStub, ResponseStub>(
                    d, a, p, l);

                /* Act */
                var exception = Record.Exception(create);

                /* Assert */
                Assert.IsType<BusinessAppException>(exception);
            }
        }

        public class HandleAsync : AuthorizationRequestDecoratorTests
        {
            private readonly QueryStub query;
            private readonly CancellationToken cancelToken;

            public HandleAsync()
            {
                query = new QueryStub();
                cancelToken = A.Dummy<CancellationToken>();
            }

            [Fact]
            public async Task NotAuthorized_HandlerNotCalled()
            {
                /* Arrange */
                A.CallTo(() => authorizer.AuthorizeObject(query)).Returns(false);

                /* Act */
                var _ = await sut.HandleAsync(query, cancelToken);

                /* Assert */
                A.CallTo(() => decorated.HandleAsync(A<QueryStub>._, A<CancellationToken>._))
                    .MustNotHaveHappened();
            }

            [Fact]
            public async Task NotAuthorized_SecurityExceptionReturns()
            {
                /* Arrange */
                A.CallTo(() => authorizer.AuthorizeObject(query)).Returns(false);

                /* Act */
                var result = await sut.HandleAsync(query, cancelToken);

                /* Assert */
                Assert.IsType<SecurityException>(result.UnwrapError());
            }

            [Fact]
            public async Task NotAuthorized_MessageInException()
            {
                /* Arrange */
                A.CallTo(() => authorizer.AuthorizeObject(query)).Returns(false);

                /* Act */
                var result = await sut.HandleAsync(query, cancelToken);

                /* Assert */
                Assert.Equal("You are not authorized to execute this request",
                    result.UnwrapError().Message);
            }

            [Fact]
            public async Task NotAuthorized_LogsInfoEntry()
            {
                /* Arrange */
                LogEntry logEntry = null;
                A.CallTo(() => authorizer.AuthorizeObject(query)).Returns(false);
                A.CallTo(() => logger.Log(A<LogEntry>._))
                    .Invokes(ctx => logEntry = ctx.GetArgument<LogEntry>(0));
                A.CallTo(() => authorizer.AuthorizeObject(query)).Returns(false);

                /* Act */
                var result = await sut.HandleAsync(query, cancelToken);

                /* Assert */
                Assert.Equal(LogSeverity.Info, logEntry.Severity);
            }

            [Theory]
            [InlineData(null, "Anonymous")]
            [InlineData("foouser", "foouser")]
            public async Task NotAuthorized_LogsUser(string identityName, string usedName)
            {
                /* Arrange */
                var identity = A.Fake<IIdentity>();
                LogEntry logEntry = null;
                A.CallTo(() => logger.Log(A<LogEntry>._))
                    .Invokes(ctx => logEntry = ctx.GetArgument<LogEntry>(0));
                A.CallTo(() => authorizer.AuthorizeObject(query)).Returns(false);
                A.CallTo(() => user.Identity).Returns(identity);
                A.CallTo(() => identity.Name).Returns(identityName);

                /* Act */
                var _ = await sut.HandleAsync(query, cancelToken);

                /* Assert */
                Assert.Equal(
                    $"'{usedName}' is not authorized to execute BusinessApp.Infrastructure.UnitTest.QueryStub",
                    logEntry.Message
                );
            }

            [Fact]
            public async Task NotAuthorized_NoIdentity_LogsAnonymousUser()
            {
                /* Arrange */
                var identity = A.Fake<IIdentity>();
                LogEntry logEntry = null;
                A.CallTo(() => logger.Log(A<LogEntry>._))
                    .Invokes(ctx => logEntry = ctx.GetArgument<LogEntry>(0));
                A.CallTo(() => authorizer.AuthorizeObject(query)).Returns(false);
                A.CallTo(() => user.Identity).Returns(null);

                /* Act */
                var _ = await sut.HandleAsync(query, cancelToken);

                /* Assert */
                Assert.Equal(
                    $"'Anonymous' is not authorized to execute BusinessApp.Infrastructure.UnitTest.QueryStub",
                    logEntry.Message
                );
            }

            [Fact]
            public async Task NotAuthorized_LogsRequest()
            {
                /* Arrange */
                LogEntry logEntry = null;
                A.CallTo(() => authorizer.AuthorizeObject(query)).Returns(false);
                A.CallTo(() => logger.Log(A<LogEntry>._))
                    .Invokes(ctx => logEntry = ctx.GetArgument<LogEntry>(0));

                /* Act */
                var result = await sut.HandleAsync(query, cancelToken);

                /* Assert */
                Assert.Same(query, logEntry.Data);
            }

            [Fact]
            public async Task Authorized_ReturnsInnerResult()
            {
                /* Arrange */
                var result = Result<ResponseStub, Exception>.Ok(new ResponseStub());
                A.CallTo(() => authorizer.AuthorizeObject(query)).Returns(true);
                A.CallTo(() => decorated.HandleAsync(A<QueryStub>._, A<CancellationToken>._))
                    .Returns(result);

                /* Act */
                var handlerResult = await sut.HandleAsync(query, default);

                /* Assert */
                Assert.Equal(result, handlerResult);
            }
        }
    }
}
