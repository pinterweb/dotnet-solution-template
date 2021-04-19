using FakeItEasy;
using Microsoft.AspNetCore.Http;
using Xunit;
using System.Threading.Tasks;
using BusinessApp.WebApi.ProblemDetails;
using BusinessApp.App;
using System;
using System.Collections.Generic;
using System.Threading;
using BusinessApp.Domain;

namespace BusinessApp.WebApi.UnitTest
{
    using HandlerResult = Domain.Result<HandlerContext<RequestStub, ResponseStub>, System.Exception>;

    public class HttpResponseDecoratorTests
    {
        private readonly IHttpRequestHandler<RequestStub, ResponseStub> inner;
        private readonly IProblemDetailFactory problemFactory;
        private readonly ISerializer serializer;
        private readonly HttpContext context;
        private readonly HttpResponseDecorator<RequestStub, ResponseStub> sut;

        public HttpResponseDecoratorTests()
        {
            context = A.Dummy<HttpContext>();
            serializer = A.Fake<ISerializer>();
            problemFactory = A.Fake<IProblemDetailFactory>();
            inner = A.Fake<IHttpRequestHandler<RequestStub, ResponseStub>>();
            sut = new HttpResponseDecorator<RequestStub, ResponseStub>(inner,
                problemFactory, serializer);
        }

        public class Constructor : HttpResponseDecoratorTests
        {
            public static IEnumerable<object[]> InvalidCtorArgs => new[]
            {
                new object[]
                {
                    null,
                    A.Dummy<IProblemDetailFactory>(),
                    A.Dummy<ISerializer>()
                },
                new object[]
                {
                    A.Dummy<IHttpRequestHandler<RequestStub, ResponseStub>>(),
                    null,
                    A.Dummy<ISerializer>()
                },
                new object[]
                {
                    A.Dummy<IHttpRequestHandler<RequestStub, ResponseStub>>(),
                    A.Dummy<IProblemDetailFactory>(),
                    null
                },
            };

            [Theory, MemberData(nameof(InvalidCtorArgs))]
            public void InvalidCtorArgs_ExceptionThrown(
                IHttpRequestHandler<RequestStub, ResponseStub> i,
                IProblemDetailFactory f, ISerializer s)
            {
                /* Arrange */
                void shouldThrow() =>
                    new HttpResponseDecorator<RequestStub, ResponseStub>(i, f, s);

                /* Act */
                var ex = Record.Exception(shouldThrow);

                /* Assert */
                Assert.IsType<BusinessAppException>(ex);
            }
        }

        public class HandleAsync : HttpResponseDecoratorTests
        {
            private readonly CancellationToken cancelToken;

            public HandleAsync()
            {
                cancelToken = A.Dummy<CancellationToken>();
            }

            [Fact]
            public async Task ResponseStarted_ExceptionThrown()
            {
                /* Arrange */
                A.CallTo(() => inner.HandleAsync(context, cancelToken))
                    .Returns(A.Dummy<HandlerResult>());
                A.CallTo(() => context.Response.HasStarted).Returns(true);

                /* Act */
                var ex = await Record.ExceptionAsync(() => sut.HandleAsync(context, cancelToken));

                /* Assert */
                Assert.IsType<BusinessAppWebApiException>(ex);
                Assert.Equal(
                    "The response has already started outside the expected write decorator. " +
                    "You cannot write it more than once.",
                    ex.Message
                );
            }

            [Fact]
            public async Task ResultIsOk_ValueSerialized()
            {
                /* Arrange */
                var blob = new byte[0];
                var model = A.Dummy<ResponseStub>();
                var result = HandlerResult.Ok(HandlerContext.Create(A.Dummy<RequestStub>(), model));
                A.CallTo(() => inner.HandleAsync(context, cancelToken)).Returns(result);
                A.CallTo(() => serializer.Serialize(model)).Returns(blob);

                /* Act */
                await sut.HandleAsync(context, cancelToken);

                /* Assert */
                A.CallTo(() => context.Response.BodyWriter.WriteAsync(blob, cancelToken))
                    .MustHaveHappenedOnceExactly();
            }

            [Fact]
            public async Task ResultIsError_ProblemDetailSerialized()
            {
                /* Arrange */
                var error = A.Dummy<Exception>();
                ProblemDetail problem = new TestProblemDetail();
                var result = HandlerResult.Error(error);
                A.CallTo(() => problemFactory.Create(error)).Returns(problem);
                A.CallTo(() => inner.HandleAsync(context, cancelToken)).Returns(result);

                /* Act */
                await sut.HandleAsync(context, cancelToken);

                /* Assert */
                A.CallTo(() => serializer.Serialize(problem)).MustHaveHappenedOnceExactly();
            }

            [Fact]
            public async Task ResultIsError_StatusCodeSetFromProblem()
            {
                /* Arrange */
                var error = A.Dummy<Exception>();
                ProblemDetail problem = new TestProblemDetail(400);
                var result = HandlerResult.Error(error);
                A.CallTo(() => problemFactory.Create(error)).Returns(problem);
                A.CallTo(() => inner.HandleAsync(context, cancelToken)).Returns(result);

                /* Act */
                await sut.HandleAsync(context, cancelToken);

                /* Assert */
                Assert.Equal(400, context.Response.StatusCode);
            }

            [Theory]
            [InlineData(200, "get", 0)]
            [InlineData(200, "post", 0)]
            [InlineData(200, "put", 1)]
            [InlineData(200, "PUT", 1)]
            [InlineData(200, "delete", 1)]
            [InlineData(200, "DELETE", 1)]
            [InlineData(500, "put", 0)]
            [InlineData(500, "delete", 0)]
            public async Task OkNoContentMethods_Status204Set(
                int currentStatus, string method, int statusSetCalls)
            {
                /* Arrange */
                var model = A.Dummy<ResponseStub>();
                var result = HandlerResult.Ok(HandlerContext.Create(A.Dummy<RequestStub>(), model));
                A.CallTo(() => context.Response.StatusCode).Returns(currentStatus);
                A.CallTo(() => context.Request.Method).Returns(method);
                A.CallTo(() => inner.HandleAsync(context, cancelToken)).Returns(result);

                /* Act */
                await sut.HandleAsync(context, cancelToken);

                /* Assert */
                A.CallToSet(() => context.Response.StatusCode).To(204)
                    .MustHaveHappened(statusSetCalls, Times.Exactly);
            }

            [Fact]
            public async Task OkNoContent_NoBodyWritten()
            {
                /* Arrange */
                A.CallTo(() => context.Response.StatusCode).Returns(200);
                A.CallTo(() => context.Request.Method).Returns("put");
                A.CallTo(() => inner.HandleAsync(context, cancelToken))
                    .Returns(A.Dummy<HandlerResult>());

                /* Act */
                await sut.HandleAsync(context, cancelToken);

                /* Assert */
                A.CallTo(() => context.Response.BodyWriter.WriteAsync(A<ReadOnlyMemory<byte>>._, cancelToken))
                    .MustNotHaveHappened();
            }
        }

        private sealed class TestProblemDetail : ProblemDetail
        {
            public TestProblemDetail(int statusCode = 1) : base(statusCode)
            {}
        }
    }
}
