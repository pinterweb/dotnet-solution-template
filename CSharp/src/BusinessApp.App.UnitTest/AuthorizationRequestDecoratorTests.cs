namespace BusinessApp.App.UnitTest
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using BusinessApp.Domain;
    using FakeItEasy;
    using Xunit;

    public class AuthorizationRequestDecoratorTests
    {
        private readonly AuthorizationRequestDecorator<QueryStub, ResponseStub> sut;
        private readonly IRequestHandler<QueryStub, ResponseStub> decorated;
        private readonly IAuthorizer<QueryStub> authorizer;

        public AuthorizationRequestDecoratorTests()
        {
            decorated = A.Fake<IRequestHandler<QueryStub, ResponseStub>>();
            authorizer = A.Fake<IAuthorizer<QueryStub>>();

            sut = new AuthorizationRequestDecorator<QueryStub, ResponseStub>(decorated, authorizer);
        }

        public class Constructor : AuthorizationRequestDecoratorTests
        {
            public static IEnumerable<object[]> InvalidCtorArgs
            {
                get
                {
                    return new []
                    {
                        new object[] { null, A.Dummy<IAuthorizer<QueryStub>>() },
                        new object[] { A.Dummy<IRequestHandler<QueryStub, ResponseStub>>(), null }
                    };
                }
            }

            [Theory, MemberData(nameof(InvalidCtorArgs))]
            public void WithAnyNullService_ExceptionThrown(
                IRequestHandler<QueryStub, ResponseStub> d,
                IAuthorizer<QueryStub> a)
            {
                /* Arrange */
                Action create = () => new AuthorizationRequestDecorator<QueryStub, ResponseStub>(d, a);

                /* Act */
                var exception = Record.Exception(create);

                /* Assert */
                Assert.IsType<BadStateException>(exception);
            }
        }


        public class HandleAsync : AuthorizationRequestDecoratorTests
        {
            QueryStub query;

            public HandleAsync()
            {
                query = new QueryStub();
            }

            [Fact]
            public async Task AuthorizedBeforeHandles()
            {
                /* Arrange */
                CancellationToken cancelToken = default;

                /* Act */
                await sut.HandleAsync(query, cancelToken);

                /* Assert */
                A.CallTo(() => authorizer.AuthorizeObject(query)).MustHaveHappenedOnceExactly().Then(
                    A.CallTo(() => decorated.HandleAsync(query, cancelToken)).MustHaveHappenedOnceExactly());
            }

            [Fact]
            public async Task CallsDecoratedHandlerOnce()
            {
                /* Act */
                await sut.HandleAsync(query, default);

                /* Assert */
                A.CallTo(() => decorated.HandleAsync(A<QueryStub>._, A<CancellationToken>._))
                    .MustHaveHappenedOnceExactly();
            }

            [Fact]
            public async Task CallsAuthorizerOnce()
            {
                /* Act */
                await sut.HandleAsync(query, default);

                /* Assert */
                A.CallTo(() => authorizer.AuthorizeObject(A<QueryStub>._))
                    .MustHaveHappenedOnceExactly();
            }

            [Fact]
            public async Task ReturnsDecoratedResult()
            {
                /* Arrange */
                var expectedResponse = A.Dummy<Result<ResponseStub, IFormattable>>();
                var cancelToken = new CancellationToken();
                A.CallTo(() => decorated.HandleAsync(query, cancelToken))
                    .Returns(Task.FromResult(expectedResponse));

                /* Act */
                var actualResponse = await sut.HandleAsync(query, cancelToken);

                /* Assert */
                Assert.Equal(expectedResponse, actualResponse);
            }
        }
    }
}
