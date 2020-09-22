namespace BusinessApp.App.UnitTest
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using FakeItEasy;
    using BusinessApp.Domain;
    using Xunit;
    using System.Threading;

    public class TransactionDecoratorTests
    {
        private readonly TransactionDecorator<CommandStub> sut;
        private readonly ICommandHandler<CommandStub> inner;
        private readonly ITransactionFactory factory;
        private readonly PostCommitRegister register;

        public TransactionDecoratorTests()
        {
            inner = A.Fake<ICommandHandler<CommandStub>>();
            factory = A.Fake<ITransactionFactory>();
            register = new PostCommitRegister();

            sut = new TransactionDecorator<CommandStub>(factory, inner, register);
        }

        public class Constructor : TransactionDecoratorTests
        {
            public static IEnumerable<object[]> InvalidCtorArgs => new[]
            {
                new object[]
                {
                    null,
                    A.Dummy<ICommandHandler<CommandStub>>(),
                    A.Dummy<PostCommitRegister>()
                },
                new object[]
                {
                    A.Fake<ITransactionFactory>(),
                    null,
                    A.Dummy<PostCommitRegister>()
                },
                new object[]
                {
                    A.Fake<ITransactionFactory>(),
                    A.Dummy<ICommandHandler<CommandStub>>(),
                    null,
                },
            };

            [Theory, MemberData(nameof(InvalidCtorArgs))]
            public void InvalidCtorArgs_ExceptionThrown(ITransactionFactory v,
                ICommandHandler<CommandStub> c, PostCommitRegister r)
            {
                /* Arrange */
                void shouldThrow() => new TransactionDecorator<CommandStub>(v, c, r);

                /* Act */
                var ex = Record.Exception(shouldThrow);

                /* Assert */
                Assert.IsType<BadStateException>(ex);
            }
        }

        public class HandleAsync : TransactionDecoratorTests
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
            public async Task BeforeRegister_TransAndHandlerCalledInOrder()
            {
                /* Arrange */
                var token = A.Dummy<CancellationToken>();
                var command = A.Dummy<CommandStub>();
                var uow = A.Fake<IUnitOfWork>();
                A.CallTo(() => factory.Begin()).Returns(uow);

                /* Act */
                await sut.HandleAsync(command, token);

                /* Assert */
                A.CallTo(() => factory.Begin()).MustHaveHappenedOnceExactly()
                    .Then(A.CallTo(() => inner.HandleAsync(command, token)).MustHaveHappenedOnceExactly())
                    .Then(A.CallTo(() => uow.CommitAsync(token)).MustHaveHappenedOnceExactly());
            }

            [Fact]
            public async Task WithFinishHandlers_CommitRunAfterHandlers()
            {
                /* Arrange */
                var token = A.Dummy<CancellationToken>();
                var handler1 = A.Fake<Func<Task>>();
                var handler2 = A.Fake<Func<Task>>();
                var uow = A.Fake<IUnitOfWork>();
                register.FinishHandlers.Add(handler1);
                register.FinishHandlers.Add(handler2);
                A.CallTo(() => factory.Begin()).Returns(uow);

                /* Act */
                await sut.HandleAsync(A.Dummy<CommandStub>(), token);

                /* Assert */
                A.CallTo(() => handler1()).MustHaveHappenedOnceExactly()
                    .Then(A.CallTo(() => handler2()).MustHaveHappenedOnceExactly())
                    .Then(A.CallTo(() => uow.CommitAsync(token)).MustHaveHappened());
            }

            [Fact]
            public async Task WithFinishHandlers_RevertRunAfterHandlerThrows()
            {
                /* Arrange */
                var token = A.Dummy<CancellationToken>();
                var handler1 = A.Fake<Func<Task>>();
                var handler2 = A.Fake<Func<Task>>();
                var uow = A.Fake<IUnitOfWork>();
                A.CallTo(() => handler1.Invoke()).Throws<Exception>();
                A.CallTo(() => factory.Begin()).Returns(uow);
                register.FinishHandlers.Add(handler1);
                register.FinishHandlers.Add(handler2);

                /* Act */
                var ex = await Record.ExceptionAsync(() => sut.HandleAsync(A.Dummy<CommandStub>(), token));

                /* Assert */
                Assert.NotNull(ex);
                A.CallTo(() => handler1.Invoke()).MustHaveHappenedOnceExactly()
                    .Then(A.CallTo(() => uow.RevertAsync(token)).MustHaveHappenedOnceExactly());
                A.CallTo(() => handler2.Invoke()).MustNotHaveHappened();
            }

            [Fact]
            public async Task WithFinishHandlers_ThrowsIfFinalCommitFails()
            {
                /* Arrange */
                var token = A.Dummy<CancellationToken>();
                var thrown = new Exception();
                var handler1 = A.Fake<Func<Task>>();
                var uow = A.Fake<IUnitOfWork>();
                A.CallTo(() => factory.Begin()).Returns(uow);
                A.CallTo(() => uow.CommitAsync(token)).Returns(Task.CompletedTask).Once()
                    .Then.Throws(thrown);
                register.FinishHandlers.Add(handler1);

                /* Act */
                var ex = await Record.ExceptionAsync(() => sut.HandleAsync(A.Dummy<CommandStub>(), token));

                /* Assert */
                Assert.IsType<CommunicationException>(ex);
                Assert.Same(thrown, ex.InnerException);
                Assert.Equal(
                    "Some events generated by this business " +
                    "transaction failed to save. As a result, some data may be in a invalid state." +
                    "Please verify your data before continuing",
                    ex.Message);
            }
        }
    }
}
