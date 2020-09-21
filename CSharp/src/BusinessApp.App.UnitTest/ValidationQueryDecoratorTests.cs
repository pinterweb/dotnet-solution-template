namespace BusinessApp.App.UnitTest
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using FakeItEasy;
    using BusinessApp.App;
    using Xunit;
    using System.Threading;

    public class ValidationQueryDecoratorTests
    {
        private readonly CancellationToken token;
        private readonly ValidationQueryDecorator<QueryStub, ResponseStub> sut;
        private readonly IQueryHandler<QueryStub, ResponseStub> inner;
        private readonly IValidator<QueryStub> validator;

        public ValidationQueryDecoratorTests()
        {
            token = A.Dummy<CancellationToken>();
            inner = A.Fake<IQueryHandler<QueryStub, ResponseStub>>();
            validator = A.Fake<IValidator<QueryStub>>();

            sut = new ValidationQueryDecorator<QueryStub, ResponseStub>(validator, inner);
        }

        public class Constructor : ValidationQueryDecoratorTests
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
                void shouldThrow() => new ValidationQueryDecorator<QueryStub, ResponseStub>(v, c);

                /* Act */
                var ex = Record.Exception(shouldThrow);

                /* Assert */
                Assert.IsType<ArgumentNullException>(ex);
            }
        }

        public class HandleAsync : ValidationQueryDecoratorTests
        {
            [Fact]
            public async Task WithoutQuery_ExceptionThrown()
            {
                /* Arrange */
                Task shouldthrow() => sut.HandleAsync(null, A.Dummy<CancellationToken>());

                /* Act */
                var ex = await Record.ExceptionAsync(shouldthrow);

                /* Assert */
                Assert.NotNull(ex);
            }

            [Fact]
            public async Task WithQuery_ValidationCalledBeforeHandeler()
            {
                /* Arrange */
                var handlerCallsBeforeValidate = 0;
                var query = A.Dummy<QueryStub>();
                A.CallTo(() => validator.ValidateAsync(A<QueryStub>._, token))
                    .Invokes(ctx => handlerCallsBeforeValidate += Fake.GetCalls(inner).Count());

                /* Act */
                await sut.HandleAsync(query, token);

                /* Assert */
                A.CallTo(() => validator.ValidateAsync(query, token)).MustHaveHappened()
                    .Then(A.CallTo(() => inner.HandleAsync(query, token)).MustHaveHappened());
            }

            [Fact]
            public async Task WithQuery_ValidatedOnce()
            {
                /* Arrange */
                var command = A.Dummy<QueryStub>();

                /* Act */
                await sut.HandleAsync(command, token);

                /* Assert */
                A.CallTo(() => validator.ValidateAsync(command, token))
                    .MustHaveHappenedOnceExactly();
            }

            [Fact]
            public async Task WithQuery_HandledOnce()
            {
                /* Arrange */
                var token = A.Dummy<CancellationToken>();
                var command = A.Dummy<QueryStub>();


                /* Act */
                await sut.HandleAsync(command, token);

                /* Assert */
                A.CallTo(() => inner.HandleAsync(command, token))
                    .MustHaveHappenedOnceExactly();
            }
        }
    }
}
