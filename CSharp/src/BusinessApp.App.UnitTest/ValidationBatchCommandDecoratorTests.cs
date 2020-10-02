namespace BusinessApp.App.UnitTest
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using FakeItEasy;
    using Xunit;
    using System.Threading;
    using BusinessApp.Domain;

    public class ValidationBatchCommandDecoratorTests
    {
        private readonly CancellationToken token;
        private readonly ValidationBatchCommandDecorator<CommandStub, IEnumerable<CommandStub>> sut;
        private readonly ICommandHandler<IEnumerable<CommandStub>> inner;
        private readonly IValidator<CommandStub> validator;

        public ValidationBatchCommandDecoratorTests()
        {
            token = A.Dummy<CancellationToken>();
            inner = A.Fake<ICommandHandler<IEnumerable<CommandStub>>>();
            validator = A.Fake<IValidator<CommandStub>>();

            sut = new ValidationBatchCommandDecorator<CommandStub, IEnumerable<CommandStub>>(validator, inner);
        }

        public class Constructor : ValidationBatchCommandDecoratorTests
        {
            public static IEnumerable<object[]> InvalidCtorArgs => new[]
            {
                new object[] { null, A.Dummy<ICommandHandler<IEnumerable<CommandStub>>>() },
                new object[] { A.Dummy<IValidator<CommandStub>>(), null },
            };

            [Theory, MemberData(nameof(InvalidCtorArgs))]
            public void InvalidCtorArgs_ExceptionThrown(
                IValidator<CommandStub> v,
                ICommandHandler<IEnumerable<CommandStub>> c)
            {
                /* Arrange */
                void shouldThrow() => new ValidationBatchCommandDecorator<CommandStub, IEnumerable<CommandStub>>(v, c);

                /* Act */
                var ex = Record.Exception(shouldThrow);

                /* Assert */
                Assert.IsType<BadStateException>(ex);
            }
        }

        public class HandleAsync : ValidationBatchCommandDecoratorTests
        {
            [Fact]
            public async Task WithoutCommandArg_ExceptionThrown()
            {
                /* Arrange */
                Task shouldthrow() => sut.HandleAsync(null, A.Dummy<CancellationToken>());

                /* Act */
                var ex = await Record.ExceptionAsync(shouldthrow);

                /* Assert */
                Assert.NotNull(ex);
            }

            [Fact]
            public async Task WithCommandArg_ValidationCalledBeforeHandeler()
            {
                /* Arrange */
                var handlerCallsBeforeValidate = 0;
                var commands = new[] { A.Dummy<CommandStub>(), A.Dummy<CommandStub>() };
                A.CallTo(() => validator.ValidateAsync(A<CommandStub>._, token))
                    .Invokes(ctx => handlerCallsBeforeValidate += Fake.GetCalls(inner).Count());

                /* Act */
                var _ = await sut.HandleAsync(commands, token);

                /* Assert */
                Assert.Equal(0, handlerCallsBeforeValidate);
            }

            [Fact]
            public async Task WithTwoCommandArgss_ValidatedTwice()
            {
                /* Arrange */
                var commands = new[] { A.Dummy<CommandStub>(), A.Dummy<CommandStub>() };

                /* Act */
                var _ = await sut.HandleAsync(commands, token);

                /* Assert */
                A.CallTo(() => validator.ValidateAsync(commands.First(), token))
                    .MustHaveHappenedOnceExactly().Then(
                        A.CallTo(() => validator.ValidateAsync(commands.Last(), token))
                            .MustHaveHappenedOnceExactly());
            }

            [Fact]
            public async Task WithTwoCommandArgs_HandledOnce()
            {
                /* Arrange */
                var commands = new[] { A.Dummy<CommandStub>(), A.Dummy<CommandStub>() };

                /* Act */
                var _ = await sut.HandleAsync(commands, token);

                /* Assert */
                A.CallTo(() => inner.HandleAsync(commands, token))
                    .MustHaveHappenedOnceExactly();
            }

            [Fact]
            public async Task NoValidationErrors_InnerResultReturned()
            {
                /* Arrange */
                var commands = new[] { A.Dummy<CommandStub>(), A.Dummy<CommandStub>() };
                var innerResult = Result<IEnumerable<CommandStub>, IFormattable>.Error(DateTime.Now);
                A.CallTo(() => inner.HandleAsync(commands, token))
                    .Returns(innerResult);

                /* Act */
                var results = await sut.HandleAsync(commands, token);

                /* Assert */
                Assert.Equal(innerResult, results);
            }

            [Fact]
            public async Task ModelValidationExceptionThrown_BatchExceptionInError()
            {
                /* Arrange */
                var commands = new[] { A.Dummy<CommandStub>() };
                var memberError = new MemberValidationException("foo", A.CollectionOfDummy<string>(1));

                A.CallTo(() => validator.ValidateAsync(commands.First(), token))
                    .Throws(new ModelValidationException("foomsg", new[] { memberError }));

                /* Act */
                var result = await sut.HandleAsync(commands, token);

                /* Assert */
                Assert.IsType<BatchException>(result.UnwrapError());
            }

            [Fact]
            public async Task ModelValidationExceptionThrown_ResultsInBatchException()
            {
                /* Arrange */
                var commands = new[]
                {
                    A.Dummy<CommandStub>(),
                    A.Dummy<CommandStub>(),
                    A.Dummy<CommandStub>()
                };
                var modelError = new ModelValidationException("foomsg", A.CollectionOfDummy<MemberValidationException>(1));
                var innerError = new ModelValidationException("barmsg", A.CollectionOfDummy<MemberValidationException>(1));

                A.CallTo(() => validator.ValidateAsync(A<CommandStub>._, token))
                    .Throws(modelError)
                    .Once()
                    .Then
                    .Throws(new AggregateException(new[] { innerError }))
                    .Once();

                /* Act */
                var result = await sut.HandleAsync(commands, token);

                /* Assert */
                var error = Assert.IsType<BatchException>(result.UnwrapError());
                Assert.Collection(error.Results,
                    r => Assert.Same(modelError, r.UnwrapError()),
                    r => Assert.Same(innerError, r.UnwrapError())
                );
            }
        }
    }
}
