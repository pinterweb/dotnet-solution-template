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

    public class ValidationRequestDecoratorTests
    {
        private readonly CancellationToken cancelToken;
        private readonly ValidationRequestDecorator<QueryStub, ResponseStub> sut;
        private readonly IQueryHandler<QueryStub, ResponseStub> inner;
        private readonly IValidator<QueryStub> validator;

        public ValidationRequestDecoratorTests()
        {
            cancelToken = A.Dummy<CancellationToken>();
            inner = A.Fake<IQueryHandler<QueryStub, ResponseStub>>();
            validator = A.Fake<IValidator<QueryStub>>();

            sut = new ValidationRequestDecorator<QueryStub, ResponseStub>(validator, inner);
        }

        public class Constructor : ValidationRequestDecoratorTests
        {
            public static IEnumerable<object[]> InvalidCtorArgs => new[]
            {
                new object[] { null, A.Dummy<IQueryHandler<QueryStub, ResponseStub>>() },
                new object[] { A.Fake<IValidator<QueryStub>>(), null },
            };

            [Theory, MemberData(nameof(InvalidCtorArgs))]
            public void InvalidCtorArgs_ExceptionThrown(
                IValidator<QueryStub> v,
                IQueryHandler<QueryStub, ResponseStub> c)
            {
                /* Arrange */
                void shouldThrow() => new ValidationRequestDecorator<QueryStub, ResponseStub>(v, c);

                /* Act */
                var ex = Record.Exception(shouldThrow);

                /* Assert */
                Assert.IsType<BadStateException>(ex);
            }
        }

        public class HandleAsync : ValidationRequestDecoratorTests
        {
            [Fact]
            public async Task WithoutRequestArg_ExceptionThrown()
            {
                /* Arrange */
                Task shouldthrow() => sut.HandleAsync(null, A.Dummy<CancellationToken>());

                /* Act */
                var ex = await Record.ExceptionAsync(shouldthrow);

                /* Assert */
                Assert.NotNull(ex);
            }

            [Fact]
            public async Task InvalidRequest_ValidationResultErrorReturned()
            {
                /* Arrange */
                var query = A.Dummy<QueryStub>();
                A.CallTo(() => validator.ValidateAsync(A<QueryStub>._, cancelToken))
                    .Returns(Result.Error($"foobar"));

                /* Act */
                var result = await sut.HandleAsync(query, cancelToken);

                /* Assert */
                Assert.Equal(Result<ResponseStub, IFormattable>.Error($"foobar"), result);
            }

            [Fact]
            public async Task InvalidRequest_InnerHandlerNotRun()
            {
                /* Arrange */
                var query = A.Dummy<QueryStub>();
                A.CallTo(() => validator.ValidateAsync(A<QueryStub>._, cancelToken))
                    .Returns(Result.Error($"foobar"));

                /* Act */
                var result = await sut.HandleAsync(query, cancelToken);

                /* Assert */
                A.CallTo(() => inner.HandleAsync(A<QueryStub>._, A<CancellationToken>._))
                    .MustNotHaveHappened();
            }

            [Fact]
            public async Task ValidRequest_HandledOnce()
            {
                /* Arrange */
                var query = A.Dummy<QueryStub>();
                A.CallTo(() => validator.ValidateAsync(A<QueryStub>._, cancelToken))
                    .Returns(Result.Ok);

                /* Act */
                var _ = await sut.HandleAsync(query, cancelToken);

                /* Assert */
                A.CallTo(() => inner.HandleAsync(query, cancelToken))
                    .MustHaveHappenedOnceExactly();
            }

            [Fact]
            public async Task WithRequestArg_InnerResultsReturned()
            {
                /* Arrange */
                var cancelToken = A.Dummy<CancellationToken>();
                var query = A.Dummy<QueryStub>();
                var innerResults = A.Dummy<Result<ResponseStub, IFormattable>>();
                A.CallTo(() => inner.HandleAsync(query, cancelToken))
                    .Returns(innerResults);

                /* Act */
                var results = await sut.HandleAsync(query, cancelToken);

                /* Assert */
                Assert.Equal(innerResults, results);
            }
        }
    }
}
