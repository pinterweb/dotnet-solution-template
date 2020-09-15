namespace BusinessApp.App.UnitTest
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using FakeItEasy;
    using Xunit;

    public class ApplicationScopeBatchDecoratorTests
    {
        private readonly CancellationToken token;
        private readonly ApplicationScopeBatchDecorator<CommandStub> sut;
        private readonly ICommandHandler<IEnumerable<CommandStub>> inner;
        private readonly IAppScope scope;

        public ApplicationScopeBatchDecoratorTests()
        {
            token = A.Dummy<CancellationToken>();
            inner = A.Fake<ICommandHandler<IEnumerable<CommandStub>>>();
            scope = A.Fake<IAppScope>();

            sut = new ApplicationScopeBatchDecorator<CommandStub>(scope, () => inner);
        }

        public class Constructor : ApplicationScopeBatchDecoratorTests
        {
            public static IEnumerable<object[]> InvalidCtorArgs => new[]
            {
                new object[]
                {
                    A.Dummy<IAppScope>(),
                    null
                },
                new object[]
                {
                    null,
                    A.Dummy<Func<ICommandHandler<IEnumerable<CommandStub>>>>(),
                },
            };

            [Theory, MemberData(nameof(InvalidCtorArgs))]
            public void InvalidCtorArgs_ExceptionThrown(IAppScope a,
                Func<ICommandHandler<IEnumerable<CommandStub>>> f)
            {
                /* Arrange */
                void shouldThrow() => new ApplicationScopeBatchDecorator<CommandStub>(a, f);

                /* Act */
                var ex = Record.Exception(shouldThrow);

                /* Assert */
                Assert.IsType<ArgumentNullException>(ex);
            }
        }

        public class HandleAsync : ApplicationScopeBatchDecoratorTests
        {
            [Fact]
            public async Task WithinANewScope_CallsInnerHandler()
            {
                /* Arrange */
                var cmd = new[] { new CommandStub() };

                /* Act */
                await sut.HandleAsync(cmd, token);

                /* Assert */
                A.CallTo(() => scope.NewScope()).MustHaveHappened()
                    .Then(A.CallTo(() => inner.HandleAsync(cmd, token)).MustHaveHappened());
            }
        }
    }
}
