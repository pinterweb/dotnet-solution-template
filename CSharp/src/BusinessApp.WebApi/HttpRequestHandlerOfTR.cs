using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using BusinessApp.Infrastructure;
using BusinessApp.Domain;
using System;

namespace BusinessApp.WebApi
{
    public class HttpRequestHandler<T, R> : IHttpRequestHandler<T, R>
        where T : notnull
    {
        private readonly IRequestHandler<T, R> handler;
        private readonly ISerializer serializer;

        public HttpRequestHandler(IRequestHandler<T, R> handler, ISerializer serializer)
        {
            this.handler = handler.NotNull().Expect(nameof(handler));
            this.serializer = serializer.NotNull().Expect(nameof(serializer));
        }

        public async Task<Result<HandlerContext<T, R>, Exception>> HandleAsync(
            HttpContext context, CancellationToken cancelToken)
        {
            var request = await context.Request.DeserializeAsync<T>(serializer, cancelToken);

            if (request == null)
            {
                throw new BusinessAppWebApiException("Request cannot be null");
            }

            return await handler.HandleAsync(request, cancelToken)
                .MapAsync(response => HandlerContext.Create(request, response));
        }
    }
}
