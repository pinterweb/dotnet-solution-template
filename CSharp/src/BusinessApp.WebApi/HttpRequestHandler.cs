namespace BusinessApp.WebApi
{
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Http;
    using BusinessApp.App;
    using BusinessApp.Domain;
    using System;

    public class HttpRequestHandler<TRequest, TResponse> : IHttpRequestHandler<TRequest, TResponse>
        where TRequest : notnull
    {
        private readonly IRequestHandler<TRequest, TResponse> handler;
        private readonly ISerializer serializer;

        public HttpRequestHandler(
            IRequestHandler<TRequest, TResponse> handler,
            ISerializer serializer)
        {
            this.handler = Guard.Against.Null(handler).Expect(nameof(handler));
            this.serializer = Guard.Against.Null(serializer).Expect(nameof(serializer));
        }

        public async Task<Result<TResponse, IFormattable>> HandleAsync(HttpContext context,
            CancellationToken cancellationToken)
        {
            var query = await context.DeserializeIntoAsync<TRequest>(serializer, cancellationToken);

            if (query == null)
            {
                throw new BusinessAppWebApiException("Query cannot be null");
            }

            return await handler.HandleAsync(query, cancellationToken);
        }
    }
}
