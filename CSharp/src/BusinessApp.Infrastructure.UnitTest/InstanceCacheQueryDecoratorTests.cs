using System.Collections.Generic;
using System.Threading.Tasks;
using FakeItEasy;
using Xunit;
using System.Threading;
using BusinessApp.Kernel;
using System;
using BusinessApp.Test.Shared;

namespace BusinessApp.Infrastructure.UnitTest
{
    public class InstanceCacheQueryDecoratorTests
    {
        private readonly CancellationToken cancelToken;
        private readonly InstanceCacheQueryDecorator<QueryStub, ResponseStub> sut;
        private readonly IRequestHandler<QueryStub, ResponseStub> inner;

        public InstanceCacheQueryDecoratorTests()
        {
            cancelToken = A.Dummy<CancellationToken>();
            inner = A.Fake<IRequestHandler<QueryStub, ResponseStub>>();

            sut = new InstanceCacheQueryDecorator<QueryStub, ResponseStub>(inner);
        }

        public class Constructor : InstanceCacheQueryDecoratorTests
        {
            public static IEnumerable<object[]> InvalidCtorArgs => new[]
            {
                new object[]
                {
                    null,
                }
            };

            [Theory, MemberData(nameof(InvalidCtorArgs))]
            public void InvalidCtorArgs_ExceptionThrown(IRequestHandler<QueryStub, ResponseStub> q)
            {
                /* Arrange */
                void shouldThrow() => new InstanceCacheQueryDecorator<QueryStub, ResponseStub>(q);

                /* Act */
                var ex = Record.Exception(shouldThrow);

                /* Assert */
                Assert.IsType<BusinessAppException>(ex);
            }
        }

        public class HandleAsync : InstanceCacheQueryDecoratorTests
        {
            [Fact]
            public async Task SameQueryInstanceRunTwice_InnerHandlerNotRun()
            {
                /* Arrange */
                var query = new QueryStub();
                await sut.HandleAsync(query, cancelToken);
                Fake.ClearRecordedCalls(inner);

                /* Act */
                await sut.HandleAsync(query, cancelToken);

                /* Assert */
                A.CallTo(() => inner.HandleAsync(A<QueryStub>._, A<CancellationToken>._))
                    .MustNotHaveHappened();
            }

            [Fact]
            public async Task SameQueryInstanceRunTwice_CacheValueReturned()
            {
                /* Arrange */
                var query = new QueryStub();
                var firstResponse = Result.Error<ResponseStub>(A.Dummy<Exception>());
                A.CallTo(() => inner.HandleAsync(query, cancelToken))
                    .Returns(firstResponse)
                    .Once();
                await sut.HandleAsync(query, cancelToken);

                /* Act */
                var secondResponse = await sut.HandleAsync(query, cancelToken);

                /* Assert */
                Assert.Equal(firstResponse, secondResponse);
            }

            [Fact]
            public async Task EqualInstancesRunTwice_CacheValueReturned()
            {
                /* Arrange */
                var query1 = new QueryStub { Id = 1 };
                var query2 = new QueryStub { Id = 1 };
                var firstResponse = Result.Error<ResponseStub>(A.Dummy<Exception>());
                A.CallTo(() => inner.HandleAsync(query1, cancelToken))
                    .Returns(firstResponse)
                    .Once();
                await sut.HandleAsync(query1, cancelToken);

                /* Act */
                var secondResponse = await sut.HandleAsync(query2, cancelToken);

                /* Assert */
                Assert.Equal(firstResponse, secondResponse);
            }

            [Fact]
            public async Task NotEqualInstancesRunTwice_HandlerRunsTwice()
            {
                /* Arrange */
                var query1 = new QueryStub { Id = 1 };
                var query2 = new QueryStub { Id = 2 };
                var firstResponse = Result.Error<ResponseStub>(A.Dummy<Exception>());
                var secondResponse = Result.Ok(new ResponseStub());
                A.CallTo(() => inner.HandleAsync(query1, cancelToken))
                    .Returns(firstResponse);
                A.CallTo(() => inner.HandleAsync(query2, cancelToken))
                    .Returns(secondResponse);
                await sut.HandleAsync(query1, cancelToken);

                /* Act */
                var secondResult = await sut.HandleAsync(query2, cancelToken);

                /* Assert */
                Assert.Equal(secondResponse, secondResult);
            }
        }
    }
}
