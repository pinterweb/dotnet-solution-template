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

    public class BatchCommandHandlerTests
    {
        private readonly BatchCommandHandler<DummyCommand> sut;
        private readonly ICommandHandler<DummyCommand> inner;

        public BatchCommandHandlerTests()
        {
            inner = A.Fake<ICommandHandler<DummyCommand>>();

            sut = new BatchCommandHandler<DummyCommand>(inner);
        }

        public class Constructor : BatchCommandHandlerTests
        {
            public static IEnumerable<object[]> InvalidCtorArgs => new[]
            {
                new object[] { null  },
            };

            [Theory, MemberData(nameof(InvalidCtorArgs))]
            public void InvalidCtorArgs_ExceptionThrown(ICommandHandler<DummyCommand> c)
            {
                /* Arrange */
                void shouldThrow() => new BatchCommandHandler<DummyCommand>(c);

                /* Act */
                var ex = Record.Exception(shouldThrow);

                /* Assert */
                Assert.IsType<ArgumentNullException>(ex);
            }
        }

        public class HandleAsync : BatchCommandHandlerTests
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
            public async Task WithCommands_EachCalled()
            {
                /* Arrange */
                var commands = new[] { A.Dummy<DummyCommand>(), A.Dummy<DummyCommand>() };
                var token = CancellationToken.None;

                /* Act */
                await sut.HandleAsync(commands, token);

                /* Assert */
                A.CallTo(() => inner.HandleAsync(commands.First(), token)).MustHaveHappenedOnceExactly();
                A.CallTo(() => inner.HandleAsync(commands.Last(), token)).MustHaveHappenedOnceExactly();
            }
        }
    }
}
