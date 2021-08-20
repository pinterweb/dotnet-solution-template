using FakeItEasy;
using Microsoft.AspNetCore.Http;
using Xunit;
using System.Threading.Tasks;
using System.Collections.Generic;
using BusinessApp.Kernel;
using BusinessApp.Test.Shared;

namespace BusinessApp.WebApi.UnitTest
{
    public class HttpRequestBodyAnalyzerTests
    {
        private readonly IHttpRequestHandler inner;
        private readonly IHttpRequestAnalyzer analyzer;
        private HttpRequestBodyAnalyzer sut;

        public HttpRequestBodyAnalyzerTests()
        {
            inner = A.Fake<IHttpRequestHandler>();
            analyzer = A.Fake<IHttpRequestAnalyzer>();
            sut = new HttpRequestBodyAnalyzer(inner, analyzer);
        }

        public class Constructor : HttpRequestBodyAnalyzerTests
        {
            public static IEnumerable<object[]> InvalidCtorArgs => new[]
            {
                new object[]
                {
                    null,
                    A.Dummy<IHttpRequestAnalyzer>()
                },
                new object[]
                {
                    A.Dummy<IHttpRequestHandler>(),
                    null
                },
            };

            [Theory, MemberData(nameof(InvalidCtorArgs))]
            public void InvalidCtorArgs_ExceptionThrown(IHttpRequestHandler i,
                IHttpRequestAnalyzer a)
            {
                /* Arrange */
                void shouldThrow() => new HttpRequestBodyAnalyzer(i, a);

                /* Act */
                var ex = Record.Exception(shouldThrow);

                /* Assert */
                Assert.IsType<BusinessAppException>(ex);
            }
        }

        public class HandleAsync : HttpRequestBodyAnalyzerTests
        {
            private readonly HttpContext context;

            public HandleAsync()
            {
                context = A.Fake<HttpContext>();
            }

            [Theory]
            [InlineData("GET")]
            [InlineData("DELETE")]
            [InlineData("OPTIONS")]
            public async Task RequestMethodIsQuery_SingleHandlerCalled(string method)
            {
                /* Arrange */
                A.CallTo(() => context.Request.Method).Returns(method);

                /* Act */
                await sut.HandleAsync<RequestStub, ResponseStub>(context);

                /* Assert */
                A.CallTo(() => inner.HandleAsync<RequestStub, ResponseStub>(context))
                    .MustHaveHappenedOnceExactly();
            }

            [Theory]
            [InlineData("PATCH")]
            [InlineData("POST")]
            [InlineData("PUT")]
            public async Task RequestMethodIsArrayCommand_IEnumerableHandlerCalled(string method)
            {
                /* Arrange */
                A.CallTo(() => context.Request.Method).Returns(method);
                A.CallTo(() => analyzer.GetBodyTypeAsync(context.Request))
                    .Returns(HttpRequestPayloadType.Array);

                /* Act */
                await sut.HandleAsync<RequestStub, ResponseStub>(context);

                /* Assert */
                A.CallTo(() => inner.HandleAsync<IEnumerable<RequestStub>, IEnumerable<ResponseStub>>(context))
                    .MustHaveHappenedOnceExactly();
            }

            [Theory]
            [InlineData("PATCH")]
            [InlineData("POST")]
            [InlineData("PUT")]
            public async Task RequestMethodIsObjectCommand_SingleHandlerCalled(string method)
            {
                /* Arrange */
                A.CallTo(() => context.Request.Method).Returns(method);
                A.CallTo(() => analyzer.GetBodyTypeAsync(context.Request))
                    .Returns(HttpRequestPayloadType.SingleObject);

                /* Act */
                await sut.HandleAsync<RequestStub, ResponseStub>(context);

                /* Assert */
                A.CallTo(() => inner.HandleAsync<RequestStub, ResponseStub>(context))
                    .MustHaveHappenedOnceExactly();
            }
        }
    }
}
