namespace BusinessApp.App.UnitTest
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using BusinessApp.Domain;
    using FakeItEasy;
    using Xunit;

    public class AuthorizationQueryDecoratorTests
    {
        private readonly AuthorizationQueryDecorator<QueryStub, ResponseStub> sut;
        private readonly IQueryHandler<QueryStub, ResponseStub> decorated;
        private readonly IAuthorizer<QueryStub> authorizer;

        public AuthorizationQueryDecoratorTests()
        {
            decorated = A.Fake<IQueryHandler<QueryStub, ResponseStub>>();
            authorizer = A.Fake<IAuthorizer<QueryStub>>();

            sut = new AuthorizationQueryDecorator<QueryStub, ResponseStub>(decorated, authorizer);
        }

        public class Constructor : AuthorizationQueryDecoratorTests
        {
            public static IEnumerable<object[]> InvalidCtorArgs
            {
                get
                {
                    return new []
                    {
                        new object[] { null, A.Dummy<IAuthorizer<QueryStub>>() },
                        new object[] { A.Dummy<IQueryHandler<QueryStub, ResponseStub>>(), null }
                    };
                }
            }

            [Theory, MemberData(nameof(InvalidCtorArgs))]
            public void WithAnyNullService_ExceptionThrown(
                IQueryHandler<QueryStub, ResponseStub> d,
                IAuthorizer<QueryStub> a)
            {
                /* Arrange */
                Action create = () => new AuthorizationQueryDecorator<QueryStub, ResponseStub>(d, a);

                /* Act */
                var exception = Record.Exception(create);

                /* Assert */
                Assert.IsType<BadStateException>(exception);
            }
        }


        public class HandleAsync : AuthorizationQueryDecoratorTests
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
                CancellationToken token = default;

                /* Act */
                await sut.HandleAsync(query, token);

                /* Assert */
                A.CallTo(() => authorizer.AuthorizeObject(query)).MustHaveHappenedOnceExactly().Then(
                    A.CallTo(() => decorated.HandleAsync(query, token)).MustHaveHappenedOnceExactly());
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
                var token = new CancellationToken();
                A.CallTo(() => decorated.HandleAsync(query, token))
                    .Returns(Task.FromResult(expectedResponse));

                /* Act */
                var actualResponse = await sut.HandleAsync(query, token);

                /* Assert */
                Assert.Equal(expectedResponse, actualResponse);
            }
        }
    }
}
