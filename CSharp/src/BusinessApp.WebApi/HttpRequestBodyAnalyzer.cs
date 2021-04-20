using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using BusinessApp.Kernel;

namespace BusinessApp.WebApi
{
    public class HttpRequestBodyAnalyzer : IHttpRequestHandler
    {
        private readonly IHttpRequestHandler inner;
        private readonly IHttpRequestAnalyzer analyzer;

        public HttpRequestBodyAnalyzer(IHttpRequestHandler inner, IHttpRequestAnalyzer analyzer)
        {
            this.inner = inner.NotNull().Expect(nameof(inner));
            this.analyzer = analyzer.NotNull().Expect(nameof(analyzer));
        }

        public async Task HandleAsync<T, R>(HttpContext context) where T : notnull
        {
            var payloadType = await Analyze(context.Request);

            if (payloadType == HttpRequestPayloadType.Array)
            {
                await inner.HandleAsync<IEnumerable<T>, IEnumerable<R>>(context);
            }
            else
            {
                await inner.HandleAsync<T, R>(context);
            }
        }

        public Task<HttpRequestPayloadType> Analyze(HttpRequest request)
        {
            if (!request.IsCommand()) return Task.FromResult(HttpRequestPayloadType.Unknown);

            return analyzer.GetBodyTypeAsync(request);
        }
    }
}
