namespace BusinessApp.WebApi.UnitTest.Json
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using BusinessApp.Domain;
    using BusinessApp.WebApi.Json;
    using FakeItEasy;
    using Microsoft.AspNetCore.Http;
    using Xunit;

    public class JsonHttpDecoratorTests
    {
        private readonly IHttpRequestHandler<RequestStub, ResponseStub> inner;
        private readonly IResponseWriter modelWriter;
        private readonly HttpContext context;
        private readonly CancellationToken token;
        private readonly JsonHttpDecorator<RequestStub, ResponseStub> sut;

        public JsonHttpDecoratorTests()
        {
            inner = A.Fake<IHttpRequestHandler<RequestStub, ResponseStub>>();
            modelWriter = A.Fake<IResponseWriter>();
            context = A.Fake<HttpContext>();
            token = A.Dummy<CancellationToken>();

            sut = new JsonHttpDecorator<RequestStub, ResponseStub>(inner, modelWriter);
        }

        public class Constructor : JsonHttpDecoratorTests
        {
            public static IEnumerable<object[]> InvalidInputs => new[]
            {
                new object[] { null, A.Dummy<IResponseWriter>() },
                new object[]
                {
                    A.Dummy<IHttpRequestHandler<RequestStub, ResponseStub>>(),
                    null
                }
            };

            [Theory, MemberData(nameof(InvalidInputs))]
            public void InvalidInputs_ExceptionThrown(
                IHttpRequestHandler<RequestStub, ResponseStub> i,
                IResponseWriter w)
            {
                /* Arrange */
                void shouldThrow() => new JsonHttpDecorator<RequestStub, ResponseStub>(i, w);

                /* Act */
                var ex = Record.Exception(shouldThrow);

                /* Assert */
                Assert.IsType<BadStateException>(ex);
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
                IFormattable error = $"Expected content-type to be application/json";
                A.CallTo(() => context.Request.ContentType).Returns("text");
                A.CallTo(() => context.Request.Method).Returns(method);

                /* Act */
                var result = await sut.HandleAsync(context, token);

                /* Assert */
                Assert.Equal(
                    Result<ResponseStub, IFormattable>.Error(error),
                    result
                );
            }

            [Theory, MemberData(nameof(SaveMethods))]
            public async Task NotValidContent_UnsupportedMediaResponseSet(string method)
            {
                /* Arrange */
                IFormattable error = $"Expected content-type to be application/json";
                A.CallTo(() => context.Request.ContentType).Returns("text");
                A.CallTo(() => context.Request.Method).Returns(method);

                /* Act */
                var result = await sut.HandleAsync(context, token);

                /* Assert */
                A.CallToSet(() => context.Response.StatusCode).To(415)
                    .MustHaveHappenedOnceExactly();
            }

            [Fact]
            public async Task BeforeWriteResponse_ContentTypeSet()
            {
                /* Arrange */
                var innerReturn = Result.Ok.Into<ResponseStub>();
                A.CallTo(() => inner.HandleAsync(context, token)).Returns(innerReturn);

                /* Act */
                var result = await Record.ExceptionAsync(() => sut.HandleAsync(context, token));

                /* Assert */
                A.CallToSet(() => context.Response.ContentType).To("application/json")
                    .MustHaveHappened()
                    .Then(A.CallTo(() => modelWriter.WriteResponseAsync(context, innerReturn))
                        .MustHaveHappened()
                    );
            }

            [Fact]
            public async Task WhenInnerSuccessful_ApplicationJsonContentReturned()
            {
                /* Arrange */
                A.CallTo(() => inner.HandleAsync(context, token))
                    .Returns(Result.Ok.Into<ResponseStub>());

                /* Act */
                var result = await sut.HandleAsync(context, token);

                /* Assert */
                A.CallToSet(() => context.Response.ContentType).To("application/json")
                    .MustHaveHappenedOnceExactly();
            }

            [Fact]
            public async Task WhenInnerFailed_ApplicationProblemJsonContentReturned()
            {
                /* Arrange */
                A.CallTo(() => inner.HandleAsync(context, token))
                    .Returns(Result.Error($"foobar").Into<ResponseStub>());

                /* Act */
                var result = await sut.HandleAsync(context, token);

                /* Assert */
                A.CallToSet(() => context.Response.ContentType).To("application/problem+json")
                    .MustHaveHappenedOnceExactly();
            }

            [Fact]
            public async Task WhenInnerThrows_ApplicationProblemJsonContentReturned()
            {
                /* Arrange */
                A.CallTo(() => inner.HandleAsync(context, token))
                    .Throws<Exception>();

                /* Act */
                var result = await Record.ExceptionAsync(() => sut.HandleAsync(context, token));

                /* Assert */
                A.CallToSet(() => context.Response.ContentType).To("application/problem+json")
                    .MustHaveHappenedOnceExactly();
            }

            [Fact]
            public async Task WhenWriterThrows_ApplicationProblemJsonContentReturned()
            {
                /* Arrange */
                var innerReturn = Result.Ok.Into<ResponseStub>();
                A.CallTo(() => inner.HandleAsync(context, token)).Returns(innerReturn);
                A.CallTo(() => modelWriter.WriteResponseAsync(context, innerReturn))
                    .Throws<Exception>();

                /* Act */
                var result = await Record.ExceptionAsync(() => sut.HandleAsync(context, token));

                /* Assert */
                A.CallToSet(() => context.Response.ContentType).To("application/problem+json")
                    .MustHaveHappenedOnceExactly();
            }
        }

        public class RequestStub {}
        public class ResponseStub {}
    }
}
