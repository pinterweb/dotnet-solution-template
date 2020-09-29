namespace BusinessApp.App.UnitTest
{
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using BusinessApp.Domain;
    using FakeItEasy;
    using Xunit;

    public class BatchMacroCommandDecoratorTests
    {
        private readonly CancellationToken token;
        private readonly BatchMacroCommandDecorator<CommandMacro, CommandStub> sut;
        private readonly ICommandHandler<IEnumerable<CommandStub>> inner;
        private readonly IBatchMacro<CommandMacro, CommandStub> expander;

        public BatchMacroCommandDecoratorTests()
        {
            token = A.Dummy<CancellationToken>();
            inner = A.Fake<ICommandHandler<IEnumerable<CommandStub>>>();
            expander = A.Fake<IBatchMacro<CommandMacro, CommandStub>>();

            sut = new BatchMacroCommandDecorator<CommandMacro, CommandStub>(expander, inner);
        }

        public class Constructor : BatchMacroCommandDecoratorTests
        {
            public static IEnumerable<object[]> InvalidCtorArgs => new[]
            {
                new object[]
                {
                    null,
                    A.Dummy<ICommandHandler<IEnumerable<CommandStub>>>(),
                },
                new object[]
                {
                    A.Fake<IBatchMacro<CommandMacro, CommandStub>>(),
                    null,
                },
            };

            [Theory, MemberData(nameof(InvalidCtorArgs))]
            public void InvalidCtorArgs_ExceptionThrown(
                IBatchMacro<CommandMacro, CommandStub> g,
                ICommandHandler<IEnumerable<CommandStub>> i)
            {
                /* Arrange */
                void shouldThrow() => new BatchMacroCommandDecorator<CommandMacro, CommandStub>(g, i);

                /* Act */
                var ex = Record.Exception(shouldThrow);

                /* Assert */
                Assert.IsType<BadStateException>(ex);
            }
        }

        public class HandleAsync : BatchMacroCommandDecoratorTests
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
            public async Task NoReturnedPayloadsFromMacro_ValidationExceptionThrown()
            {
                /* Arrange */
                var macro = A.Dummy<CommandMacro>();
                A.CallTo(() => expander.ExpandAsync(macro, token)).Returns(new CommandStub[0]);

                /* Act */
                var ex = await Record.ExceptionAsync(() => sut.HandleAsync(macro, token));

                /* Assert */
                Assert.Equal(
                    "The macro you ran expected to find records to change, but none were " +
                    "found",
                    Assert.IsType<BusinessAppAppException>(ex)?.Message
                );
            }

            [Fact]
            public async Task ReturnedPayloadsFromMacro_PassedToInnerHandler()
            {
                /* Arrange */
                var macro = A.Dummy<CommandMacro>();
                var commands = new[] { A.Dummy<CommandStub>() };
                A.CallTo(() => expander.ExpandAsync(macro, token)).Returns(commands);

                /* Act */
                var ex = await Record.ExceptionAsync(() => sut.HandleAsync(macro, token));

                /* Assert */
                A.CallTo(() => inner.HandleAsync(commands, token)).MustHaveHappenedOnceExactly();
            }
        }

        public sealed class CommandMacro : IMacro<CommandStub>
        {}
    }
}
