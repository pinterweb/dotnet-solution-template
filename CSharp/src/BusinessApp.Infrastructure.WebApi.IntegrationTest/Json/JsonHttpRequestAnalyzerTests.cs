using System.Buffers;
using System.IO.Pipelines;
using System.Text;
using System.Threading.Tasks;
using BusinessApp.Infrastructure.WebApi.Json;
using FakeItEasy;
using Microsoft.AspNetCore.Http;
using Xunit;

namespace BusinessApp.Infrastructure.WebApi.IntegrationTest.Json
{
    public class JsonHttpRequestAnalyzerTests
    {
        private readonly JsonHttpRequestAnalyzer sut;

        public JsonHttpRequestAnalyzerTests()
        {
            sut = new JsonHttpRequestAnalyzer();
        }

        [Fact]
        public async Task HasStartOfArray_HttpRequestPayloadTypeArrayReturned()
        {
            /* Arrange */
            var context = A.Fake<HttpContext>();
            var bytes = Encoding.UTF8.GetBytes("[{\"id\":1}]");
            var result = new ValueTask<ReadResult>(
                new ReadResult(new ReadOnlySequence<byte>(bytes), true, true));
            A.CallTo(() => context.Request.BodyReader.ReadAsync(default)).Returns(result);

            /* Act */
            var type = await sut.GetBodyTypeAsync(context.Request);

            /* Assert */
            Assert.Equal(HttpRequestPayloadType.Array, type);
        }

        [Fact]
        public async Task HasStartOfObject_HttpRequestPayloadTypeArrayReturned()
        {
            /* Arrange */
            var context = A.Fake<HttpContext>();
            var bytes = Encoding.UTF8.GetBytes("{\"id\":1}");
            var result = new ValueTask<ReadResult>(
                new ReadResult(new ReadOnlySequence<byte>(bytes), true, true));
            A.CallTo(() => context.Request.BodyReader.ReadAsync(default)).Returns(result);

            /* Act */
            var type = await sut.GetBodyTypeAsync(context.Request);

            /* Assert */
            Assert.Equal(HttpRequestPayloadType.SingleObject, type);
        }
    }
}
