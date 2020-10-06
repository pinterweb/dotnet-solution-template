namespace BusinessApp.WebApi.UnitTest
{
    using FakeItEasy;
    using Microsoft.AspNetCore.Http;
    using BusinessApp.Domain;
    using Xunit;
    using System.Threading.Tasks;
    using BusinessApp.WebApi.ProblemDetails;
    using BusinessApp.App;
    using BusinessApp.Test;
    using System;

    using _ = System.Int16;
    using System.Collections.Generic;

    public class HttpResponseWriterTests
    {
        private readonly IProblemDetailFactory problemFactory;
        private readonly ISerializer serializer;
        private readonly HttpContext context;
        public readonly HttpResponseWriter sut;

        public HttpResponseWriterTests()
        {
            context = A.Dummy<HttpContext>();
            serializer = A.Fake<ISerializer>();
            problemFactory = A.Fake<IProblemDetailFactory>();
            sut = new HttpResponseWriter(problemFactory, serializer);
        }

        public class Constructor : HttpResponseWriterTests
        {
            public static IEnumerable<object[]> InvalidCtorArgs => new[]
            {
                new object[] { null, A.Dummy<ISerializer>() },
                new object[] { A.Dummy<IProblemDetailFactory>(), null },
            };

            [Theory, MemberData(nameof(InvalidCtorArgs))]
            public void InvalidCtorArgs_ExceptionThrown(IProblemDetailFactory f, ISerializer s)
            {
                /* Arrange */
                void shouldThrow() => new HttpResponseWriter(f, s);

                /* Act */
                var ex = Record.Exception(shouldThrow);

                /* Assert */
                Assert.NotNull(ex);
            }
        }

        public class WriteResponseAsyncWithResult : HttpResponseWriterTests
        {
            [Fact]
            public async Task ResponseStarted_ExceptionThrown()
            {
                /* Arrange */
                A.CallTo(() => context.Response.HasStarted).Returns(true);

                /* Act */
                var ex = await Record.ExceptionAsync(() =>
                    sut.WriteResponseAsync(context, A.Dummy<Result<ResponseStub, _>>()));

                /* Assert */
                Assert.IsType<BusinessAppWebApiException>(ex);
                Assert.Equal(
                    "The response has already started. You cannot write it more than once",
                    ex.Message
                );
            }

            [Fact]
            public async Task ResultIsOk_ValueSerialized()
            {
                /* Arrange */
                var model = A.Dummy<ResponseStub>();
                var result = Result<ResponseStub, _>.Ok(model);

                /* Act */
                await sut.WriteResponseAsync(context, result);

                /* Assert */
                A.CallTo(() => serializer.Serialize(context.Response.Body, model))
                    .MustHaveHappenedOnceExactly();
            }

            [Fact]
            public async Task ResultIsError_ProblemDetailSerialized()
            {
                /* Arrange */
                var error = A.Dummy<IFormattable>();
                var problem = new TestProblemDetail();
                var result = Result<_, IFormattable>.Error(error);
                A.CallTo(() => problemFactory.Create(error)).Returns(problem);

                /* Act */
                await sut.WriteResponseAsync(context, result);

                /* Assert */
                A.CallTo(() => serializer.Serialize(context.Response.Body, problem))
                    .MustHaveHappenedOnceExactly();
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
            public async Task StatusCodeOkOnPut_ChangedTo204(
                int currentStatus, string method, int statusSetCalls)
            {
                /* Arrange */
                A.CallTo(() => context.Response.StatusCode).Returns(currentStatus);
                A.CallTo(() => context.Request.Method).Returns(method);
                var model = A.Dummy<ResponseStub>();
                var result = Result<ResponseStub, _>.Ok(model);

                /* Act */
                await sut.WriteResponseAsync(context, result);

                /* Assert */
                A.CallToSet(() => context.Response.StatusCode).To(204)
                    .MustHaveHappened(statusSetCalls, Times.Exactly);
            }
        }

        public class WriteResponseAsyncWithoutResult : HttpResponseWriterTests
        {
            [Fact]
            public async Task ResponseStarted_ExceptionThrown()
            {
                /* Arrange */
                A.CallTo(() => context.Response.HasStarted).Returns(true);

                /* Act */
                var ex = await Record.ExceptionAsync(() =>
                    sut.WriteResponseAsync(context, A.Dummy<Result<ResponseStub, _>>()));

                /* Assert */
                Assert.IsType<BusinessAppWebApiException>(ex);
                Assert.Equal(
                    "The response has already started. You cannot write it more than once",
                    ex.Message
                );
            }

            [Fact]
            public async Task StatusIsNotSuccess_ProblemDetailSerialized()
            {
                /* Arrange */
                var model = new TestProblemDetail();
                A.CallTo(() => problemFactory.Create(null)).Returns(model);

                /* Act */
                await sut.WriteResponseAsync(context);

                /* Assert */
                A.CallTo(() => serializer.Serialize(context.Response.Body, model))
                    .MustHaveHappenedOnceExactly();
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
            public async Task StatusCodeOkOnPut_ChangedTo204(
                int currentStatus, string method, int statusSetCalls)
            {
                /* Arrange */
                A.CallTo(() => context.Response.StatusCode).Returns(currentStatus);
                A.CallTo(() => context.Request.Method).Returns(method);
                var model = A.Dummy<ResponseStub>();
                var result = Result<ResponseStub, _>.Ok(model);

                /* Act */
                await sut.WriteResponseAsync(context, result);

                /* Assert */
                A.CallToSet(() => context.Response.StatusCode).To(204)
                    .MustHaveHappened(statusSetCalls, Times.Exactly);
            }
        }

        private sealed class TestProblemDetail : ProblemDetail
        {
            public TestProblemDetail() : base(1)
            {
            }
        }
    }
}
