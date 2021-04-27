using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using BusinessApp.Kernel;
using BusinessApp.WebApi.Json;
using FakeItEasy;
using Microsoft.AspNetCore.Http;
using Xunit;

namespace BusinessApp.WebApi.UnitTest.Json
{
    using Result = Kernel.Result<HandlerContext<RequestStub, ResponseStub>, System.Exception>;
    using HandlerResponse = HandlerContext<RequestStub, ResponseStub>;

    public class JsonHttpDecoratorTests
    {
        private readonly IHttpRequestHandler<RequestStub, ResponseStub> inner;
        private readonly HttpContext context;
        private readonly CancellationToken cancelToken;
        private readonly JsonHttpDecorator<RequestStub, ResponseStub> sut;

        public JsonHttpDecoratorTests()
        {
            inner = A.Fake<IHttpRequestHandler<RequestStub, ResponseStub>>();
            context = A.Fake<HttpContext>();
            cancelToken = A.Dummy<CancellationToken>();

            sut = new JsonHttpDecorator<RequestStub, ResponseStub>(inner);
        }

        public class Constructor : JsonHttpDecoratorTests
        {
            public static IEnumerable<object[]> InvalidInputs => new[]
            {
                new object[] { null },
            };

            [Theory, MemberData(nameof(InvalidInputs))]
            public void InvalidInputs_ExceptionThrown(
                IHttpRequestHandler<RequestStub, ResponseStub> i)
            {
                /* Arrange */
                void shouldThrow() => new JsonHttpDecorator<RequestStub, ResponseStub>(i);

                /* Act */
                var ex = Record.Exception(shouldThrow);

                /* Assert */
                Assert.IsType<BusinessAppException>(ex);
            }
        }

        public class HandleAsync : JsonHttpDecoratorTests
        {
            public static IEnumerable<object[]> SaveMethods => new[]
            {
                new object[] { HttpMethods.Post },
                new object[] { HttpMethods.Put },
                new object[] { HttpMethods.Patch }
            };

            [Theory, MemberData(nameof(SaveMethods))]
            public async Task NotValidContent_ResultErrorReturned(string method)
            {
                /* Arrange */
                var error = new BusinessAppException(
                    "Expected content-type to be application/json");
                A.CallTo(() => context.Request.ContentType).Returns("text");
                A.CallTo(() => context.Request.Method).Returns(method);

                /* Act */
                var result = await sut.HandleAsync(context, cancelToken);

                /* Assert */
                Assert.Equal(error.Message, result.UnwrapError().Message);
            }

            [Theory, MemberData(nameof(SaveMethods))]
            public async Task NotValidContent_UnsupportedMediaResponseSet(string method)
            {
                /* Arrange */
                A.CallTo(() => context.Request.ContentType).Returns("text");
                A.CallTo(() => context.Request.Method).Returns(method);

                /* Act */
                var result = await sut.HandleAsync(context, cancelToken);

                /* Assert */
                A.CallToSet(() => context.Response.StatusCode).To(415)
                    .MustHaveHappenedOnceExactly();
            }

            [Fact]
            public async Task WhenInnerResultSuccessful_ApplicationJsonContentReturned()
            {
                /* Arrange */
                A.CallTo(() => inner.HandleAsync(context, cancelToken))
                    .Returns(Result.Ok(A.Dummy<HandlerResponse>()));

                /* Act */
                var result = await sut.HandleAsync(context, cancelToken);

                /* Assert */
                A.CallToSet(() => context.Response.ContentType).To("application/json")
                    .MustHaveHappenedOnceExactly();
            }

            [Fact]
            public async Task WhenInnerResultError_ApplicationProblemJsonContentReturned()
            {
                /* Arrange */
                A.CallTo(() => inner.HandleAsync(context, cancelToken))
                    .Returns(Result.Error(A.Dummy<Exception>()));

                /* Act */
                var result = await sut.HandleAsync(context, cancelToken);

                /* Assert */
                A.CallToSet(() => context.Response.ContentType).To("application/problem+json")
                    .MustHaveHappenedOnceExactly();
            }

            [Fact]
            public async Task WhenInnerHandlerThrows_ApplicationProblemJsonContentReturned()
            {
                /* Arrange */
                A.CallTo(() => inner.HandleAsync(context, cancelToken))
                    .Throws<Exception>();

                /* Act */
                var result = await Record.ExceptionAsync(() => sut.HandleAsync(context, cancelToken));

                /* Assert */
                A.CallToSet(() => context.Response.ContentType).To("application/problem+json")
                    .MustHaveHappenedOnceExactly();
            }
        }
    }
}
