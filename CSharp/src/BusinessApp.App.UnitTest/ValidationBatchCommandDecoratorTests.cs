namespace BusinessApp.App.UnitTest
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using FakeItEasy;
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
                A.CallTo(() => validator.ValidateAsync(A<DummyCommand>._))
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
                A.CallTo(() => validator.ValidateAsync(commands.First()))
                    .MustHaveHappenedOnceExactly().Then(
                        A.CallTo(() => validator.ValidateAsync(commands.Last()))
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

            [Fact]
            public async Task ValidationException_MemberNameHasIndex()
            {
                /* Arrange */
                var commands = new[] { A.Dummy<DummyCommand>(), A.Dummy<DummyCommand>() };

                A.CallTo(() => validator.ValidateAsync(commands.Last()))
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

                A.CallTo(() => validator.ValidateAsync(commands.First()))
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

                A.CallTo(() => validator.ValidateAsync(commands.Last()))
                    .Throws(new ValidationException("foo", "bar", innerException));

                /* Act */
                var ex = await Record.ExceptionAsync(() =>
                    sut.HandleAsync(commands, A.Dummy<CancellationToken>()));

                /* Assert */
                var invalid = Assert.IsType<ValidationException>(ex);
                Assert.Same(innerException, invalid.InnerException);
            }

            [Fact]
            public async Task AggregateException_HasMultipleValidationExceptions()
            {
                /* Arrange */
                var commands = new[] { A.Dummy<DummyCommand>(), A.Dummy<DummyCommand>() };

                A.CallTo(() => validator.ValidateAsync(commands.Last()))
                    .Throws(new AggregateException(new[]
                            {
                                new ValidationException("foo", "bar"),
                                new ValidationException("lorem", "ipsum"),
                            }));

                /* Act */
                var ex = await Record.ExceptionAsync(() =>
                    sut.HandleAsync(commands, A.Dummy<CancellationToken>()));

                /* Assert */
                var aggregate = Assert.IsType<AggregateException>(ex);
                Assert.Equal(2, aggregate.Flatten().InnerExceptions.Count());
            }

            [Fact]
            public async Task AggregateException_MemberNamesHasIndex()
            {
                /* Arrange */
                var commands = new[] { A.Dummy<DummyCommand>(), A.Dummy<DummyCommand>() };

                A.CallTo(() => validator.ValidateAsync(commands.Last()))
                    .Throws(new AggregateException(new[]
                            {
                                new ValidationException("foo", "bar"),
                                new ValidationException("lorem", "ipsum"),
                            }));

                /* Act */
                var ex = await Record.ExceptionAsync(() =>
                    sut.HandleAsync(commands, A.Dummy<CancellationToken>()));

                /* Assert */
                var aggregate = Assert.IsType<AggregateException>(ex);
                Assert.Collection(
                    aggregate.Flatten().InnerExceptions,
                    i => Assert.Contains("[1].foo", Assert.IsType<ValidationException>(i).Result.MemberNames),
                    i => Assert.Contains("[1].lorem", Assert.IsType<ValidationException>(i).Result.MemberNames));
            }

            [Fact]
            public async Task AggregateException_SameMessageUsed()
            {
                /* Arrange */
                var commands = new[] { A.Dummy<DummyCommand>(), A.Dummy<DummyCommand>() };

                A.CallTo(() => validator.ValidateAsync(commands.Last()))
                    .Throws(new AggregateException(new[]
                            {
                                new ValidationException("foo", "bar"),
                                new ValidationException("lorem", "ipsum"),
                            }));

                /* Act */
                var ex = await Record.ExceptionAsync(() =>
                    sut.HandleAsync(commands, A.Dummy<CancellationToken>()));

                /* Assert */
                var aggregate = Assert.IsType<AggregateException>(ex);
                Assert.Collection(
                    aggregate.Flatten().InnerExceptions,
                    i => Assert.Contains("bar", i.Message),
                    i => Assert.Contains("ipsum", i.Message));
            }

            [Fact]
            public async Task AggregateException_ValidationException_InnerExceptionUsed()
            {
                /* Arrange */
                var firstInner = new Exception();
                var secondInner = new Exception();
                var commands = new[] { A.Dummy<DummyCommand>(), A.Dummy<DummyCommand>() };

                A.CallTo(() => validator.ValidateAsync(commands.Last()))
                    .Throws(new AggregateException(new[]
                            {
                                new ValidationException("foo", "bar", firstInner),
                                new ValidationException("lorem", "ipsum", secondInner),
                            }));

                /* Act */
                var ex = await Record.ExceptionAsync(() =>
                    sut.HandleAsync(commands, A.Dummy<CancellationToken>()));

                /* Assert */
                var aggregate = Assert.IsType<AggregateException>(ex);
                Assert.Collection(
                    aggregate.Flatten().InnerExceptions,
                    i => Assert.Same(firstInner, i.InnerException),
                    i => Assert.Same(secondInner, i.InnerException));
            }
        }
    }
}
