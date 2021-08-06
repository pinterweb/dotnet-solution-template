using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FakeItEasy;
using BusinessApp.Kernel;
using Xunit;
using System.Threading;

namespace BusinessApp.Infrastructure.UnitTest
{
    public class TransactionRequestDecorator
    {
        private readonly TransactionRequestDecorator<CommandStub, CommandStub> sut;
        private readonly IRequestHandler<CommandStub, CommandStub> inner;
        private readonly ITransactionFactory factory;
        private readonly ILogger logger;
        private readonly IPostCommitHandler<CommandStub, CommandStub> postHandler;
        private readonly IUnitOfWork uow;
        private readonly CancellationToken cancelToken;
        private readonly CommandStub request;
        private readonly CommandStub response;


        public TransactionRequestDecorator()
        {
            inner = A.Fake<IRequestHandler<CommandStub, CommandStub>>();
            factory = A.Fake<ITransactionFactory>();
            logger = A.Fake<ILogger>();
            postHandler = A.Fake<IPostCommitHandler<CommandStub, CommandStub>>();
            cancelToken = A.Dummy<CancellationToken>();
            response = new CommandStub();
            request = A.Dummy<CommandStub>();

            sut = new TransactionRequestDecorator<CommandStub, CommandStub>(factory, inner, logger, postHandler);

            uow = A.Fake<IUnitOfWork>();
            A.CallTo(() => factory.Begin()).Returns(Result.Ok(uow));

            A.CallTo(() => inner.HandleAsync(request, cancelToken))
                .Returns(Result.Ok(response));
        }

        public class Constructor : TransactionRequestDecorator
        {
            public static IEnumerable<object[]> InvalidCtorArgs => new[]
            {
                new object[]
                {
                    null,
                    A.Dummy<IRequestHandler<CommandStub, CommandStub>>(),
                    A.Dummy<ILogger>(),
                    A.Dummy<IPostCommitHandler<CommandStub, CommandStub>>(),
                },
                new object[]
                {
                    A.Fake<ITransactionFactory>(),
                    null,
                    A.Dummy<ILogger>(),
                    A.Dummy<IPostCommitHandler<CommandStub, CommandStub>>(),
                },
                new object[]
                {
                    A.Fake<ITransactionFactory>(),
                    A.Dummy<IRequestHandler<CommandStub, CommandStub>>(),
                    null,
                    A.Dummy<IPostCommitHandler<CommandStub, CommandStub>>(),
                },
                new object[]
                {
                    A.Fake<ITransactionFactory>(),
                    A.Dummy<IRequestHandler<CommandStub, CommandStub>>(),
                    A.Dummy<ILogger>(),
                    null,
                },
            };

            [Theory, MemberData(nameof(InvalidCtorArgs))]
            public void InvalidCtorArgs_ExceptionThrown(ITransactionFactory v,
                IRequestHandler<CommandStub, CommandStub> c, ILogger l,
                IPostCommitHandler<CommandStub, CommandStub> p)
            {
                /* Arrange */
                void shouldThrow() => new TransactionRequestDecorator<CommandStub, CommandStub>(v, c, l, p);

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
                /* Act */
                await sut.HandleAsync(request, cancelToken);

                /* Assert */
                A.CallTo(() => factory.Begin()).MustHaveHappened()
                    .Then(A.CallTo(() => inner.HandleAsync(request, cancelToken)).MustHaveHappened())
                    .Then(A.CallTo(() => uow.CommitAsync(cancelToken)).MustHaveHappened())
                    .Then(A.CallTo(() => postHandler.HandleAsync(request, response, cancelToken)).MustHaveHappened())
                    .Then(A.CallTo(() => uow.CommitAsync(cancelToken)).MustHaveHappened());
            }

            [Fact]
            public async Task PostCommitHandlerThrows_RethrowsException()
            {
                /* Arrange */
                var originalError = new Exception();
                A.CallTo(() => postHandler.HandleAsync(request, response, cancelToken))
                    .Throws(originalError);

                /* Act */
                var thrownError = await Record.ExceptionAsync(() => sut.HandleAsync(request, cancelToken));

                /* Assert */
                Assert.Same(originalError, thrownError);
            }

            [Fact]
            public async Task PostCommitHandlerReturnsError_RethrowsException()
            {
                /* Arrange */
                var originalError = new Exception();
                A.CallTo(() => postHandler.HandleAsync(request, response, cancelToken))
                    .Returns(Result.Error(originalError));

                /* Act */
                var thrownError = await Record.ExceptionAsync(() => sut.HandleAsync(request, cancelToken));

                /* Assert */
                Assert.Same(originalError, thrownError);
            }

