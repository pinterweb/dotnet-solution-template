using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FakeItEasy;
using Xunit;
using System.Threading;
using BusinessApp.Domain;

namespace BusinessApp.App.UnitTest
{
    public class EntityNotFoundQueryDecoratorTests
    {
        private readonly CancellationToken cancelToken;
        private readonly EntityNotFoundQueryDecorator<QueryStub, ResponseStub> sut;
        private readonly IRequestHandler<QueryStub, ResponseStub> inner;

        public EntityNotFoundQueryDecoratorTests()
        {
            cancelToken = A.Dummy<CancellationToken>();
            inner = A.Fake<IRequestHandler<QueryStub, ResponseStub>>();

            sut = new EntityNotFoundQueryDecorator<QueryStub, ResponseStub>(inner);
        }

        public class Constructor : EntityNotFoundQueryDecoratorTests
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
                void shouldThrow() => new EntityNotFoundQueryDecorator<QueryStub, ResponseStub>(q);

                /* Act */
                var ex = Record.Exception(shouldThrow);

                /* Assert */
                Assert.IsType<BusinessAppException>(ex);
            }
        }

        public class HandleAsync : EntityNotFoundQueryDecoratorTests
        {
            [Fact]
            public async Task InnerHandlerHasNullOkValue_ReturnsErrorResult()
            {
                /* Arrange */
                var query = A.Dummy<QueryStub>();
                A.CallTo(() => inner.HandleAsync(query, cancelToken))
                    .Returns(Result.Ok<ResponseStub>(null));

                /* Act */
                var result = await sut.HandleAsync(query, cancelToken);

                /* Assert */
                var error = Assert.IsType<EntityNotFoundException>(result.UnwrapError());
                Assert.Equal(
                    "The data you tried to search for was not " +
                    "found based on your search critiera. Try to change your criteria " +
                    "and search again. If the data is still not found, it may have been deleted.",
                    error.Message
                );
            }

            [Fact]
            public async Task InnerHandlerHasOkValue_ReturnsInnerResult()
            {
                /* Arrange */
                var query = A.Dummy<QueryStub>();
                var innerResult = Result.Ok(new ResponseStub());
                A.CallTo(() => inner.HandleAsync(query, cancelToken))
                    .Returns(innerResult);

                /* Act */
                var result = await sut.HandleAsync(query, cancelToken);

                /* Assert */
                Assert.Equal(innerResult, result);
            }

            [Fact]
            public async Task InnerHandlerHasErrorValue_ReturnsInnerResult()
            {
                /* Arrange */
                var query = A.Dummy<QueryStub>();
                var innerResult = Result.Error<ResponseStub>(A.Dummy<Exception>());
                A.CallTo(() => inner.HandleAsync(query, cancelToken))
                    .Returns(innerResult);

                /* Act */
                var result = await sut.HandleAsync(query, cancelToken);

                /* Assert */
                Assert.Equal(innerResult, result);
            }
        }
    }
}
