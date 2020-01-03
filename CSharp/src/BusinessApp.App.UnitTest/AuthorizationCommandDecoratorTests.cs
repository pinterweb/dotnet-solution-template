namespace BusinessApp.App.UnitTest
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using FakeItEasy;
    using Xunit;

    public class AuthorizationCommandDecoratorTests
    {
        private readonly AuthorizationCommandDecorator<CommandStub> sut;
        private readonly ICommandHandler<CommandStub> decorated;
        private readonly IAuthorizer<CommandStub> authorizer;

        public AuthorizationCommandDecoratorTests()
        {
            decorated = A.Fake<ICommandHandler<CommandStub>>();
            authorizer = A.Fake<IAuthorizer<CommandStub>>();

            sut = new AuthorizationCommandDecorator<CommandStub>(decorated, authorizer);
        }

        public class Constructor : AuthorizationCommandDecoratorTests
        {
            public static IEnumerable<object[]> InvalidCtorArgs
            {
                get
                {
                    return new []
                    {
                        new object[] { null, A.Dummy<IAuthorizer<CommandStub>>() },
                        new object[] { A.Dummy<ICommandHandler<CommandStub>>(), null }
                    };
                }
            }

            [Theory, MemberData(nameof(InvalidCtorArgs))]
            public void WithAnyNullService_ExceptionThrown(
                ICommandHandler<CommandStub> d,
                IAuthorizer<CommandStub> a)
            {
                /* Arrange */
                Action create = () => new AuthorizationCommandDecorator<CommandStub>(d, a);

                /* Act */
                var exception = Record.Exception(create);

                /* Assert */
                Assert.IsType<ArgumentNullException>(exception);
            }
        }


        public class HandleAsync : AuthorizationCommandDecoratorTests
        {
            CommandStub query;

            public HandleAsync()
            {
                query = new CommandStub();
            }

            [Fact]
            public async Task AuthorizedBeforeHandles()
            {
                /* Arrange */
                bool handlerCalled = true;
                A.CallTo(() => authorizer.AuthorizeObject(query))
                    .Invokes(ctx => handlerCalled = Fake.GetCalls(decorated).Count() != 0);

                /* Act */
                await sut.HandleAsync(query, default);

                /* Assert */
                Assert.False(handlerCalled);
            }

            [Fact]
            public async Task CallsDecoratedHandlerOnce()
            {
                /* Act */
                await sut.HandleAsync(query, default);

                /* Assert */
                A.CallTo(() => decorated.HandleAsync(A<CommandStub>._, A<CancellationToken>._))
                    .MustHaveHappenedOnceExactly();
            }

            [Fact]
            public async Task CallsAuthorizerOnce()
            {
                /* Act */
                await sut.HandleAsync(query, default);

                /* Assert */
                A.CallTo(() => authorizer.AuthorizeObject(A<CommandStub>._))
                    .MustHaveHappenedOnceExactly();
            }
        }
    }
}
