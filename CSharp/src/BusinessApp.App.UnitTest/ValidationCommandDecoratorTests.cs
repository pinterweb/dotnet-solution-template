namespace BusinessApp.App.UnitTests
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using FakeItEasy;
    using BusinessApp.App;
    using Xunit;
    using System.Threading;

    public class ValidationCommandDecoratorTests
    {
        private readonly ValidationCommandDecorator<DummyCommand> sut;
        private readonly ICommandHandler<DummyCommand> inner;
        private readonly IValidator<DummyCommand> validator;

        public ValidationCommandDecoratorTests()
        {
            inner = A.Fake<ICommandHandler<DummyCommand>>();
            validator = A.Fake<IValidator<DummyCommand>>();

            sut = new ValidationCommandDecorator<DummyCommand>(validator, inner);
        }

        public class Constructor : ValidationCommandDecoratorTests
        {
            public static IEnumerable<object[]> InvalidCtorArgs => new[]
            {
                new object[] { null, A.Dummy<ICommandHandler<DummyCommand>>() },
                new object[] { A.Fake<IValidator<DummyCommand>>(), null },
            };

            [Theory, MemberData(nameof(InvalidCtorArgs))]
            public void InvalidCtorArgs_ExceptionThrown(
                IValidator<DummyCommand> v,
                ICommandHandler<DummyCommand> c)
            {
                /* Arrange */
                void shouldThrow() => new ValidationCommandDecorator<DummyCommand>(v, c);

                /* Act */
                var ex = Record.Exception(shouldThrow);

                /* Assert */
                Assert.IsType<ArgumentNullException>(ex);
            }
        }

        public class HandleAsync : ValidationCommandDecoratorTests
        {
            [Fact]
            public async Task WithoutCommand_ExceptionThrown()
            {
                /* Arrange */
                Task shouldthrow() => sut.HandleAsync(null, A.Dummy<CancellationToken>());

                /* Act */
                var ex = await Record.ExceptionAsync(shouldthrow);

                /* Assert */
                Assert.NotNull(ex);
            }

            [Fact]
            public async Task WithCommands_ValidationCalledBeforeHandeler()
            {
                /* Arrange */
                var handlerCallsBeforeValidate = 0;
                var command = A.Dummy<DummyCommand>();
                A.CallTo(() => validator.ValidateAsync(A<DummyCommand>._))
                    .Invokes(ctx => handlerCallsBeforeValidate += Fake.GetCalls(inner).Count());

                /* Act */
                await sut.HandleAsync(command, A.Dummy<CancellationToken>());

                /* Assert */
                Assert.Equal(0, handlerCallsBeforeValidate);
            }

            [Fact]
            public async Task WithCommand_ValidatedOnce()
            {
                /* Arrange */
                var command = A.Dummy<DummyCommand>();

                /* Act */
                await sut.HandleAsync(command, A.Dummy<CancellationToken>());

                /* Assert */
                A.CallTo(() => validator.ValidateAsync(command))
                    .MustHaveHappenedOnceExactly();
            }

            [Fact]
            public async Task WithCommand_HandledOnce()
            {
                /* Arrange */
                var token = A.Dummy<CancellationToken>();
                var command = A.Dummy<DummyCommand>();


                /* Act */
                await sut.HandleAsync(command, token);

                /* Assert */
                A.CallTo(() => inner.HandleAsync(command, token))
                    .MustHaveHappenedOnceExactly();
            }
        }
    }
}
