namespace BusinessApp.App.UnitTest
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using FakeItEasy;
    using BusinessApp.App;
    using Xunit;
    using System.Threading;
    using BusinessApp.Domain;
    using System;

    public class QueryLifetimeCacheDecoratorTests
    {
        private readonly CancellationToken token;
        private readonly QueryLifetimeCacheDecorator<QueryStub, ResponseStub> sut;
        private readonly IQueryHandler<QueryStub, ResponseStub> inner;

        public QueryLifetimeCacheDecoratorTests()
        {
            token = A.Dummy<CancellationToken>();
            inner = A.Fake<IQueryHandler<QueryStub, ResponseStub>>();

            sut = new QueryLifetimeCacheDecorator<QueryStub, ResponseStub>(inner);
        }

        public class Constructor : QueryLifetimeCacheDecoratorTests
        {
            public static IEnumerable<object[]> InvalidCtorArgs => new[]
            {
                new object[]
                {
                    null,
                }
            };

            [Theory, MemberData(nameof(InvalidCtorArgs))]
            public void InvalidCtorArgs_ExceptionThrown(IQueryHandler<QueryStub, ResponseStub> q)
            {
                /* Arrange */
                void shouldThrow() => new QueryLifetimeCacheDecorator<QueryStub, ResponseStub>(q);

                /* Act */
                var ex = Record.Exception(shouldThrow);

                /* Assert */
                Assert.IsType<BadStateException>(ex);
            }
        }

        public class HandleAsync : QueryLifetimeCacheDecoratorTests
        {
            [Fact]
            public async Task SameQueryInstanceRunTwice_InnerHandlerNotRun()
            {
                /* Arrange */
                var query = new QueryStub();
                await sut.HandleAsync(query, token);
                Fake.ClearRecordedCalls(inner);

                /* Act */
                await sut.HandleAsync(query, token);

                /* Assert */
                A.CallTo(() => inner.HandleAsync(A<QueryStub>._, A<CancellationToken>._))
                    .MustNotHaveHappened();
            }

            [Fact]
            public async Task SameQueryInstanceRunTwice_CacheValueReturned()
            {
                /* Arrange */
                var query = new QueryStub();
                var firstResponse = Result<ResponseStub, IFormattable>.Error(DateTime.Now);
                A.CallTo(() => inner.HandleAsync(query, token))
                    .Returns(firstResponse)
                    .Once();
                await sut.HandleAsync(query, token);

                /* Act */
                var secondResponse = await sut.HandleAsync(query, token);

                /* Assert */
                Assert.Equal(firstResponse, secondResponse);
            }

            [Fact]
            public async Task EqualInstancesRunTwice_CacheValueReturned()
            {
                /* Arrange */
                var query1 = new QueryStub { Id = 1 };
                var query2 = new QueryStub { Id = 1 };
                var firstResponse = Result<ResponseStub, IFormattable>.Error(DateTime.Now);
                A.CallTo(() => inner.HandleAsync(query1, token))
                    .Returns(firstResponse)
                    .Once();
                await sut.HandleAsync(query1, token);

                /* Act */
                var secondResponse = await sut.HandleAsync(query2, token);

                /* Assert */
                Assert.Equal(firstResponse, secondResponse);
            }

            [Fact]
            public async Task NotEqualInstancesRunTwice_HandlerRunsTwice()
            {
                /* Arrange */
                var query1 = new QueryStub { Id = 1 };
                var query2 = new QueryStub { Id = 2 };
                var firstResponse = Result<ResponseStub, IFormattable>.Error(DateTime.Now);
                var secondResponse = Result<ResponseStub, IFormattable>.Ok(new ResponseStub());
                A.CallTo(() => inner.HandleAsync(query1, token))
                    .Returns(firstResponse);
                A.CallTo(() => inner.HandleAsync(query2, token))
                    .Returns(secondResponse);
                await sut.HandleAsync(query1, token);

                /* Act */
                var secondResult = await sut.HandleAsync(query2, token);

                /* Assert */
                Assert.Equal(secondResponse, secondResult);
            }
        }
    }
}
