using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BusinessApp.Kernel;
using FakeItEasy;
using Xunit;
using BusinessApp.Test.Shared;
using System;

namespace BusinessApp.Infrastructure.UnitTest
{
    public class BatchPostCommitAdapterTests
    {
        private readonly CancellationToken cancelToken;
        private readonly BatchPostCommitAdapter<RequestStub, ResponseStub> sut;
        private readonly IPostCommitHandler<RequestStub, ResponseStub> inner;

        public BatchPostCommitAdapterTests()
        {
            cancelToken = A.Dummy<CancellationToken>();
            inner = A.Fake<IPostCommitHandler<RequestStub, ResponseStub>>();

            sut = new BatchPostCommitAdapter<RequestStub, ResponseStub>(inner);
        }

        public class Constructor : BatchRequestAdapterTests
        {
            public static IEnumerable<object[]> InvalidCtorArgs => new[]
            {
                new object[] { null  },
            };

            [Theory, MemberData(nameof(InvalidCtorArgs))]
            public void InvalidCtorArgs_ExceptionThrown(IPostCommitHandler<RequestStub, ResponseStub> c)
            {
                /* Arrange */
                void shouldThrow() => new BatchPostCommitAdapter<RequestStub, ResponseStub>(c);

                /* Act */
                var ex = Record.Exception(shouldThrow);

                /* Assert */
                Assert.IsType<BusinessAppException>(ex);
            }
        }

        public class HandleAsync : BatchPostCommitAdapterTests
        {
            [Fact]
            public async Task MultiplRequestsAndResponses_AllHandlersRunInOrder()
            {
                /* Arrange */
                var requests = A.CollectionOfDummy<RequestStub>(2);
                var responses = A.CollectionOfDummy<ResponseStub>(2);

                /* Act */
                await sut.HandleAsync(requests, responses, cancelToken);

                /* Assert */
                A.CallTo(() => inner.HandleAsync(requests.First(), responses.First(), cancelToken))
                    .MustHaveHappenedOnceExactly()
                    .Then(
                        A.CallTo(() => inner.HandleAsync(requests.Last(), responses.Last(), cancelToken))
                            .MustHaveHappenedOnceExactly()
                    );
            }

            [Fact]
            public async Task AllHandlersSuccess_OkResultReturned()
            {
                /* Arrange */
                var requests = A.CollectionOfDummy<RequestStub>(2);
                var responses = A.CollectionOfDummy<ResponseStub>(2);

                /* Act */
                var result = await sut.HandleAsync(requests, responses, cancelToken);

                /* Assert */
                Assert.Equal(Result.Ok(), result);
            }

            [Fact]
            public async Task OneHandlerErrored_ErrorReturned()
            {
                /* Arrange */
                var error = new Exception();
                var requests = A.CollectionOfDummy<RequestStub>(2);
                var responses = A.CollectionOfDummy<ResponseStub>(2);
                A.CallTo(() => inner.HandleAsync(requests.Last(), responses.Last(), cancelToken))
                    .Returns(Result.Error(error));

                /* Act */
                var result = await sut.HandleAsync(requests, responses, cancelToken);

                /* Assert */
                Assert.Same(error, result.UnwrapError());
            }

            [Fact]
            public async Task MultipleErrored_AggregateErrorReturned()
            {
                /* Arrange */
                var error1 = new Exception();
                var error2 = new Exception();
                var requests = A.CollectionOfDummy<RequestStub>(2);
                var responses = A.CollectionOfDummy<ResponseStub>(2);
                A.CallTo(() => inner.HandleAsync(A<RequestStub>._, A<ResponseStub>._, cancelToken))
                    .Returns(Result.Error(error1)).Once()
                    .Then.Returns(Result.Error(error2));

                /* Act */
                var result = await sut.HandleAsync(requests, responses, cancelToken);

                /* Assert */
                var error = Assert.IsType<AggregateException>(result.UnwrapError());
                Assert.Collection(error.InnerExceptions,
                    e => Assert.Same(error1, e),
                    e => Assert.Same(error2, e));
            }
        }
    }
}
