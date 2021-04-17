namespace BusinessApp.App.UnitTest
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using FakeItEasy;
    using BusinessApp.Domain;
    using Xunit;
    using System.Threading;

    public class TransactionRequestDecorator
    {
        private readonly TransactionRequestDecorator<CommandStub, CommandStub> sut;
        private readonly IRequestHandler<CommandStub, CommandStub> inner;
        private readonly ITransactionFactory factory;
        private readonly ILogger logger;
        private readonly PostCommitRegister register;

        public TransactionRequestDecorator()
        {
            inner = A.Fake<IRequestHandler<CommandStub, CommandStub>>();
            factory = A.Fake<ITransactionFactory>();
            register = new PostCommitRegister();
            logger = A.Fake<ILogger>();

            sut = new TransactionRequestDecorator<CommandStub, CommandStub>(factory, inner, register,
                logger);
        }

        public class Constructor : TransactionRequestDecorator
        {
            public static IEnumerable<object[]> InvalidCtorArgs => new[]
            {
                new object[]
                {
                    null,
                    A.Dummy<IRequestHandler<CommandStub, CommandStub>>(),
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
                    A.Dummy<IRequestHandler<CommandStub, CommandStub>>(),
                    null,
                    A.Dummy<ILogger>()
                },
            };

            [Theory, MemberData(nameof(InvalidCtorArgs))]
            public void InvalidCtorArgs_ExceptionThrown(ITransactionFactory v,
                IRequestHandler<CommandStub, CommandStub> c, PostCommitRegister r, ILogger l)
            {
                /* Arrange */
                void shouldThrow() => new TransactionRequestDecorator<CommandStub, CommandStub>(v, c, r, l);

                /* Act */
                var ex = Record.Exception(shouldThrow);

                /* Assert */
                Assert.IsType<BusinessAppException>(ex);
            }
        }

        public class HandleAsync : TransactionRequestDecorator
        {
            [Fact]
            public async Task BeforeRegister_TransFactoryAndHandlerCalledInOrder()
            {
                /* Arrange */
                var cancelToken = A.Dummy<CancellationToken>();
                var handler = A.Fake<Func<Task>>();
                var command = A.Dummy<CommandStub>();
                var uow = A.Fake<IUnitOfWork>();
                A.CallTo(() => factory.Begin()).Returns(Result.Ok(uow));
                register.FinishHandlers.Add(handler);

                /* Act */
                await sut.HandleAsync(command, cancelToken);

                /* Assert */
                A.CallTo(() => factory.Begin()).MustHaveHappened()
                    .Then(A.CallTo(() => inner.HandleAsync(command, cancelToken)).MustHaveHappened())
                    .Then(A.CallTo(() => uow.CommitAsync(cancelToken)).MustHaveHappened())
                    .Then(A.CallTo(() => handler.Invoke()).MustHaveHappened());
            }

            [Fact]
            public async Task AfterFirstCommit_HandlersRunThenCommit()
            {
                /* Arrange */
                var cancelToken = A.Dummy<CancellationToken>();
                var handler1 = A.Fake<Func<Task>>();
                var handler2 = A.Fake<Func<Task>>();
                var uow = A.Fake<IUnitOfWork>();
                register.FinishHandlers.Add(handler1);
                register.FinishHandlers.Add(handler2);
                A.CallTo(() => factory.Begin()).Returns(Result.Ok(uow));

                /* Act */
                await sut.HandleAsync(A.Dummy<CommandStub>(), cancelToken);

                /* Assert */
                A.CallTo(() => handler1()).MustHaveHappenedOnceExactly()
                    .Then(A.CallTo(() => handler2()).MustHaveHappenedOnceExactly())
                    .Then(A.CallTo(() => uow.CommitAsync(cancelToken)).MustHaveHappened());
            }

            [Fact]
            public async Task FinishHandlersThrow_RevertRunsImmediately()
            {
                /* Arrange */
                var cancelToken = A.Dummy<CancellationToken>();
                var handler1 = A.Fake<Func<Task>>();
                var handler2 = A.Fake<Func<Task>>();
                var uow = A.Fake<IUnitOfWork>();
                A.CallTo(() => handler1.Invoke()).Throws<Exception>();
                A.CallTo(() => factory.Begin()).Returns(Result.Ok(uow));
                register.FinishHandlers.Add(handler1);
                register.FinishHandlers.Add(handler2);

                /* Act */
                var ex = await Record.ExceptionAsync(() => sut.HandleAsync(A.Dummy<CommandStub>(), cancelToken));

                /* Assert */
                Assert.NotNull(ex);
                A.CallTo(() => handler1.Invoke()).MustHaveHappenedOnceExactly()
                    .Then(A.CallTo(() => uow.RevertAsync(cancelToken)).MustHaveHappenedOnceExactly());
                A.CallTo(() => handler2.Invoke()).MustNotHaveHappened();
            }

            [Fact]
            public async Task SecondCommitThrows_RevertRunsImmediately()
            {
                /* Arrange */
                var cancelToken = A.Dummy<CancellationToken>();
                var handler = A.Fake<Func<Task>>();
                var uow = A.Fake<IUnitOfWork>();
                A.CallTo(() => factory.Begin()).Returns(Result.Ok(uow));
                A.CallTo(() => uow.CommitAsync(cancelToken))
                    .DoesNothing().Once().Then.Throws<Exception>();
                register.FinishHandlers.Add(handler);

                /* Act */
                var ex = await Record.ExceptionAsync(() => sut.HandleAsync(A.Dummy<CommandStub>(), cancelToken));

                /* Assert */
                Assert.NotNull(ex);
                A.CallTo(() => handler.Invoke()).MustHaveHappened()
                    .Then(A.CallTo(() => uow.CommitAsync(cancelToken)).MustHaveHappened())
                    .Then(A.CallTo(() => uow.RevertAsync(cancelToken)).MustHaveHappened());
            }

            [Fact]
            public async Task SecondCommitThrows_LaterHandlerNotRun()
            {
                /* Arrange */
                var cancelToken = A.Dummy<CancellationToken>();
                var handler1 = A.Fake<Func<Task>>();
                var handler2 = A.Fake<Func<Task>>();
                var uow = A.Fake<IUnitOfWork>();
                A.CallTo(() => factory.Begin()).Returns(Result.Ok(uow));
                A.CallTo(() => uow.CommitAsync(cancelToken)).Throws<Exception>();
                A.CallTo(() => handler1.Invoke())
                    .Invokes(ctx => register.FinishHandlers.Add(handler2));
                register.FinishHandlers.Add(handler1);

                /* Act */
                var ex = await Record.ExceptionAsync(() => sut.HandleAsync(A.Dummy<CommandStub>(), cancelToken));

                /* Assert */
                A.CallTo(() => handler2.Invoke()).MustNotHaveHappened();
            }

            public class RevertThrows : TransactionRequestDecorator
            {
                private CancellationToken cancelToken;
                private LogEntry logEntry;
                private IUnitOfWork uow;

                public RevertThrows()
                {
                    uow = A.Fake<IUnitOfWork>();
                    var handler1 = A.Fake<Func<Task>>();
                    cancelToken = A.Dummy<CancellationToken>();
                    logEntry = null;
                    A.CallTo(() => factory.Begin()).Returns(Result.Ok(uow));
                    A.CallTo(() => handler1.Invoke()).Throws<Exception>();
                    register.FinishHandlers.Add(handler1);
                    A.CallTo(() => logger.Log(A<LogEntry>._))
                        .Invokes(ctx => logEntry = ctx.GetArgument<LogEntry>(0));
                }

                [Fact]
                public async Task RevertThrows_CriticalErrorLogged()
                {
                    /* Arrange */
                    A.CallTo(() => uow.RevertAsync(cancelToken)).Throws<Exception>();

                    /* Act */
                    var _ = await Record.ExceptionAsync(() => sut.HandleAsync(A.Dummy<CommandStub>(), cancelToken));

                    /* Assert */
                    Assert.Equal(LogSeverity.Critical, logEntry.Severity);
                }

                [Fact]
                public async Task RevertThrows_ExceptionMessagLogged()
                {
                    /* Arrange */
                    A.CallTo(() => uow.RevertAsync(cancelToken)).Throws(new Exception("foobar"));

                    /* Act */
                    var _ = await Record.ExceptionAsync(() => sut.HandleAsync(A.Dummy<CommandStub>(), cancelToken));

                    /* Assert */
                    Assert.Equal("foobar", logEntry.Message);
                }

                [Fact]
                public async Task RevertThrows_ExceptionLogged()
                {
                    /* Arrange */
                    var targetException = new Exception();
                    A.CallTo(() => uow.RevertAsync(cancelToken)).Throws(targetException);

                    /* Act */
                    var _ = await Record.ExceptionAsync(() => sut.HandleAsync(A.Dummy<CommandStub>(), cancelToken));

                    /* Assert */
                    Assert.Same(targetException, logEntry.Exception);
                }

                [Fact]
                public async Task RevertThrows_CommandLogged()
                {
                    /* Arrange */
                    var targetCmd = new CommandStub();
                    A.CallTo(() => uow.RevertAsync(cancelToken)).Throws<Exception>();

                    /* Act */
                    var _ = await Record.ExceptionAsync(() => sut.HandleAsync(targetCmd, cancelToken));

                    /* Assert */
                    Assert.Same(targetCmd, logEntry.Data);
                }
            }
        }
    }
}
