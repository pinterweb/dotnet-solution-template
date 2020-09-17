namespace BusinessApp.App.UnitTest
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using FakeItEasy;
    using Xunit;

    public class BatchCommandHandlerTests
    {
        private readonly CancellationToken token;
        private readonly BatchCommandHandler<CommandStub> sut;
        private readonly ICommandHandler<CommandStub> inner;

        public BatchCommandHandlerTests()
        {
            token = A.Dummy<CancellationToken>();
            inner = A.Fake<ICommandHandler<CommandStub>>();

            sut = new BatchCommandHandler<CommandStub>(inner);
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
                void shouldThrow() => new BatchCommandHandler<CommandStub>(c);

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
                Task shouldthrow() => sut.HandleAsync(null, token);

                /* Act */
                var ex = await Record.ExceptionAsync(shouldthrow);

                /* Assert */
                Assert.NotNull(ex);
            }

            [Fact]
            public async Task WithCommands_EachCalled()
            {
                /* Arrange */
                var commands = new[] { A.Dummy<CommandStub>(), A.Dummy<CommandStub>() };

                /* Act */
                await sut.HandleAsync(commands, token);

                /* Assert */
                A.CallTo(() => inner.HandleAsync(commands.First(), token)).MustHaveHappenedOnceExactly();
                A.CallTo(() => inner.HandleAsync(commands.Last(), token)).MustHaveHappenedOnceExactly();
            }

            [Fact]
            public async Task ExceptionThrown_IndexAddedToDataKey()
            {
                /* Arrange */
                var commands = new[] { A.Dummy<CommandStub>(), A.Dummy<CommandStub>() };
                var thrownException = new Exception();
                thrownException.Data.Add("foo", "bar");

                A.CallTo(() => inner.HandleAsync(commands.Last(), token)).Throws(thrownException);

                /* Act */
                var ex = await Record.ExceptionAsync(() => sut.HandleAsync(commands, token));

                /* Assert */
                Assert.Collection(ex.Data.Cast<DictionaryEntry>(),
                    d =>
                    {
                        Assert.Equal("[1].foo", d.Key);
                        Assert.Equal("bar", d.Value);
                    },
                    d =>
                    {
                        Assert.Equal("Index", d.Key);
                        Assert.Equal(1, d.Value);
                    });
            }

            [Fact]
            public async Task WithInnerExceptions_IndexAddedToInnerException()
            {
                /* Arrange */
                var commands = new[] { A.Dummy<CommandStub>(), A.Dummy<CommandStub>() };
                var thrownInner = new Exception("foo");
                var thrownException = new Exception("bar", thrownInner);
                thrownException.Data.Add("lorem", "ipsum");
                thrownInner.Data.Add("dolor", "ipsit");

                A.CallTo(() => inner.HandleAsync(commands.Last(), token)).Throws(thrownException);

                /* Act */
                var ex = await Record.ExceptionAsync(() => sut.HandleAsync(commands, token));

                /* Assert */
                Assert.Collection(ex.InnerException.Data.Cast<DictionaryEntry>(),
                    d =>
                    {
                        Assert.Equal("[1].dolor", d.Key);
                        Assert.Equal("ipsit", d.Value);
                    },
                    d =>
                    {
                        Assert.Equal("Index", d.Key);
                        Assert.Equal(1, d.Value);
                    });
            }

            [Fact]
            public async Task MultipleExceptions_AggregateExceptionThrown()
            {
                /* Arrange */
                var commands = new[] { A.Dummy<CommandStub>(), A.Dummy<CommandStub>() };
                var firstEx = new Exception("ispit");
                var secondEx = new Exception("ispit");
                firstEx.Data.Add("foo", "bar");
                secondEx.Data.Add("lorem", "ipsum");

                A.CallTo(() => inner.HandleAsync(commands.First(), token)).Throws(firstEx);
                A.CallTo(() => inner.HandleAsync(commands.Last(), token)).Throws(secondEx);

                /* Act */
                var ex = await Record.ExceptionAsync(() => sut.HandleAsync(commands, token));

                /* Assert */
                var manyErrs = Assert.IsType<AggregateException>(ex);
                Assert.Collection(manyErrs.InnerExceptions.SelectMany(e => e.Data.Cast<DictionaryEntry>()),
                    d =>
                    {
                        Assert.Equal("[0].foo", d.Key);
                        Assert.Equal("bar", d.Value);
                    },
                    d =>
                    {
                        Assert.Equal("Index", d.Key);
                        Assert.Equal(0, d.Value);
                    },
                    d =>
                    {
                        Assert.Equal("[1].lorem", d.Key);
                        Assert.Equal("ipsum", d.Value);
                    },
                    d =>
                    {
                        Assert.Equal("Index", d.Key);
                        Assert.Equal(1, d.Value);
                    });
            }
        }
    }
}
