namespace BusinessApp.WebApi
{
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.Primitives;
    using BusinessApp.App;
    using BusinessApp.Domain;
    using System;

    public class EnvelopeQueryResourceHandler<TRequest, TResponse> :
        IHttpRequestHandler<TRequest, IEnumerable<TResponse>>
        where TRequest : IQuery<EnvelopeContract<TResponse>>, new()
    {
        private readonly IQueryHandler<TRequest, EnvelopeContract<TResponse>> handler;
        private readonly ISerializer serializer;

        public EnvelopeQueryResourceHandler(
            IQueryHandler<TRequest, EnvelopeContract<TResponse>> handler,
            ISerializer serializer)
        {
            this.handler = Guard.Against.Null(handler).Expect(nameof(handler));
            this.serializer = Guard.Against.Null(serializer).Expect(nameof(serializer));
        }

        public async Task<Result<IEnumerable<TResponse>, IFormattable>> HandleAsync(HttpContext context, CancellationToken cancellationToken)
        {
            var query = await context.DeserializeIntoAsync<TRequest>(serializer, cancellationToken);

            if (query == null)
            {
                query = new TRequest();
            }

            return (await handler.HandleAsync(query, cancellationToken))
                .Map(envelope =>
                {
                    context.Response.Headers.Add("Access-Control-Expose-Headers", new[] { "VND.parkeremg.pagination" });
                    context.Response.Headers.Add("VND.parkeremg.pagination",
                        new StringValues(envelope.Pagination.ToHeaderValue()));

                    return Result<IEnumerable<TResponse>, IFormattable>.Ok(envelope.Data);
                });
        }
    }
}
