namespace BusinessApp.WebApi
{
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Http;
    using BusinessApp.App;
    using BusinessApp.Domain;
    using System;

    public class HttpRequestHandler<TRequest, TResponse> : IHttpRequestHandler<TRequest, TResponse>
    {
        private readonly IRequestHandler<TRequest, TResponse> handler;
        private readonly ISerializer serializer;

        public HttpRequestHandler(
            IRequestHandler<TRequest, TResponse> handler,
            ISerializer serializer)
        {
            this.handler = handler.NotNull().Expect(nameof(handler));
            this.serializer = serializer.NotNull().Expect(nameof(serializer));
        }

        public async Task<Result<TResponse, IFormattable>> HandleAsync(HttpContext context,
            CancellationToken cancellationToken)
        {
            var request = await context.DeserializeIntoAsync<TRequest>(serializer, cancellationToken);

            if (request == null)
            {
                throw new BusinessAppWebApiException("Request cannot be null");
            }

            return await handler.HandleAsync(request, cancellationToken);
        }
    }
}
