using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using BusinessApp.Kernel;
using System;

namespace BusinessApp.Infrastructure.WebApi
{
    /// <summary>
    /// Transforms an <see cref="HttpContext" /> into <typeparam name="TRequest" />
    /// and <typeparam name="TResponse" /> objects
    /// </summary>
    public class HttpRequestHandler<TRequest, TResponse> : IHttpRequestHandler<TRequest, TResponse>
        where TRequest : notnull
    {
        private readonly IRequestHandler<TRequest, TResponse> handler;
        private readonly ISerializer serializer;

        public HttpRequestHandler(IRequestHandler<TRequest, TResponse> handler, ISerializer serializer)
        {
            this.handler = handler.NotNull().Expect(nameof(handler));
            this.serializer = serializer.NotNull().Expect(nameof(serializer));
        }

        public async Task<Result<HandlerContext<TRequest, TResponse>, Exception>> HandleAsync(
            HttpContext context, CancellationToken cancelToken)
        {
            var request = await context.Request.DeserializeAsync<TRequest>(serializer, cancelToken);

            return request == null
                ? throw new BusinessAppException("Request cannot be null")
                : await handler.HandleAsync(request, cancelToken)
                    .MapAsync(response => HandlerContext.Create(request, response));
        }
    }
}
