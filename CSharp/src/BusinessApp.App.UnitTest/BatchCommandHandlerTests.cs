namespace BusinessApp.App.UnitTest
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using BusinessApp.Domain;
    using FakeItEasy;
    using Xunit;

    public class BatchCommandHandlerTests
    {
        private readonly CancellationToken token;
        private readonly BatchCommandHandler<CommandStub, CommandStub> sut;
        private readonly ICommandHandler<CommandStub> inner;

        public BatchCommandHandlerTests()
        {
            token = A.Dummy<CancellationToken>();
            inner = A.Fake<ICommandHandler<CommandStub>>();

            sut = new BatchCommandHandler<CommandStub, CommandStub>(inner);
        }

        public class Constructor : BatchCommandHandlerTests
        {
            public static IEnumerable<object[]> InvalidCtorArgs => new[]
            {
                new object[] { null  },
            };

            [Theory, MemberData(nameof(InvalidCtorArgs))]
            public void InvalidCtorArgs_ExceptionThrown(ICommandHandler<CommandStub> c)
            {
                /* Arrange */
                void shouldThrow() => new BatchCommandHandler<CommandStub, CommandStub>(c);

                /* Act */
                var ex = Record.Exception(shouldThrow);

                /* Assert */
                Assert.IsType<BadStateException>(ex);
            }
        }

        public class HandleAsync : BatchCommandHandlerTests
        {
            [Fact]
            public async Task WithoutCommandArg_ExceptionThrown()
            {
                /* Arrange */
                Task shouldthrow() => sut.HandleAsync(null, token);

                /* Act */
                var ex = await Record.ExceptionAsync(shouldthrow);

                /* Assert */
                Assert.NotNull(ex);
            }

            [Fact]
            public async Task WithMultipleCommands_HandlerCalledForEach()
            {
                /* Arrange */
                var commands = new[] { A.Dummy<CommandStub>(), A.Dummy<CommandStub>() };

                /* Act */
                await sut.HandleAsync(commands, token);

                /* Assert */
                A.CallTo(() => inner.HandleAsync(commands.First(), token)).MustHaveHappenedOnceExactly();
                A.CallTo(() => inner.HandleAsync(commands.Last(), token)).MustHaveHappenedOnceExactly();
            }

            public class OnError : BatchCommandHandlerTests
            {
                private readonly Result<CommandStub, IFormattable> error;
                private readonly Result<CommandStub, IFormattable> ok;
                private readonly IEnumerable<CommandStub> commands;

                public OnError()
                {
                    commands = new[]
                    {
                        new CommandStub(),
                        new CommandStub(),
                        new CommandStub(),
                    };

                    error = Result<CommandStub, IFormattable>
                        .Error(DateTime.Now);
                    ok = Result<CommandStub, IFormattable>
                        .Ok(A.Dummy<CommandStub>());
                    A.CallTo(() => inner.HandleAsync(A<CommandStub>._, token))
                        .Returns(error).Once().Then.Returns(ok).Once().Then.Returns(ok);
                }

                [Fact]
                public async Task AllReturnedInBatchException()
                {
                    /* Act */
                    var results = await sut.HandleAsync(commands, token);

                    /* Assert */
                    Assert.IsType<BatchException>(results.UnwrapError());
                }

                [Fact]
                public async Task AllReturnedInBatchExceptionInOrder()
                {
                    /* Act */
                    var results = await sut.HandleAsync(commands, token);

                    /* Assert */
                    var ex = Assert.IsType<BatchException>(results.UnwrapError());
                    Assert.Collection(ex.Results,
                        r => Assert.Equal(error.UnwrapError(), r.UnwrapError()),
                        r => Assert.Equal(Result.Ok, r.Kind),
                        r => Assert.Equal(Result.Ok, r.Kind)
                    );
                }
            }

            public async Task AllOkResults_ResultsReturned()
            {
                /* Arrange */
                var commands = new[]
                {
                    new CommandStub(),
                    new CommandStub(),
                    new CommandStub(),
                };
                var result1 = new CommandStub();
                var result2 = new CommandStub();

                var ok1 = Result<CommandStub, IFormattable>
                    .Ok(result1);
                var ok2 = Result<CommandStub, IFormattable>
                    .Ok(result2);

                A.CallTo(() => inner.HandleAsync(A<CommandStub>._, token))
                    .Returns(ok1).Once().Then.Returns(ok2);

                /* Act */
                var results = await sut.HandleAsync(commands, token);

                /* Assert */
                Assert.Collection(results.Unwrap(),
                    v => Assert.Same(result1, v),
                    v => Assert.Same(result2, v),
                    v => Assert.Same(result1, v)
                );
            }
        }
    }
}
