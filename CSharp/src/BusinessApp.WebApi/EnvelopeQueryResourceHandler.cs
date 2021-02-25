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
        where TRequest : IQuery
    {
        private readonly IRequestHandler<TRequest, EnvelopeContract<TResponse>> handler;
        private readonly ISerializer serializer;

        public EnvelopeQueryResourceHandler(
            IRequestHandler<TRequest, EnvelopeContract<TResponse>> handler,
            ISerializer serializer)
        {
            this.handler = handler.NotNull().Expect(nameof(handler));
            this.serializer = serializer.NotNull().Expect(nameof(serializer));
        }

        public async Task<Result<IEnumerable<TResponse>, IFormattable>> HandleAsync(HttpContext context,
            CancellationToken cancelToken)
        {
            var query = await context.Request.DeserializeAsync<TRequest>(serializer, cancelToken);

            if (query == null)
            {
                throw new BusinessAppWebApiException("Query cannot be null");
            }

            return (await handler.HandleAsync(query, cancelToken))
                .Map(envelope =>
                {
                    context.Response.Headers.Add("Access-Control-Expose-Headers", new[] { "VND.parkeremg.pagination" });
                    context.Response.Headers.Add("VND.parkeremg.pagination",
                        new StringValues(envelope.Pagination.ToHeaderValue()));

                    return envelope.Data;
                });
        }
    }
}
