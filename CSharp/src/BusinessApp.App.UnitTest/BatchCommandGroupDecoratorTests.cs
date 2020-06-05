namespace BusinessApp.App.UnitTest
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using FakeItEasy;
    using Xunit;

    public class BatchCommandGroupDecoratorTests
    {
        private readonly CancellationToken token;
        private readonly BatchCommandGroupDecorator<DummyCommand> sut;
        private readonly ICommandHandler<IEnumerable<DummyCommand>> inner;
        private readonly IBatchGrouper<DummyCommand> grouper;

        public BatchCommandGroupDecoratorTests()
        {
            token = A.Dummy<CancellationToken>();
            inner = A.Fake<ICommandHandler<IEnumerable<DummyCommand>>>();
            grouper = A.Fake<IBatchGrouper<DummyCommand>>();

            sut = new BatchCommandGroupDecorator<DummyCommand>(grouper, inner);
        }

        public class Constructor : BatchCommandGroupDecoratorTests
        {
            public static IEnumerable<object[]> InvalidCtorArgs => new[]
            {
                new object[]
                {
                    A.Fake<IBatchGrouper<DummyCommand>>(),
                    null
                },
                new object[]
                {
                    A.Fake<IBatchGrouper<DummyCommand>>(),
                    null
                },
            };

            [Theory, MemberData(nameof(InvalidCtorArgs))]
            public void InvalidCtorArgs_ExceptionThrown(
                IBatchGrouper<DummyCommand> g,
                ICommandHandler<IEnumerable<DummyCommand>> i)
            {
                /* Arrange */
                void shouldThrow() => new BatchCommandGroupDecorator<DummyCommand>(g, i);

                /* Act */
                var ex = Record.Exception(shouldThrow);

                /* Assert */
                Assert.IsType<ArgumentNullException>(ex);
            }
        }

        public class HandleAsync : BatchCommandGroupDecoratorTests
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
                    new[] { A.Dummy<DummyCommand>(), A.Dummy<DummyCommand>() },
                    new[] { A.Dummy<DummyCommand>() },
                };
                var commands = A.Dummy<IEnumerable<DummyCommand>>();
                var payloads = new List<IEnumerable<DummyCommand>>();
                A.CallTo(() => grouper.GroupAsync(commands, token)).Returns(groups);
                A.CallTo(() => inner.HandleAsync(A<IEnumerable<DummyCommand>>._, token))
                    .Invokes(ctx => payloads.Add(ctx.GetArgument<IEnumerable<DummyCommand>>(0)));

                /* Act */
                await sut.HandleAsync(commands, token);

                /* Assert */
                Assert.Equal(2, payloads.Count());
                Assert.Collection(payloads.First(),
                    p => Assert.Contains(p, groups.First()),
                    p => Assert.Contains(p, groups.First()));
                Assert.Collection(payloads.Last(),
                    p => Assert.Contains(p, groups.Last()));
            }

            [Theory]
            [InlineData(0, 0, "[2].foo")]
            [InlineData(0, 1, "[0].foo")]
            [InlineData(1, 0, "[1].foo")]
            public async Task ExceptionWithIndexInDataKey_IndexChanged(int throwOnCommand,
                int exceptionIndexKey, string actualIndexKey)
            {
                /* Arrange */
                var exception = new Exception();
                exception.Data.Add("Index", exceptionIndexKey);
                exception.Data.Add("foo", "bar");
                var commands = new[]
                {
                    A.Dummy<DummyCommand>(),
                    A.Dummy<DummyCommand>(),
                    A.Dummy<DummyCommand>()
                };
                var groups = new[]
                {
                    new[] { commands.Last(), commands.First() },
                    new[] { commands.ElementAt(1) },
                };
                A.CallTo(() => grouper.GroupAsync(commands, token)).Returns(groups);
                A.CallTo(() => inner.HandleAsync(groups.ElementAt(throwOnCommand), token))
                    .Throws(exception);

                /* Act */
                var ex = await Record.ExceptionAsync(() => sut.HandleAsync(commands, token));

                /* Assert */
                Assert.NotNull(ex);
                Assert.True(ex.Data.Contains(actualIndexKey));
            }

            [Fact]
            public async Task AggregateExceptionWithIndexInDataKey_IndexChanged()
            {
                /* Arrange */
                var exception1 = new Exception();
                var exception2 = new Exception();
                var aggregate = new AggregateException(new [] { exception2, exception1 });
                exception1.Data.Add("Index", 1);
                exception1.Data.Add("foo", "bar");
                exception2.Data.Add("Index", 0);
                exception2.Data.Add("lorem", "ipsum");
                var commands = new[]
                {
                    A.Dummy<DummyCommand>(),
                    A.Dummy<DummyCommand>(),
                    A.Dummy<DummyCommand>()
                };
                var groups = new[]
                {
                    new[] { commands.Last(), commands.First() },
                };
                A.CallTo(() => grouper.GroupAsync(commands, token)).Returns(groups);
                A.CallTo(() => inner.HandleAsync(groups.ElementAt(0), token))
                    .Throws(aggregate);

                /* Act */
                var ex = await Record.ExceptionAsync(() => sut.HandleAsync(commands, token));

                /* Assert */
                var thrownAggregate = Assert.IsType<AggregateException>(ex);
                Assert.Collection(thrownAggregate.InnerExceptions,
                    e => Assert.True(e.Data.Contains("[2].lorem")),
                    e => Assert.True(e.Data.Contains("[0].foo")));
            }

            [Theory]
            [InlineData(0, 0, "[0].foo", "[2].foo")]
            [InlineData(0, 1, "[1].foo", "[0].foo")]
            [InlineData(1, 0, "[0].foo", "[1].foo")]
            public async Task ExceptionWithIndexedDataKey_IndexChanged(int throwOnCommand,
                int innerHandlerIndex, string exceptionIndexKey, string actualIndexKey)
            {
                /* Arrange */
                var exception = new Exception();
                exception.Data.Add("Index", innerHandlerIndex);
                exception.Data.Add(exceptionIndexKey, "bar message");
                var commands = new[]
                {
                    A.Dummy<DummyCommand>(),
                    A.Dummy<DummyCommand>(),
                    A.Dummy<DummyCommand>()
                };
                var groups = new[]
                {
                    new[] { commands.Last(), commands.First() },
                    new[] { commands.ElementAt(1) },
                };
                A.CallTo(() => grouper.GroupAsync(commands, token)).Returns(groups);
                A.CallTo(() => inner.HandleAsync(groups.ElementAt(throwOnCommand), token))
                    .Throws(exception);

                /* Act */
                var ex = await Record.ExceptionAsync(() => sut.HandleAsync(commands, token));

                /* Assert */
                Assert.NotNull(ex);
                Assert.True(ex.Data.Contains(actualIndexKey));
            }

            [Fact]
            public async Task AggregateExceptionWithIndexedDataKey_IndexChanged()
            {
                /* Arrange */
                var exception1 = new Exception();
                var exception2 = new Exception();
                var aggregate = new AggregateException(new [] { exception2, exception1 });
                exception1.Data.Add("[1].foo", "bar");
                exception2.Data.Add("[0].lorem", "ipsum");
                var commands = new[]
                {
                    A.Dummy<DummyCommand>(),
                    A.Dummy<DummyCommand>(),
                    A.Dummy<DummyCommand>()
                };
                var groups = new[]
                {
                    new[] { commands.Last(), commands.First() },
                };
                A.CallTo(() => grouper.GroupAsync(commands, token)).Returns(groups);
                A.CallTo(() => inner.HandleAsync(groups.ElementAt(0), token))
                    .Throws(aggregate);

                /* Act */
                var ex = await Record.ExceptionAsync(() => sut.HandleAsync(commands, token));

                /* Assert */
                var thrownAggregate = Assert.IsType<AggregateException>(ex);
                Assert.Collection(thrownAggregate.InnerExceptions,
                    e => Assert.True(e.Data.Contains("[2].lorem")),
                    e => Assert.True(e.Data.Contains("[0].foo")));
            }

            [Fact]
            public async Task ExceptionWithoutIndexNOrIndexKey_DoesNothing()
            {
                /* Arrange */
                var exception = new Exception();
                exception.Data.Add("foo", "bar");
                exception.Data.Add("lorem", "ipsum");
                var commands = new[]
                {
                    A.Dummy<DummyCommand>(), A.Dummy<DummyCommand>()
                };
                var groups = new[]
                {
                    new[] { commands.First(), commands.Last() }
                };
                A.CallTo(() => grouper.GroupAsync(commands, token)).Returns(groups);
                A.CallTo(() => inner.HandleAsync(groups.ElementAt(0), token))
                    .Throws(exception);

                /* Act */
                var ex = await Record.ExceptionAsync(() => sut.HandleAsync(commands, token));

                /* Assert */
                Assert.NotNull(ex);
                Assert.Equal(2, ex.Data.Count);
                Assert.True(ex.Data.Contains("foo"));
                Assert.Equal("bar", ex.Data["foo"]);
                Assert.True(ex.Data.Contains("lorem"));
                Assert.Equal("ipsum", ex.Data["lorem"]);
            }

            [Fact]
            public async Task AggregateExceptionWithoutIndexNOrIndexKey_DoesNothing()
            {
                /* Arrange */
                var exception1 = new Exception();
                var exception2 = new Exception();
                var aggregate = new AggregateException(new [] { exception2, exception1 });
                exception1.Data.Add("lorem", "ipsum");
                exception2.Data.Add("foo", "bar");
                var commands = new[]
                {
                    A.Dummy<DummyCommand>(), A.Dummy<DummyCommand>()
                };
                var groups = new[]
                {
                    new[] { commands.First(), commands.Last() }
                };
                A.CallTo(() => grouper.GroupAsync(commands, token)).Returns(groups);
                A.CallTo(() => inner.HandleAsync(groups.ElementAt(0), token))
                    .Throws(aggregate);

                /* Act */
                var ex = await Record.ExceptionAsync(() => sut.HandleAsync(commands, token));

                /* Assert */
                var thrownAggregate = Assert.IsType<AggregateException>(ex);
                Assert.Collection(thrownAggregate.InnerExceptions,
                    e => Assert.True(e.Data.Contains("foo")),
                    e => Assert.True(e.Data.Contains("lorem")));
            }

            [Fact]
            public async Task MultipleExceptions_ThrowsAsAggregate()
            {
                /* Arrange */
                var exception1 = new Exception();
                var exception2 = new Exception();
                var commands = new[]
                {
                    A.Dummy<DummyCommand>(),
                    A.Dummy<DummyCommand>(),
                };
                var groups = new[]
                {
                    new[] { commands.First() },
                    new[] { commands.Last() }
                };
                A.CallTo(() => grouper.GroupAsync(commands, token)).Returns(groups);
                A.CallTo(() => inner.HandleAsync(groups.ElementAt(0), token))
                    .Throws(exception1);
                A.CallTo(() => inner.HandleAsync(groups.ElementAt(1), token))
                    .Throws(exception2);

                /* Act */
                var ex = await Record.ExceptionAsync(() => sut.HandleAsync(commands, token));

                /* Assert */
                var multiErr = Assert.IsType<AggregateException>(ex);
                Assert.Collection(multiErr.InnerExceptions,
                    e => Assert.Same(exception1, e),
                    e => Assert.Same(exception2, e));
            }

            [Fact]
            public async Task OneException_ThrowsIt()
            {
                /* Arrange */
                var exception = new Exception();
                var commands = new[]
                {
                    A.Dummy<DummyCommand>(),
                };
                var groups = new[]
                {
                    new[] { commands.First() }
                };
                A.CallTo(() => grouper.GroupAsync(commands, token)).Returns(groups);
                A.CallTo(() => inner.HandleAsync(groups.ElementAt(0), token))
                    .Throws(exception);

                /* Act */
                var ex = await Record.ExceptionAsync(() => sut.HandleAsync(commands, token));

                /* Assert */
                Assert.Same(exception, ex);
            }
        }
    }
}
