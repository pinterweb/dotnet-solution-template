namespace BusinessApp.App.UnitTest
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using FakeItEasy;
    using Xunit;
    using System.Threading;
    using BusinessApp.Domain;

    public class ValidationBatchCommandDecoratorTests
    {
        private readonly CancellationToken token;
        private readonly ValidationBatchCommandDecorator<CommandStub> sut;
        private readonly ICommandHandler<IEnumerable<CommandStub>> inner;
        private readonly IValidator<CommandStub> validator;

        public ValidationBatchCommandDecoratorTests()
        {
            token = A.Dummy<CancellationToken>();
            inner = A.Fake<ICommandHandler<IEnumerable<CommandStub>>>();
            validator = A.Fake<IValidator<CommandStub>>();

            sut = new ValidationBatchCommandDecorator<CommandStub>(validator, inner);
        }

        public class Constructor : ValidationBatchCommandDecoratorTests
        {
            public static IEnumerable<object[]> InvalidCtorArgs => new[]
            {
                new object[] { null, A.Dummy<ICommandHandler<IEnumerable<CommandStub>>>() },
                new object[] { A.Fake<IValidator<CommandStub>>(), null },
            };

            [Theory, MemberData(nameof(InvalidCtorArgs))]
            public void InvalidCtorArgs_ExceptionThrown(
                IValidator<CommandStub> v,
                ICommandHandler<IEnumerable<CommandStub>> c)
            {
                /* Arrange */
                void shouldThrow() => new ValidationBatchCommandDecorator<CommandStub>(v, c);

                /* Act */
                var ex = Record.Exception(shouldThrow);

                /* Assert */
                Assert.IsType<BadStateException>(ex);
            }
        }

        public class HandleAsync : ValidationBatchCommandDecoratorTests
        {
            [Fact]
            public async Task WithoutCommandArg_ExceptionThrown()
            {
                /* Arrange */
                Task shouldthrow() => sut.HandleAsync(null, A.Dummy<CancellationToken>());

                /* Act */
                var ex = await Record.ExceptionAsync(shouldthrow);

                /* Assert */
                Assert.NotNull(ex);
            }

            [Fact]
            public async Task WithCommandArg_ValidationCalledBeforeHandeler()
            {
                /* Arrange */
                var handlerCallsBeforeValidate = 0;
                var commands = new[] { A.Dummy<CommandStub>(), A.Dummy<CommandStub>() };
                A.CallTo(() => validator.ValidateAsync(A<CommandStub>._, token))
                    .Invokes(ctx => handlerCallsBeforeValidate += Fake.GetCalls(inner).Count());

                /* Act */
                await sut.HandleAsync(commands, token);

                /* Assert */
                Assert.Equal(0, handlerCallsBeforeValidate);
            }

            [Fact]
            public async Task WithTwoCommandArgss_ValidatedTwice()
            {
                /* Arrange */
                var commands = new[] { A.Dummy<CommandStub>(), A.Dummy<CommandStub>() };

                /* Act */
                await sut.HandleAsync(commands, token);

                /* Assert */
                A.CallTo(() => validator.ValidateAsync(commands.First(), token))
                    .MustHaveHappenedOnceExactly().Then(
                        A.CallTo(() => validator.ValidateAsync(commands.Last(), token))
                            .MustHaveHappenedOnceExactly());
            }

            [Fact]
            public async Task WithTwoCommandArgs_HandledOnce()
            {
                /* Arrange */
                var commands = new[] { A.Dummy<CommandStub>(), A.Dummy<CommandStub>() };

                /* Act */
                await sut.HandleAsync(commands, token);

                /* Assert */
                A.CallTo(() => inner.HandleAsync(commands, token))
                    .MustHaveHappenedOnceExactly();
            }

            [Fact]
            public async Task ModelValidationExceptionThrown_MemberNameHasIndex()
            {
                /* Arrange */
                var commands = new[] { A.Dummy<CommandStub>(), A.Dummy<CommandStub>() };
                var memberError = new MemberValidationException("foo", A.CollectionOfDummy<string>(1));

                A.CallTo(() => validator.ValidateAsync(commands.Last(), token))
                    .Throws(new ModelValidationException("foomsg", new[] { memberError }));

                /* Act */
                var ex = await Record.ExceptionAsync(() => sut.HandleAsync(commands, token));

                /* Assert */
                var modelError = Assert.IsType<ModelValidationException>(ex);
                Assert.Collection(modelError,
                    e => Assert.Equal("[1].foo", e.MemberName)
                );
            }

            [Fact]
            public async Task ModelValidationExceptionThrown_SameErrorsUsed()
            {
                /* Arrange */
                var commands = new[] { A.Dummy<CommandStub>(), A.Dummy<CommandStub>() };
                var memberError = new MemberValidationException("foo", new[] { "bar" });

                A.CallTo(() => validator.ValidateAsync(commands.First(), token))
                    .Throws(new ModelValidationException("foomsg", new[] { memberError }));

                /* Act */
                var ex = await Record.ExceptionAsync(() => sut.HandleAsync(commands, token));

                /* Assert */
                var modelError = Assert.IsType<ModelValidationException>(ex);
                Assert.Collection(modelError,
                    e => Assert.Equal(new[] { "bar" }, e.Errors)
                );
            }

            [Fact]
            public async Task AggregateExceptionThrown_HasMultipleModelValidationExceptions()
            {
                /* Arrange */
                var commands = new[] { A.Dummy<CommandStub>(), A.Dummy<CommandStub>() };

                A.CallTo(() => validator.ValidateAsync(commands.Last(), token))
                    .Throws(new AggregateException(new[]
                    {
                        new ModelValidationException("foo", A.CollectionOfDummy<MemberValidationException>(1)),
                        new ModelValidationException("lorem", A.CollectionOfDummy<MemberValidationException>(1)),
                    }));

                /* Act */
                var ex = await Record.ExceptionAsync(() => sut.HandleAsync(commands, token));

                /* Assert */
                var aggregate = Assert.IsType<AggregateException>(ex);
                Assert.Equal(2, aggregate.Flatten().InnerExceptions.Count());
            }

            [Fact]
            public async Task AggregateExceptionThrown_MemberNamesHasIndex()
            {
                /* Arrange */
                var commands = new[] { A.Dummy<CommandStub>(), A.Dummy<CommandStub>() };

                A.CallTo(() => validator.ValidateAsync(commands.Last(), token))
                    .Throws(new AggregateException(new[]
                    {
                        new ModelValidationException(
                            "foomsg",
                            new[] { new MemberValidationException("foo", A.CollectionOfDummy<string>(1)) }
                        ),
                        new ModelValidationException(
                            "loremmsg",
                            new[] { new MemberValidationException("lorem", A.CollectionOfDummy<string>(1)) }
                        ),
                    }));

                /* Act */
                var ex = await Record.ExceptionAsync(() => sut.HandleAsync(commands, token));

                /* Assert */
                var aggregate = Assert.IsType<AggregateException>(ex);
                Assert.Collection(
                    aggregate.Flatten().InnerExceptions,
                    i => Assert.Contains("[1].foo", Assert.IsType<ModelValidationException>(i).Select(m => m.MemberName)),
                    i => Assert.Contains("[1].lorem", Assert.IsType<ModelValidationException>(i).Select(m => m.MemberName))
                );
            }

            [Fact]
            public async Task AggregateExceptionThrown_SameMessageUsed()
            {
                /* Arrange */
                var commands = new[] { A.Dummy<CommandStub>(), A.Dummy<CommandStub>() };

                A.CallTo(() => validator.ValidateAsync(commands.Last(), token))
                    .Throws(new AggregateException(new[]
                    {
                        new ModelValidationException("bar", A.CollectionOfDummy<MemberValidationException>(1)),
                        new ModelValidationException("ipsum", A.CollectionOfDummy<MemberValidationException>(1)),
                    }));

                /* Act */
                var ex = await Record.ExceptionAsync(() => sut.HandleAsync(commands, token));

                /* Assert */
                var aggregate = Assert.IsType<AggregateException>(ex);
                Assert.Collection(
                    aggregate.Flatten().InnerExceptions,
                    i => Assert.Contains("bar", i.Message),
                    i => Assert.Contains("ipsum", i.Message));
            }
        }
    }
}
