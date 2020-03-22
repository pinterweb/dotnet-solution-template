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

    public class ValidationBatchCommandDecoratorTests
    {
        private readonly ValidationBatchCommandDecorator<DummyCommand> sut;
        private readonly ICommandHandler<IEnumerable<DummyCommand>> inner;
        private readonly IValidator<DummyCommand> validator;

        public ValidationBatchCommandDecoratorTests()
        {
            inner = A.Fake<ICommandHandler<IEnumerable<DummyCommand>>>();
            validator = A.Fake<IValidator<DummyCommand>>();

            sut = new ValidationBatchCommandDecorator<DummyCommand>(validator, inner);
        }

        public class Constructor : ValidationBatchCommandDecoratorTests
        {
            public static IEnumerable<object[]> InvalidCtorArgs => new[]
            {
                new object[] { null, A.Dummy<ICommandHandler<IEnumerable<DummyCommand>>>() },
                new object[] { A.Fake<IValidator<DummyCommand>>(), null },
            };

            [Theory, MemberData(nameof(InvalidCtorArgs))]
            public void InvalidCtorArgs_ExceptionThrown(
                IValidator<DummyCommand> v,
                ICommandHandler<IEnumerable<DummyCommand>> c)
            {
                /* Arrange */
                void shouldThrow() => new ValidationBatchCommandDecorator<DummyCommand>(v, c);

                /* Act */
                var ex = Record.Exception(shouldThrow);

                /* Assert */
                Assert.IsType<ArgumentNullException>(ex);
            }
        }

        public class HandleAsync : ValidationBatchCommandDecoratorTests
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
                var commands = new[] { A.Dummy<DummyCommand>(), A.Dummy<DummyCommand>() };
                A.CallTo(() => validator.ValidateObject(A<DummyCommand>._))
                    .Invokes(ctx => handlerCallsBeforeValidate += Fake.GetCalls(inner).Count());

                /* Act */
                await sut.HandleAsync(commands, A.Dummy<CancellationToken>());

                /* Assert */
                Assert.Equal(0, handlerCallsBeforeValidate);
            }

            [Fact]
            public async Task WithTwoCommands_ValidatedTwice()
            {
                /* Arrange */
                var commands = new[] { A.Dummy<DummyCommand>(), A.Dummy<DummyCommand>() };

                /* Act */
                await sut.HandleAsync(commands, A.Dummy<CancellationToken>());

                /* Assert */
                A.CallTo(() => validator.ValidateObject(commands.First()))
                    .MustHaveHappenedOnceExactly().Then(
                        A.CallTo(() => validator.ValidateObject(commands.Last()))
                            .MustHaveHappenedOnceExactly());
            }

            [Fact]
            public async Task WithTwoCommands_HandledOnce()
            {
                /* Arrange */
                var token = A.Dummy<CancellationToken>();
                var commands = new[] { A.Dummy<DummyCommand>(), A.Dummy<DummyCommand>() };

                /* Act */
                await sut.HandleAsync(commands, token);

                /* Assert */
                A.CallTo(() => inner.HandleAsync(commands, token))
                    .MustHaveHappenedOnceExactly();
            }
        }
    }
}
