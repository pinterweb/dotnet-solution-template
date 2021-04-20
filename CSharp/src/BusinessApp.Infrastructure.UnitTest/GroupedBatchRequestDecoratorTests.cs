using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BusinessApp.Kernel;
using FakeItEasy;
using Xunit;

namespace BusinessApp.Infrastructure.UnitTest
{
    public class GroupedBatchRequestDecoratorTests
    {
        private readonly CancellationToken cancelToken;
        private readonly GroupedBatchRequestDecorator<CommandStub, CommandStub> sut;
        private readonly IRequestHandler<IEnumerable<CommandStub>, IEnumerable<CommandStub>> inner;
        private readonly IBatchGrouper<CommandStub> grouper;

        public GroupedBatchRequestDecoratorTests()
        {
            cancelToken = A.Dummy<CancellationToken>();
            inner = A.Fake<IRequestHandler<IEnumerable<CommandStub>, IEnumerable<CommandStub>>>();
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
                    A.Dummy<IRequestHandler<IEnumerable<CommandStub>, IEnumerable<CommandStub>>>()
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
                IRequestHandler<IEnumerable<CommandStub>, IEnumerable<CommandStub>> i)
            {
                /* Arrange */
                void shouldThrow() => new GroupedBatchRequestDecorator<CommandStub, CommandStub>(g, i);

                /* Act */
                var ex = Record.Exception(shouldThrow);

                /* Assert */
                Assert.IsType<BusinessAppException>(ex);
            }
        }

        public class HandleAsync : GroupedBatchRequestDecoratorTests
        {
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
                A.CallTo(() => grouper.GroupAsync(commands, cancelToken)).Returns(groups);
                A.CallTo(() => inner.HandleAsync(groups.First(), cancelToken))
                    .Invokes(ctx => firstPayload.Add(ctx.GetArgument<IEnumerable<CommandStub>>(0)));
                A.CallTo(() => inner.HandleAsync(groups.Last(), cancelToken))
                    .Invokes(ctx => secondPayload.Add(ctx.GetArgument<IEnumerable<CommandStub>>(0)));

                /* Act */
                await sut.HandleAsync(commands, cancelToken);

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
                A.CallTo(() => grouper.GroupAsync(commands, cancelToken)).Returns(groups);

                /* Act */
                var ex = await Record.ExceptionAsync(() => sut.HandleAsync(commands, cancelToken));

                /* Assert */
                Assert.IsType<BusinessAppException>(ex);
                Assert.Equal(
                    "Could not find the original command(s) after " +
                    "it was grouped. Consider overriding Equals if the batch grouper " +
                    "creates new classes.",
                    ex.Message
                );
            }

            public class OnError : GroupedBatchRequestDecoratorTests
            {
                private readonly Exception errorException;
                private readonly Result<IEnumerable<CommandStub>, Exception> error;
                private readonly Result<IEnumerable<CommandStub>, Exception> ok;
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

                    errorException = new Exception();
                    error = Result.Error<IEnumerable<CommandStub>>(errorException);
                    ok = Result.Ok<IEnumerable<CommandStub>>(new[] { commands.ElementAt(1) });

                    A.CallTo(() => grouper.GroupAsync(commands, cancelToken)).Returns(groups);
                    A.CallTo(() => inner.HandleAsync(A<IEnumerable<CommandStub>>._, cancelToken))
                        .Returns(error).Once().Then.Returns(ok);
                }

                [Fact]
                public async Task AllReturnedInBatchException()
                {
                    /* Act */
                    var results = await sut.HandleAsync(commands, cancelToken);

                    /* Assert */
                    Assert.IsType<BatchException>(results.UnwrapError());
                }

                [Fact]
                public async Task AllReturnedInBatchExceptionInOrder()
                {
                    /* Act */
                    var results = await sut.HandleAsync(commands, cancelToken);

                    /* Assert */
                    var ex = Assert.IsType<BatchException>(results.UnwrapError());
                    Assert.Collection(ex,
                        r => Assert.Equal(Result.Error<object>(errorException), r),
                        r => Assert.Equal(Result.Ok<object>(ok.Unwrap()), r),
                        r => Assert.Equal(Result.Error<object>(errorException), r)
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

                var ok1 = Result.Ok<IEnumerable<CommandStub>>(result1);
                var ok2 = Result.Ok<IEnumerable<CommandStub>>(result2);

                A.CallTo(() => grouper.GroupAsync(commands, cancelToken)).Returns(groups);
                A.CallTo(() => inner.HandleAsync(A<IEnumerable<CommandStub>>._, cancelToken))
                    .Returns(ok1).Once().Then.Returns(ok2);

                /* Act */
                var results = await sut.HandleAsync(commands, cancelToken);

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
