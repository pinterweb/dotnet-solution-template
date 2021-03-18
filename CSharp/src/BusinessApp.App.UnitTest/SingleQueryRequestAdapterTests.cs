namespace BusinessApp.App.UnitTest
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using BusinessApp.Domain;
    using FakeItEasy;
    using Xunit;

    public class SingleQueryRequestAdapterTests
    {
        private readonly CancellationToken cancelToken;
        private readonly SingleQueryRequestAdapter<ConsumerStub, QueryStub, ResponseStub> sut;
        private readonly ConsumerStub consumer;

        public SingleQueryRequestAdapterTests()
        {
            cancelToken = A.Dummy<CancellationToken>();
            consumer = A.Dummy<ConsumerStub>();

            sut = new SingleQueryRequestAdapter<ConsumerStub, QueryStub, ResponseStub>(consumer);
        }

        public class Constructor : SingleQueryRequestAdapterTests
        {
            public static IEnumerable<object[]> InvalidCtorArgs => new[]
            {
                new object[] { null  },
            };

            [Theory, MemberData(nameof(InvalidCtorArgs))]
            public void InvalidCtorArgs_ExceptionThrown(ConsumerStub s)
            {
                /* Arrange */
                void shouldThrow() => new SingleQueryRequestAdapter<ConsumerStub, QueryStub, ResponseStub>(s);

                /* Act */
                var ex = Record.Exception(shouldThrow);

                /* Assert */
                Assert.IsType<BadStateException>(ex);
            }
        }

        public class HandleAsync : SingleQueryRequestAdapterTests
        {
            [Fact]
            public async Task ConsumeReturnsError_ThatErrorReturned()
            {
                /* Arrange */
                var err = new Exception();
                var innerResult = Result.Error<IEnumerable<ResponseStub>>(err);
                var request = A.Dummy<QueryStub>();
                A.CallTo(() => consumer.HandleAsync(request, cancelToken)).Returns(innerResult);

                /* Act */
                var result = await sut.HandleAsync(request, cancelToken);

                /* Assert */
                Assert.Equal(innerResult.UnwrapError(), result.UnwrapError());
            }

            [Fact]
            public async Task ConsumeReturnsOneResponse_ThatResponseReturned()
            {
                /* Arrange */
                var expectedResponse = new ResponseStub();
                var expectedResult = Result.Ok(expectedResponse);
                var responses = new[] { expectedResponse };
                var innerResult = Result.Ok<IEnumerable<ResponseStub>>(responses);
                var request = A.Dummy<QueryStub>();
                A.CallTo(() => consumer.HandleAsync(request, cancelToken)).Returns(innerResult);

                /* Act */
                var result = await sut.HandleAsync(request, cancelToken);

                /* Assert */
                Assert.Equal(expectedResult, result);
            }

            [Fact]
            public async Task ConsumeReturnsMoreThanOneResponse_NewErrorReturned()
            {
                /* Arrange */
                var expectedErrMsg = "Your query expected to return one result, but " +
                    "for some reason more than one result was returned. Please try the " +
                    "request again or contact support";
                var expectedErr = new BusinessAppAppException(expectedErrMsg);
                var responses = new[] { new ResponseStub(), new ResponseStub() };
                var innerResult = Result.Ok<IEnumerable<ResponseStub>>(responses);
                var request = A.Dummy<QueryStub>();
                A.CallTo(() => consumer.HandleAsync(request, cancelToken)).Returns(innerResult);

                /* Act */
                var result = await sut.HandleAsync(request, cancelToken);

                /* Assert */
                var ex = Assert.IsType<BusinessAppAppException>(result.UnwrapError());
                Assert.Equal(expectedErrMsg, ex.Message);
            }
        }

        public class ConsumerStub : IRequestHandler<QueryStub, IEnumerable<ResponseStub>>
        {
            public virtual Task<Result<IEnumerable<ResponseStub>, Exception>> HandleAsync(
                QueryStub request, CancellationToken cancelToken)
            {
                return Task.FromResult(Result.Ok<IEnumerable<ResponseStub>>(new ResponseStub[0]));
            }
        }
    }
}