            [Fact]
            public async Task PostCommitHandlerThrows_RevertRunsImmediately()
            {
                /* Arrange */
                A.CallTo(() => postHandler.HandleAsync(request, response, cancelToken)).Throws<Exception>();

                /* Act */
                var ex = await Record.ExceptionAsync(() => sut.HandleAsync(request, cancelToken));

                /* Assert */
                A.CallTo(() => factory.Begin()).MustHaveHappened()
                    .Then(A.CallTo(() => inner.HandleAsync(request, cancelToken)).MustHaveHappened())
                    .Then(A.CallTo(() => uow.CommitAsync(cancelToken)).MustHaveHappened())
                    .Then(A.CallTo(() => postHandler.HandleAsync(request, response, cancelToken)).MustHaveHappened())
                    .Then(A.CallTo(() => uow.RevertAsync(cancelToken)).MustHaveHappened());
            }

            [Fact]
            public async Task PostCommitHandlerReturnsErrors_RevertRunsImmediately()
            {
                /* Arrange */
                A.CallTo(() => postHandler.HandleAsync(request, response, cancelToken))
                    .Returns(Result.Error(A.Dummy<Exception>()));

                /* Act */
                var ex = await Record.ExceptionAsync(() => sut.HandleAsync(request, cancelToken));

                /* Assert */
                A.CallTo(() => factory.Begin()).MustHaveHappened()
                    .Then(A.CallTo(() => inner.HandleAsync(request, cancelToken)).MustHaveHappened())
                    .Then(A.CallTo(() => uow.CommitAsync(cancelToken)).MustHaveHappened())
                    .Then(A.CallTo(() => postHandler.HandleAsync(request, response, cancelToken)).MustHaveHappened())
                    .Then(A.CallTo(() => uow.RevertAsync(cancelToken)).MustHaveHappened());
            }

            [Fact]
            public async Task SecondCommitThrows_RevertRunImmediately()
            {
                /* Arrange */
                A.CallTo(() => uow.CommitAsync(cancelToken))
                    .Returns(Task.CompletedTask).Once()
                    .Then
                    .Throws<Exception>();

                /* Act */
                var ex = await Record.ExceptionAsync(() => sut.HandleAsync(request, cancelToken));

                /* Assert */
                A.CallTo(() => factory.Begin()).MustHaveHappened()
                    .Then(A.CallTo(() => inner.HandleAsync(request, cancelToken)).MustHaveHappened())
                    .Then(A.CallTo(() => uow.CommitAsync(cancelToken)).MustHaveHappened())
                    .Then(A.CallTo(() => postHandler.HandleAsync(request, response, cancelToken)).MustHaveHappened())
                    .Then(A.CallTo(() => uow.CommitAsync(cancelToken)).MustHaveHappened())
                    .Then(A.CallTo(() => uow.RevertAsync(cancelToken)).MustHaveHappened());
            }

            public class RevertThrows : TransactionRequestDecorator
            {
                private LogEntry logEntry;

                public RevertThrows()
                {
                    logEntry = null;
                    A.CallTo(() => postHandler.HandleAsync(request, response, cancelToken))
                        .Throws<Exception>();
                    A.CallTo(() => logger.Log(A<LogEntry>._))
                        .Invokes(ctx => logEntry = ctx.GetArgument<LogEntry>(0));
                }

                [Fact]
                public async Task RevertThrows_CriticalErrorLogged()
                {
                    /* Arrange */
                    A.CallTo(() => uow.RevertAsync(cancelToken)).Throws<Exception>();

                    /* Act */
                    var _ = await Record.ExceptionAsync(() => sut.HandleAsync(request, cancelToken));

                    /* Assert */
                    Assert.Equal(LogSeverity.Critical, logEntry.Severity);
                }

                [Fact]
                public async Task RevertThrows_ExceptionMessagLogged()
                {
                    /* Arrange */
                    A.CallTo(() => uow.RevertAsync(cancelToken)).Throws(new Exception("foobar"));

                    /* Act */
                    var _ = await Record.ExceptionAsync(() => sut.HandleAsync(request, cancelToken));

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
                    var _ = await Record.ExceptionAsync(() => sut.HandleAsync(request, cancelToken));

                    /* Assert */
                    Assert.Same(targetException, logEntry.Exception);
                }

                [Fact]
                public async Task RevertThrows_CommandLogged()
                {
                    /* Arrange */
                    A.CallTo(() => uow.RevertAsync(cancelToken)).Throws<Exception>();

                    /* Act */
                    var _ = await Record.ExceptionAsync(() => sut.HandleAsync(request, cancelToken));

                    /* Assert */
                    Assert.Same(request, logEntry.Data);
                }
            }
        }
    }
}
