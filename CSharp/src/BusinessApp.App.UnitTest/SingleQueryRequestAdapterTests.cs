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
        private readonly SingleQueryRequestAdapter<QueryStub, ResponseStub> sut;
        private readonly IRequestHandler<QueryStub, IEnumerable<ResponseStub>> inner;

        public SingleQueryRequestAdapterTests()
        {
            cancelToken = A.Dummy<CancellationToken>();
            inner = A.Fake<IRequestHandler<QueryStub, IEnumerable<ResponseStub>>>();

            sut = new SingleQueryRequestAdapter<QueryStub, ResponseStub>(inner);
        }

        public class Constructor : SingleQueryRequestAdapterTests
        {
            public static IEnumerable<object[]> InvalidCtorArgs => new[]
            {
                new object[] { null  },
            };

            [Theory, MemberData(nameof(InvalidCtorArgs))]
            public void InvalidCtorArgs_ExceptionThrown(
                IRequestHandler<QueryStub, IEnumerable<ResponseStub>> s)
            {
                /* Arrange */
                void shouldThrow() => new SingleQueryRequestAdapter<QueryStub, ResponseStub>(s);

                /* Act */
                var ex = Record.Exception(shouldThrow);

                /* Assert */
                Assert.IsType<BusinessAppException>(ex);
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
                A.CallTo(() => inner.HandleAsync(request, cancelToken)).Returns(innerResult);

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
                A.CallTo(() => inner.HandleAsync(request, cancelToken)).Returns(innerResult);

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
                var expectedErr = new BadStateException(expectedErrMsg);
                var responses = new[] { new ResponseStub(), new ResponseStub() };
                var innerResult = Result.Ok<IEnumerable<ResponseStub>>(responses);
                var request = A.Dummy<QueryStub>();
                A.CallTo(() => inner.HandleAsync(request, cancelToken)).Returns(innerResult);

                /* Act */
                var result = await sut.HandleAsync(request, cancelToken);

                /* Assert */
                var ex = Assert.IsType<BadStateException>(result.UnwrapError());
                Assert.Equal(expectedErrMsg, ex.Message);
            }
        }
    }
}
