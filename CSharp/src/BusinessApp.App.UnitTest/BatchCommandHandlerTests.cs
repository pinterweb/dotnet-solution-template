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

            [Fact]
            public async Task ValidationException_MemberNameHasIndex()
            {
                /* Arrange */
                var commands = new[] { A.Dummy<DummyCommand>(), A.Dummy<DummyCommand>() };

                A.CallTo(() => inner.HandleAsync(commands.Last(), A<CancellationToken>._))
                    .Throws(new ValidationException("foo", "bar"));

                /* Act */
                var ex = await Record.ExceptionAsync(() =>
                    sut.HandleAsync(commands, A.Dummy<CancellationToken>()));

                /* Assert */
                var invalid = Assert.IsType<ValidationException>(ex);
                var member = Assert.Single(invalid.Result.MemberNames);
                Assert.Equal("[1].foo", member);
            }

            [Fact]
            public async Task ValidationException_SameMessageUsed()
            {
                /* Arrange */
                var commands = new[] { A.Dummy<DummyCommand>(), A.Dummy<DummyCommand>() };

                A.CallTo(() => inner.HandleAsync(commands.Last(), A<CancellationToken>._))
                    .Throws(new ValidationException("foo", "bar"));

                /* Act */
                var ex = await Record.ExceptionAsync(() =>
                    sut.HandleAsync(commands, A.Dummy<CancellationToken>()));

                /* Assert */
                var invalid = Assert.IsType<ValidationException>(ex);
                Assert.Equal("bar", invalid.Result.ErrorMessage);
            }

            [Fact]
            public async Task ValidationException_InnerExceptionUsed()
            {
                /* Arrange */
                var commands = new[] { A.Dummy<DummyCommand>(), A.Dummy<DummyCommand>() };
                var innerException = new ValidationException("lorem", "ipsum");

                A.CallTo(() => inner.HandleAsync(commands.Last(), A<CancellationToken>._))
                    .Throws(new ValidationException("foo", "bar", innerException));

                /* Act */
                var ex = await Record.ExceptionAsync(() =>
                    sut.HandleAsync(commands, A.Dummy<CancellationToken>()));

                /* Assert */
                var invalid = Assert.IsType<ValidationException>(ex);
                Assert.Same(innerException, invalid.InnerException);
            }

            [Fact]
            public async Task SecurityException_MemberNameHasIndex()
            {
                /* Arrange */
                var commands = new[] { A.Dummy<DummyCommand>(), A.Dummy<DummyCommand>() };

                A.CallTo(() => inner.HandleAsync(commands.Last(), A<CancellationToken>._))
                    .Throws(new SecurityResourceException("foo", "bar"));

                /* Act */
                var ex = await Record.ExceptionAsync(() =>
                    sut.HandleAsync(commands, A.Dummy<CancellationToken>()));

                /* Assert */
                var unauthorized = Assert.IsType<SecurityResourceException>(ex);
                Assert.Equal("[1].foo", unauthorized.ResourceName);
            }

            [Fact]
            public async Task SecurityException_SameMessageUsed()
            {
                /* Arrange */
                var commands = new[] { A.Dummy<DummyCommand>(), A.Dummy<DummyCommand>() };

                A.CallTo(() => inner.HandleAsync(commands.First(), A<CancellationToken>._))
                    .Throws(new SecurityResourceException("foo", "bar"));

                /* Act */
                var ex = await Record.ExceptionAsync(() =>
                    sut.HandleAsync(commands, A.Dummy<CancellationToken>()));

                /* Assert */
                var unauthorized = Assert.IsType<SecurityResourceException>(ex);
                Assert.Equal("bar", unauthorized.Message);
            }

            [Fact]
            public async Task SecurityException_InnerExceptionUsed()
            {
                /* Arrange */
                var commands = new[] { A.Dummy<DummyCommand>(), A.Dummy<DummyCommand>() };
                var innerException = new SecurityResourceException("lorem", "ipsum");

                A.CallTo(() => inner.HandleAsync(commands.Last(), A<CancellationToken>._))
                    .Throws(new SecurityResourceException("foo", "bar", innerException));

                /* Act */
                var ex = await Record.ExceptionAsync(() =>
                    sut.HandleAsync(commands, A.Dummy<CancellationToken>()));

                /* Assert */
                var invalid = Assert.IsType<SecurityResourceException>(ex);
                Assert.Same(innerException, invalid.InnerException);
            }

            [Fact]
            public async Task MultipleExceptions_AggregateExceptionThrown()
            {
                /* Arrange */
                var commands = new[] { A.Dummy<DummyCommand>(), A.Dummy<DummyCommand>() };
                var firstEx = new ValidationException("foo", "bar");
                var secondEx = new SecurityResourceException("foo", "bar");

                A.CallTo(() => inner.HandleAsync(A<DummyCommand>._, A.Dummy<CancellationToken>()))
                    .Throws(firstEx).Once();
                A.CallTo(() => inner.HandleAsync(A<DummyCommand>._, A.Dummy<CancellationToken>()))
                    .Throws(secondEx).Once();

                /* Act */
                var ex = await Record.ExceptionAsync(() =>
                    sut.HandleAsync(commands, A.Dummy<CancellationToken>()));

                /* Assert */
                var aggregate = Assert.IsType<AggregateException>(ex);
                var innerExceptions = aggregate.Flatten().InnerExceptions;
                Assert.Single(innerExceptions, i => i is ValidationException);
                Assert.Single(innerExceptions, i => i is SecurityResourceException);
            }
        }
    }
}
