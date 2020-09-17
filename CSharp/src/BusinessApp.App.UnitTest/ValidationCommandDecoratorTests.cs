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

    public class ValidationCommandDecoratorTests
    {
        private readonly CancellationToken token;
        private readonly ValidationCommandDecorator<CommandStub> sut;
        private readonly ICommandHandler<CommandStub> inner;
        private readonly IValidator<CommandStub> validator;

        public ValidationCommandDecoratorTests()
        {
            token = A.Dummy<CancellationToken>();
            inner = A.Fake<ICommandHandler<CommandStub>>();
            validator = A.Fake<IValidator<CommandStub>>();

            sut = new ValidationCommandDecorator<CommandStub>(validator, inner);
        }

        public class Constructor : ValidationCommandDecoratorTests
        {
            public static IEnumerable<object[]> InvalidCtorArgs => new[]
            {
                new object[] { null, A.Dummy<ICommandHandler<CommandStub>>() },
                new object[] { A.Fake<IValidator<CommandStub>>(), null },
            };

            [Theory, MemberData(nameof(InvalidCtorArgs))]
            public void InvalidCtorArgs_ExceptionThrown(
                IValidator<CommandStub> v,
                ICommandHandler<CommandStub> c)
            {
                /* Arrange */
                void shouldThrow() => new ValidationCommandDecorator<CommandStub>(v, c);

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
                var command = A.Dummy<CommandStub>();
                A.CallTo(() => validator.ValidateAsync(A<CommandStub>._, token))
                    .Invokes(ctx => handlerCallsBeforeValidate += Fake.GetCalls(inner).Count());

                /* Act */
                await sut.HandleAsync(command, token);

                /* Assert */
                Assert.Equal(0, handlerCallsBeforeValidate);
            }

            [Fact]
            public async Task WithCommand_ValidatedOnce()
            {
                /* Arrange */
                var command = A.Dummy<CommandStub>();

                /* Act */
                await sut.HandleAsync(command, token);

                /* Assert */
                A.CallTo(() => validator.ValidateAsync(command, token))
                    .MustHaveHappenedOnceExactly();
            }

            [Fact]
            public async Task WithCommand_HandledOnce()
            {
                /* Arrange */
                var token = A.Dummy<CancellationToken>();
                var command = A.Dummy<CommandStub>();


                /* Act */
                await sut.HandleAsync(command, token);

                /* Assert */
                A.CallTo(() => inner.HandleAsync(command, token))
                    .MustHaveHappenedOnceExactly();
            }
        }
    }
}
