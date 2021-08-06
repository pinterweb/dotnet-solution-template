using Xunit;
using System.Collections.Generic;
using FakeItEasy;
using System.Threading.Tasks;
using System.Linq;
using System.Threading;
using BusinessApp.Kernel;
using System;
using BusinessApp.Test.Shared;

namespace BusinessApp.Infrastructure.UnitTest
{
    public class CompositePostCommitHandlerTests
    {
        private readonly CancellationToken cancelToken;
        private readonly IEnumerable<IPostCommitHandler<RequestStub, RequestStub>> handlers;
        private readonly RequestStub request;
        private readonly RequestStub response;
        private CompositePostCommitHandler<RequestStub, RequestStub> sut;

        public CompositePostCommitHandlerTests()
        {
            cancelToken = A.Dummy<CancellationToken>();
            request = new RequestStub();
            response = new RequestStub();
            handlers = new[]
            {
                A.Fake<IPostCommitHandler<RequestStub, RequestStub>>(),
                A.Fake<IPostCommitHandler<RequestStub, RequestStub>>(),
            };
            sut = new CompositePostCommitHandler<RequestStub, RequestStub>(handlers);
        }

        public class Constructor: CompositePostCommitHandlerTests
        {
            [Fact]
            public void InvalidArgs_ExceptionThrown()
            {
                /* Arrange */
                void shouldThrow() => new CompositePostCommitHandler<RequestStub, RequestStub>(null);

                /* Act */
                var ex = Record.Exception(shouldThrow);

                /* Assert */
                Assert.IsType<BusinessAppException>(ex);
            }
        }

        public class HandleAsync : CompositePostCommitHandlerTests
        {
            [Fact]
            public async Task EmptyPostCommitHandlers_OkResultReturned()
            {
                /* Arrange */
                sut = new CompositePostCommitHandler<RequestStub, RequestStub>(
                    Array.Empty<IPostCommitHandler<RequestStub, RequestStub>>());

                /* Act */
                var result = await sut.HandleAsync(A.Dummy<RequestStub>(),
                    A.Dummy<RequestStub>(), cancelToken);

                /* Assert */
                Assert.Equal(Result.Ok(), result);
            }

            [Fact]
            public async Task FirstHandlerHasError_FirstErrorReturned()
            {
                /* Arrange */
                var error = new Exception();
                A.CallTo(() => handlers.First().HandleAsync(request, response, cancelToken))
                    .Returns(Result.Error(error));
                A.CallTo(() => handlers.Last().HandleAsync(request, response, cancelToken))
                    .Returns(Result.Ok());

                /* Act */
                var result = await sut.HandleAsync(request, response, cancelToken);

                /* Assert */
                Assert.Equal(Result.Error(error), result);
            }

            [Fact]
            public async Task FirstHandlerHasError_SecondNotRun()
            {
                /* Arrange */
                var error = new Exception();
                A.CallTo(() => handlers.First().HandleAsync(request, response, cancelToken))
                    .Returns(Result.Error(error));
                A.CallTo(() => handlers.Last().HandleAsync(request, response, cancelToken))
                    .Returns(Result.Ok());

                /* Act */
                var result = await sut.HandleAsync(request, response, cancelToken);

                /* Assert */
                A.CallTo(() => handlers.Last().HandleAsync(A<RequestStub>._, A<RequestStub>._,
                    cancelToken)).MustNotHaveHappened();
            }

            [Fact]
            public async Task AllOkResults_OkResultReturned()
            {
                /* Arrange */
                A.CallTo(() => handlers.First().HandleAsync(request, response, cancelToken))
                    .Returns(Result.Ok());
                A.CallTo(() => handlers.Last().HandleAsync(request, response, cancelToken))
                    .Returns(Result.Ok());

                /* Act */
                var result = await sut.HandleAsync(request, response, cancelToken);

                /* Assert */
                Assert.Equal(Result.Ok(), result);
            }
        }
    }
}
