namespace BusinessApp.WebApi
{
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.Primitives;
    using BusinessApp.App;
    using BusinessApp.Domain;

    public class EnvelopeQueryResourceHandler<TRequest, TResponse> :
        IResourceHandler<TRequest, IEnumerable<TResponse>>
        where TRequest : IQuery<EnvelopeContract<TResponse>>, new()
    {
        private readonly IQueryHandler<TRequest, EnvelopeContract<TResponse>> handler;
        private readonly ISerializer serializer;

        public EnvelopeQueryResourceHandler(
            IQueryHandler<TRequest, EnvelopeContract<TResponse>> handler,
            ISerializer serializer)
        {
            this.handler = GuardAgainst.Null(handler, nameof(handler));
            this.serializer = GuardAgainst.Null(serializer, nameof(serializer));
        }

        public async Task<IEnumerable<TResponse>> HandleAsync(HttpContext context, CancellationToken cancellationToken)
        {
            var query = context.DeserializeInto<TRequest>(serializer);

            if (query == null)
            {
                query = new TRequest();
            }

            var resource = await handler.HandleAsync(query, cancellationToken);

            context.Response.Headers.Add("Access-Control-Expose-Headers", new[] { "VND.parkeremg.pagination" });
            context.Response.Headers.Add("VND.parkeremg.pagination",
                new StringValues(resource.Pagination.ToHeaderValue()));

            return resource.Data;
        }
    }
}
