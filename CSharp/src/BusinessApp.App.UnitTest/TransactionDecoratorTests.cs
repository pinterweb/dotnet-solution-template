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
        private readonly TransactionDecorator<DummyCommand> sut;
        private readonly ICommandHandler<DummyCommand> inner;
        private readonly ITransactionFactory factory;
        private readonly PostCommitRegister register;

        public TransactionDecoratorTests()
        {
            inner = A.Fake<ICommandHandler<DummyCommand>>();
            factory = A.Fake<ITransactionFactory>();
            register = new PostCommitRegister();

            sut = new TransactionDecorator<DummyCommand>(factory, inner, register);
        }

        public class Constructor : TransactionDecoratorTests
        {
            public static IEnumerable<object[]> InvalidCtorArgs => new[]
            {
                new object[]
                {
                    null,
                    A.Dummy<ICommandHandler<DummyCommand>>(),
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
                    A.Dummy<ICommandHandler<DummyCommand>>(),
                    null,
                },
            };

            [Theory, MemberData(nameof(InvalidCtorArgs))]
            public void InvalidCtorArgs_ExceptionThrown(ITransactionFactory v,
                ICommandHandler<DummyCommand> c, PostCommitRegister r)
            {
                /* Arrange */
                void shouldThrow() => new TransactionDecorator<DummyCommand>(v, c, r);

                /* Act */
                var ex = Record.Exception(shouldThrow);

                /* Assert */
                Assert.IsType<ArgumentNullException>(ex);
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
                var command = A.Dummy<DummyCommand>();
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
                await sut.HandleAsync(A.Dummy<DummyCommand>(), token);

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
                var ex = await Record.ExceptionAsync(() => sut.HandleAsync(A.Dummy<DummyCommand>(), token));

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
                var ex = await Record.ExceptionAsync(() => sut.HandleAsync(A.Dummy<DummyCommand>(), token));

                /* Assert */
                Assert.NotNull(ex);
                Assert.Same(thrown, ex.InnerException);
                Assert.Equal(
                    "If this business transaction generated external messages to other system(s), " +
                    "then the two may be out of sync. The app [currently] does not revert exernal " +
                    "systems once a message was sent. If you expected messages to be sent to external " +
                    "systems please verify the data in those systems before proceeding",
                    ex.Message);
            }
        }
    }
}
