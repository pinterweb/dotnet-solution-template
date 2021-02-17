namespace BusinessApp.App.UnitTest
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using BusinessApp.Domain;
    using FakeItEasy;
    using Xunit;

    public class ScopedBatchRequestProxyTests
    {
        private readonly CancellationToken cancelToken;
        private readonly ScopedBatchRequestProxy<CommandStub, IEnumerable<CommandStub>> sut;
        private readonly ICommandHandler<IEnumerable<CommandStub>> inner;
        private readonly IAppScope scope;

        public ScopedBatchRequestProxyTests()
        {
            cancelToken = A.Dummy<CancellationToken>();
            inner = A.Fake<ICommandHandler<IEnumerable<CommandStub>>>();
            scope = A.Fake<IAppScope>();

            sut = new ScopedBatchRequestProxy<CommandStub, IEnumerable<CommandStub>>(
                scope,
                () => inner);
        }

        public class Constructor : ScopedBatchRequestProxyTests
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
                void shouldThrow() =>
                    new ScopedBatchRequestProxy<CommandStub, IEnumerable<CommandStub>>(a, f);

                /* Act */
                var ex = Record.Exception(shouldThrow);

                /* Assert */
                Assert.IsType<BadStateException>(ex);
            }
        }

        public class HandleAsync : ScopedBatchRequestProxyTests
        {
            [Fact]
            public async Task WithinANewScope_CallsInnerHandler()
            {
                /* Arrange */
                var cmd = new[] { new CommandStub() };

                /* Act */
                await sut.HandleAsync(cmd, cancelToken);

                /* Assert */
                A.CallTo(() => scope.NewScope()).MustHaveHappened()
                    .Then(A.CallTo(() => inner.HandleAsync(cmd, cancelToken)).MustHaveHappened());
            }
        }
    }
}
