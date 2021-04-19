using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using System.Buffers;

namespace BusinessApp.WebApi.Json
{
    public class JsonHttpRequestAnalyzer : IHttpRequestAnalyzer
    {
        public async Task<HttpRequestPayloadType> GetBodyTypeAsync(HttpRequest request)
        {
            var bodyReader = request.BodyReader;
            var result = await bodyReader.ReadAsync();
            var firstBit = System.Text.Encoding.UTF8.GetString(result.Buffer.Slice(0,1).ToArray())[0];

            bodyReader.RewindTo(result.Buffer);

            return firstBit == '[' ? HttpRequestPayloadType.Array : HttpRequestPayloadType.Object;
        }
    }
}
