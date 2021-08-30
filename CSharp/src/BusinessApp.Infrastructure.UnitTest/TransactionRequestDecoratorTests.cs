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
        private readonly IPostCommitHandler<CommandStub, CommandStub> postHandler;
        private readonly IUnitOfWork uow;
        private readonly CancellationToken cancelToken;
        private readonly CommandStub request;
        private readonly CommandStub response;

        public TransactionRequestDecorator()
        {
            inner = A.Fake<IRequestHandler<CommandStub, CommandStub>>();
            uow = A.Fake<IUnitOfWork>();
            postHandler = A.Fake<IPostCommitHandler<CommandStub, CommandStub>>();
            cancelToken = A.Dummy<CancellationToken>();
            response = new CommandStub();
            request = A.Dummy<CommandStub>();

            sut = new TransactionRequestDecorator<CommandStub, CommandStub>(inner, uow, postHandler);

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
                    A.Dummy<IPostCommitHandler<CommandStub, CommandStub>>(),
                },
                new object[]
                {
                    A.Fake<IUnitOfWork>(),
                    null,
                    A.Dummy<IPostCommitHandler<CommandStub, CommandStub>>(),
                },
                new object[]
                {
                    A.Fake<IUnitOfWork>(),
                    A.Dummy<IRequestHandler<CommandStub, CommandStub>>(),
                    null,
                },
            };

            [Theory, MemberData(nameof(InvalidCtorArgs))]
            public void InvalidCtorArgs_ExceptionThrown(IUnitOfWork v,
                IRequestHandler<CommandStub, CommandStub> c,
                IPostCommitHandler<CommandStub, CommandStub> p)
            {
                /* Arrange */
                void shouldThrow() => new TransactionRequestDecorator<CommandStub, CommandStub>(c, v, p);

                /* Act */
                var ex = Record.Exception(shouldThrow);

                /* Assert */
                Assert.IsType<BusinessAppException>(ex);
            }
        }

        public class HandleAsync : TransactionRequestDecorator
        {
            [Fact]
            public async Task NoErrors_InnerResultReturned()
            {
                /* Arrange */
                var originalResult= Result.Ok(A.Dummy<CommandStub>());
                A.CallTo(() => inner.HandleAsync(request, cancelToken))
                    .Returns(originalResult);

                /* Act */
                var result = await sut.HandleAsync(request, cancelToken);

                /* Assert */
                Assert.Equal(originalResult, result);
            }

            [Fact]
            public async Task NoErrors_ServicesCalledInOrder()
            {
                /* Act */
                await sut.HandleAsync(request, cancelToken);

                /* Assert */
                A.CallTo(() => inner.HandleAsync(request, cancelToken)).MustHaveHappened()
                    .Then(A.CallTo(() => uow.CommitAsync(cancelToken)).MustHaveHappened())
                    .Then(A.CallTo(() => postHandler.HandleAsync(request, response, cancelToken)).MustHaveHappened())
                    .Then(A.CallTo(() => uow.CommitAsync(cancelToken)).MustHaveHappened());
            }

            [Fact]
            public async Task InnerHandlerReturnsError_CommitNotCalled()
            {
                /* Arrange */
                A.CallTo(() => inner.HandleAsync(request, cancelToken))
                    .Returns(Result.Error<CommandStub>(A.Dummy<Exception>()));

                /* Act */
                await sut.HandleAsync(request, cancelToken);

                /* Assert */
                A.CallTo(() => uow.CommitAsync(A<CancellationToken>._)).MustNotHaveHappened();
            }

            [Fact]
            public async Task InnerHandlerReturnsError_PostHandlersNotCalled()
            {
                /* Arrange */
                A.CallTo(() => inner.HandleAsync(request, cancelToken))
                    .Returns(Result.Error<CommandStub>(A.Dummy<Exception>()));

                /* Act */
                await sut.HandleAsync(request, cancelToken);

                /* Assert */
                A.CallTo(() => postHandler.HandleAsync(A<CommandStub>._, A<CommandStub>._, A<CancellationToken>._))
                    .MustNotHaveHappened();
            }

            [Fact]
            public async Task InnerHandlerReturnsError_RevertNotCalled()
            {
                /* Arrange */
                A.CallTo(() => inner.HandleAsync(request, cancelToken))
                    .Returns(Result.Error<CommandStub>(A.Dummy<Exception>()));

                /* Act */
                await sut.HandleAsync(request, cancelToken);

                /* Assert */
                A.CallTo(() => uow.RevertAsync(cancelToken)).MustNotHaveHappened();
            }

            [Fact]
            public async Task PostCommitHandlerThrows_RevertCalled()
            {
                /* Arrange */
                A.CallTo(() => postHandler.HandleAsync(request, response, cancelToken))
                    .Throws<Exception>();

                /* Act */
                var thrownError = await Record.ExceptionAsync(() => sut.HandleAsync(request, cancelToken));

                /* Assert */
                A.CallTo(() => uow.RevertAsync(cancelToken)).MustHaveHappenedOnceExactly();
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
            public async Task PostCommitHandlerReturnsError_RevertCalled()
            {
                /* Arrange */
                A.CallTo(() => postHandler.HandleAsync(request, response, cancelToken))
                    .Returns(Result.Error(A.Dummy<Exception>()));

                /* Act */
                _ = await sut.HandleAsync(request, cancelToken);

                /* Assert */
                A.CallTo(() => uow.RevertAsync(cancelToken)).MustHaveHappenedOnceExactly();
            }

            [Fact]
            public async Task PostCommitHandlerReturnsError_ThatErrorReturned()
            {
                /* Arrange */
                var exception = new Exception("fooish");
                var originalResult = Result.Error(exception);
                A.CallTo(() => postHandler.HandleAsync(request, response, cancelToken))
                    .Returns(originalResult);

                /* Act */
                var result = await sut.HandleAsync(request, cancelToken);

                /* Assert */
                Assert.Same(exception, result.UnwrapError());
            }

            [Fact]
            public async Task RevertThrows_RevertNotRunAgain()
            {
                /* Arrange */
                A.CallTo(() => postHandler.HandleAsync(request, response, cancelToken))
                    .Returns(Result.Error(A.Dummy<Exception>()));
                A.CallTo(() => uow.RevertAsync(cancelToken)).Throws<Exception>();

                /* Act */
                var ex = await Record.ExceptionAsync(() => sut.HandleAsync(request, cancelToken));

                /* Assert */
                A.CallTo(() => uow.RevertAsync(cancelToken)).MustHaveHappenedOnceExactly();
            }


            [Fact]
            public async Task SecondCommitThrows_RevertRun()
            {
                /* Arrange */
                A.CallTo(() => uow.CommitAsync(cancelToken))
                    .Returns(Task.CompletedTask).Once()
                    .Then
                    .Throws<Exception>();

                /* Act */
                var ex = await Record.ExceptionAsync(() => sut.HandleAsync(request, cancelToken));

                /* Assert */
                A.CallTo(() => uow.RevertAsync(cancelToken)).MustHaveHappenedOnceExactly();
            }
        }
    }
}
