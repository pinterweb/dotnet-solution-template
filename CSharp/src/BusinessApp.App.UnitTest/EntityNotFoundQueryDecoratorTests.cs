namespace BusinessApp.App.UnitTest
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using FakeItEasy;
    using BusinessApp.App;
    using Xunit;
    using System.Threading;
    using BusinessApp.Domain;

    public class EntityNotFoundQueryDecoratorTests
    {
        private readonly CancellationToken token;
        private readonly EntityNotFoundQueryDecorator<QueryStub, ResponseStub> sut;
        private readonly IQueryHandler<QueryStub, ResponseStub> inner;

        public EntityNotFoundQueryDecoratorTests()
        {
            token = A.Dummy<CancellationToken>();
            inner = A.Fake<IQueryHandler<QueryStub, ResponseStub>>();

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
            public void InvalidCtorArgs_ExceptionThrown(IQueryHandler<QueryStub, ResponseStub> q)
            {
                /* Arrange */
                void shouldThrow() => new EntityNotFoundQueryDecorator<QueryStub, ResponseStub>(q);

                /* Act */
                var ex = Record.Exception(shouldThrow);

                /* Assert */
                Assert.IsType<BadStateException>(ex);
            }
        }

        public class HandleAsync : EntityNotFoundQueryDecoratorTests
        {
            [Fact]
            public async Task InnerHandlerHasNullOkValue_ReturnsErrorResult()
            {
                /* Arrange */
                var query = A.Dummy<QueryStub>();
                A.CallTo(() => inner.HandleAsync(query, token))
                    .Returns(Result<ResponseStub, IFormattable>.Ok(null));

                /* Act */
                var result = await sut.HandleAsync(query, token);

                /* Assert */
                var error = Assert.IsType<EntityNotFoundException>(result.UnwrapError());
                Assert.Equal(
                    "The data you tried to search for was not " +
                    "found based on your search critiera. Try to change your criteria " +
                    "and search again. If the data is still found, it may have been deleted.",
                    error.Message
                );
            }

            [Fact]
            public async Task InnerHandlerHasOkValue_ReturnsInnerResult()
            {
                /* Arrange */
                var query = A.Dummy<QueryStub>();
                var innerResult = Result<ResponseStub, IFormattable>.Ok(new ResponseStub());
                A.CallTo(() => inner.HandleAsync(query, token))
                    .Returns(innerResult);

                /* Act */
                var result = await sut.HandleAsync(query, token);

                /* Assert */
                Assert.Equal(innerResult, result);
            }

            [Fact]
            public async Task InnerHandlerHasErrorValue_ReturnsInnerResult()
            {
                /* Arrange */
                var query = A.Dummy<QueryStub>();
                var innerResult = Result<ResponseStub, IFormattable>.Error(DateTime.Now);
                A.CallTo(() => inner.HandleAsync(query, token))
                    .Returns(innerResult);

                /* Act */
                var result = await sut.HandleAsync(query, token);

                /* Assert */
                Assert.Equal(innerResult, result);
            }
        }
    }
}
