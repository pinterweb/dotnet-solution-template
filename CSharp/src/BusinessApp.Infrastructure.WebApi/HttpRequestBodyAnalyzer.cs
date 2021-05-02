﻿using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using BusinessApp.Kernel;

namespace BusinessApp.Infrastructure.WebApi
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

        public async Task HandleAsync<TRequest, TResponse>(HttpContext context) where TRequest : notnull
        {
            var payloadType = await Analyze(context.Request);

            if (payloadType == HttpRequestPayloadType.Array)
            {
                await inner.HandleAsync<IEnumerable<TRequest>, IEnumerable<TResponse>>(context);
            }
            else
            {
                await inner.HandleAsync<TRequest, TResponse>(context);
            }
        }

        public Task<HttpRequestPayloadType> Analyze(HttpRequest request) => !request.IsCommand()
            ? Task.FromResult(HttpRequestPayloadType.Unknown)
            : analyzer.GetBodyTypeAsync(request);
    }
}