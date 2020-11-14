namespace BusinessApp.App.UnitTest
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using BusinessApp.Domain;
    using FakeItEasy;
    using Xunit;

    public class GroupedBatchRequestDecoratorTests
    {
        private readonly CancellationToken token;
        private readonly GroupedBatchRequestDecorator<CommandStub, CommandStub> sut;
        private readonly ICommandHandler<IEnumerable<CommandStub>> inner;
        private readonly IBatchGrouper<CommandStub> grouper;

        public GroupedBatchRequestDecoratorTests()
        {
            token = A.Dummy<CancellationToken>();
            inner = A.Fake<ICommandHandler<IEnumerable<CommandStub>>>();
            grouper = A.Fake<IBatchGrouper<CommandStub>>();

            sut = new GroupedBatchRequestDecorator<CommandStub, CommandStub>(grouper, inner);
        }

        public class Constructor : GroupedBatchRequestDecoratorTests
        {
            public static IEnumerable<object[]> InvalidCtorArgs => new[]
            {
                new object[]
                {
                    null,
                    A.Dummy<ICommandHandler<IEnumerable<CommandStub>>>()
                },
                new object[]
                {
                    A.Dummy<IBatchGrouper<CommandStub>>(),
                    null
                },
            };

            [Theory, MemberData(nameof(InvalidCtorArgs))]
            public void InvalidCtorArgs_ExceptionThrown(
                IBatchGrouper<CommandStub> g,
                ICommandHandler<IEnumerable<CommandStub>> i)
            {
                /* Arrange */
                void shouldThrow() => new GroupedBatchRequestDecorator<CommandStub, CommandStub>(g, i);

                /* Act */
                var ex = Record.Exception(shouldThrow);

                /* Assert */
                Assert.IsType<BadStateException>(ex);
            }
        }

        public class HandleAsync : GroupedBatchRequestDecoratorTests
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
            public async Task WithCommand_HandlerCalledPerGroup()
            {
                /* Arrange */
                var groups = new[]
                {
                    new[] { A.Dummy<CommandStub>(), A.Dummy<CommandStub>() },
                    new[] { A.Dummy<CommandStub>() },
                };
                var commands = A.Dummy<IEnumerable<CommandStub>>();
                var firstPayload = new ConcurrentBag<IEnumerable<CommandStub>>();
                var secondPayload = new ConcurrentBag<IEnumerable<CommandStub>>();
                A.CallTo(() => grouper.GroupAsync(commands, token)).Returns(groups);
                A.CallTo(() => inner.HandleAsync(groups.First(), token))
                    .Invokes(ctx => firstPayload.Add(ctx.GetArgument<IEnumerable<CommandStub>>(0)));
                A.CallTo(() => inner.HandleAsync(groups.Last(), token))
                    .Invokes(ctx => secondPayload.Add(ctx.GetArgument<IEnumerable<CommandStub>>(0)));

                /* Act */
                await sut.HandleAsync(commands, token);

                /* Assert */
                Assert.Collection(firstPayload,
                    p => Assert.Same(p, groups.First()));
                Assert.Collection(secondPayload,
                    p => Assert.Same(p, groups.Last()));
            }

            [Fact]
            public async Task OriginalCommandNotFound_ExceptionThrown()
            {
                /* Arrange */
                var groups = new[]
                {
                    new[] { new CommandStub() }
                };
                var commands = new[]
                {
                    new CommandStub()
                };
                A.CallTo(() => grouper.GroupAsync(commands, token)).Returns(groups);

                /* Act */
                var ex = await Record.ExceptionAsync(() => sut.HandleAsync(commands, token));

                /* Assert */
                Assert.IsType<BusinessAppAppException>(ex);
                Assert.Equal(
                    "Could not find the original command after " +
                    "it was grouped. Consider overriding Equals if the batch grouper " +
                    "creates new classes.",
                    ex.Message
                );
            }

            public class OnError : GroupedBatchRequestDecoratorTests
            {
                private readonly Result<IEnumerable<CommandStub>, IFormattable> error;
                private readonly Result<IEnumerable<CommandStub>, IFormattable> ok;
                private readonly IEnumerable<CommandStub> commands;

                public OnError()
                {
                    commands = new[]
                    {
                        new CommandStub(),
                        new CommandStub(),
                        new CommandStub(),
                    };
                    var groups = new[]
                    {
                        new[] { commands.Last(), commands.First() },
                        new[] { commands.ElementAt(1) },
                    };

                    error = Result<IEnumerable<CommandStub>, IFormattable>
                        .Error(DateTime.Now);
                    ok = Result<IEnumerable<CommandStub>, IFormattable>
                        .Ok(A.Dummy<IEnumerable<CommandStub>>());
                    A.CallTo(() => grouper.GroupAsync(commands, token)).Returns(groups);
                    A.CallTo(() => inner.HandleAsync(A<IEnumerable<CommandStub>>._, token))
                        .Returns(error).Once().Then.Returns(ok);
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
                        r => Assert.Equal(error.UnwrapError(), r.Into().UnwrapError()),
                        r => Assert.Equal(ValueKind.Ok, r.Kind),
                        r => Assert.Equal(error.UnwrapError(), r.Into().UnwrapError())
                    );
                }
            }

            [Fact]
            public async Task AllOkResults_ResultsReturned()
            {
                /* Arrange */
                var commands = new[]
                {
                    new CommandStub(),
                    new CommandStub(),
                    new CommandStub(),
                };
                var groups = new[]
                {
                    new[] { commands.Last(), commands.First() },
                    new[] { commands.ElementAt(1) },
                };
                var result1 = new[]
                {
                    new CommandStub(),
                };
                var result2 = new[]
                {
                    new CommandStub()
                };

                var ok1 = Result<IEnumerable<CommandStub>, IFormattable>
                    .Ok(result1);
                var ok2 = Result<IEnumerable<CommandStub>, IFormattable>
                    .Ok(result2);

                A.CallTo(() => grouper.GroupAsync(commands, token)).Returns(groups);
                A.CallTo(() => inner.HandleAsync(A<IEnumerable<CommandStub>>._, token))
                    .Returns(ok1).Once().Then.Returns(ok2);

                /* Act */
                var results = await sut.HandleAsync(commands, token);

                /* Assert */
                Assert.Collection(results.Unwrap(),
                    v => Assert.Same(result1.First(), v),
                    v => Assert.Same(result2.First(), v),
                    v => Assert.Same(result1.First(), v)
                );
            }
        }
    }
}