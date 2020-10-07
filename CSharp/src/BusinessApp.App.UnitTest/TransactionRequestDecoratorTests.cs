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
        private readonly TransactionRequestDecorator<CommandStub, CommandStub> sut;
        private readonly ICommandHandler<CommandStub> inner;
        private readonly ITransactionFactory factory;
        private readonly ILogger logger;
        private readonly PostCommitRegister register;

        public TransactionDecoratorTests()
        {
            inner = A.Fake<ICommandHandler<CommandStub>>();
            factory = A.Fake<ITransactionFactory>();
            register = new PostCommitRegister();
            logger = A.Fake<ILogger>();

            sut = new TransactionRequestDecorator<CommandStub, CommandStub>(factory, inner, register, logger);
        }

        public class Constructor : TransactionDecoratorTests
        {
            public static IEnumerable<object[]> InvalidCtorArgs => new[]
            {
                new object[]
                {
                    null,
                    A.Dummy<ICommandHandler<CommandStub>>(),
                    A.Dummy<PostCommitRegister>(),
                    A.Dummy<ILogger>()
                },
                new object[]
                {
                    A.Fake<ITransactionFactory>(),
                    null,
                    A.Dummy<PostCommitRegister>(),
                    A.Dummy<ILogger>()
                },
                new object[]
                {
                    A.Fake<ITransactionFactory>(),
                    A.Dummy<ICommandHandler<CommandStub>>(),
                    null,
                    A.Dummy<ILogger>()
                },
            };

            [Theory, MemberData(nameof(InvalidCtorArgs))]
            public void InvalidCtorArgs_ExceptionThrown(ITransactionFactory v,
                ICommandHandler<CommandStub> c, PostCommitRegister r, ILogger l)
            {
                /* Arrange */
                void shouldThrow() => new TransactionRequestDecorator<CommandStub, CommandStub>(v, c, r, l);

                /* Act */
                var ex = Record.Exception(shouldThrow);

                /* Assert */
                Assert.IsType<BadStateException>(ex);
            }
        }

        public class HandleAsync : TransactionDecoratorTests
        {
            [Fact]
            public async Task NullCommand_ExceptionThrown()
            {
                /* Arrange */
                Task shouldthrow() => sut.HandleAsync(null, A.Dummy<CancellationToken>());

                /* Act */
                var ex = await Record.ExceptionAsync(shouldthrow);

                /* Assert */
                Assert.IsType<BadStateException>(ex);
            }

            [Fact]
            public async Task BeforeRegister_TransFactoryAndHandlerCalledInOrder()
            {
                /* Arrange */
                var token = A.Dummy<CancellationToken>();
                var handler = A.Fake<Func<Task>>();
                var command = A.Dummy<CommandStub>();
                var uow = A.Fake<IUnitOfWork>();
                A.CallTo(() => factory.Begin()).Returns(uow);
                register.FinishHandlers.Add(handler);

                /* Act */
                await sut.HandleAsync(command, token);

                /* Assert */
                A.CallTo(() => factory.Begin()).MustHaveHappened()
                    .Then(A.CallTo(() => inner.HandleAsync(command, token)).MustHaveHappened())
                    .Then(A.CallTo(() => uow.CommitAsync(token)).MustHaveHappened())
                    .Then(A.CallTo(() => handler.Invoke()).MustHaveHappened());
            }

            [Fact]
            public async Task AfterFirstCommit_HandlersRunThenCommit()
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
            public async Task FinishHandlersThrow_RevertRunsImmediately()
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
            public async Task SecondCommitThrows_RevertRunsImmediately()
            {
                /* Arrange */
                var token = A.Dummy<CancellationToken>();
                var handler = A.Fake<Func<Task>>();
                var uow = A.Fake<IUnitOfWork>();
                A.CallTo(() => factory.Begin()).Returns(uow);
                A.CallTo(() => uow.CommitAsync(token))
                    .DoesNothing().Once().Then.Throws<Exception>();
                register.FinishHandlers.Add(handler);

                /* Act */
                var ex = await Record.ExceptionAsync(() => sut.HandleAsync(A.Dummy<CommandStub>(), token));

                /* Assert */
                Assert.NotNull(ex);
                A.CallTo(() => handler.Invoke()).MustHaveHappened()
                    .Then(A.CallTo(() => uow.CommitAsync(token)).MustHaveHappened())
                    .Then(A.CallTo(() => uow.RevertAsync(token)).MustHaveHappened());
            }

            [Fact]
            public async Task SecondCommitThrows_LaterHandlerNotRun()
            {
                /* Arrange */
                var token = A.Dummy<CancellationToken>();
                var handler1 = A.Fake<Func<Task>>();
                var handler2 = A.Fake<Func<Task>>();
                var uow = A.Fake<IUnitOfWork>();
                A.CallTo(() => factory.Begin()).Returns(uow);
                A.CallTo(() => uow.CommitAsync(token)).Throws<Exception>();
                A.CallTo(() => handler1.Invoke())
                    .Invokes(ctx => register.FinishHandlers.Add(handler2));
                register.FinishHandlers.Add(handler1);

                /* Act */
                var ex = await Record.ExceptionAsync(() => sut.HandleAsync(A.Dummy<CommandStub>(), token));

                /* Assert */
                A.CallTo(() => handler2.Invoke()).MustNotHaveHappened();
            }

            public class RevertThrows : TransactionDecoratorTests
            {
                private CancellationToken token;
                private LogEntry logEntry;
                private IUnitOfWork uow;

                public RevertThrows()
                {
                    uow = A.Fake<IUnitOfWork>();
                    var handler1 = A.Fake<Func<Task>>();
                    token = A.Dummy<CancellationToken>();
                    logEntry = null;
                    A.CallTo(() => factory.Begin()).Returns(uow);
                    A.CallTo(() => handler1.Invoke()).Throws<Exception>();
                    register.FinishHandlers.Add(handler1);
                    A.CallTo(() => logger.Log(A<LogEntry>._))
                        .Invokes(ctx => logEntry = ctx.GetArgument<LogEntry>(0));
                }

                [Fact]
                public async Task RevertThrows_CriticalErrorLogged()
                {
                    /* Arrange */
                    A.CallTo(() => uow.RevertAsync(token)).Throws<Exception>();

                    /* Act */
                    var _ = await Record.ExceptionAsync(() => sut.HandleAsync(A.Dummy<CommandStub>(), token));

                    /* Assert */
                    Assert.Equal(LogSeverity.Critical, logEntry.Severity);
                }

                [Fact]
                public async Task RevertThrows_ExceptionMessagLogged()
                {
                    /* Arrange */
                    A.CallTo(() => uow.RevertAsync(token)).Throws(new Exception("foobar"));

                    /* Act */
                    var _ = await Record.ExceptionAsync(() => sut.HandleAsync(A.Dummy<CommandStub>(), token));

                    /* Assert */
                    Assert.Equal("foobar", logEntry.Message);
                }

                [Fact]
                public async Task RevertThrows_ExceptionLogged()
                {
                    /* Arrange */
                    var targetException = new Exception();
                    A.CallTo(() => uow.RevertAsync(token)).Throws(targetException);

                    /* Act */
                    var _ = await Record.ExceptionAsync(() => sut.HandleAsync(A.Dummy<CommandStub>(), token));

                    /* Assert */
                    Assert.Same(targetException, logEntry.Exception);
                }

                [Fact]
                public async Task RevertThrows_CommandLogged()
                {
                    /* Arrange */
                    var targetCmd = new CommandStub();
                    A.CallTo(() => uow.RevertAsync(token)).Throws<Exception>();

                    /* Act */
                    var _ = await Record.ExceptionAsync(() => sut.HandleAsync(targetCmd, token));

                    /* Assert */
                    Assert.Same(targetCmd, logEntry.Data);
                }
            }
        }
    }
}
