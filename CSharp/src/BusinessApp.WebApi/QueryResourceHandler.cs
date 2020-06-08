namespace BusinessApp.WebApi
{
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Http;
    using BusinessApp.App;
    using BusinessApp.Domain;

    /// <summary>
    /// Basic http handler for queries
    /// </summary>
    public class QueryResourceHandler<TRequest, TResponse> : IResourceHandler<TRequest, TResponse>
        where TRequest : class, IQuery<TResponse>, new()
    {
        private readonly IQueryHandler<TRequest, TResponse> handler;
        private readonly ISerializer serializer;

        public QueryResourceHandler(
            IQueryHandler<TRequest, TResponse> handler,
            ISerializer serializer)
        {
            this.handler = GuardAgainst.Null(handler, nameof(handler));
            this.serializer = GuardAgainst.Null(serializer, nameof(serializer));
        }

        public async Task<TResponse> HandleAsync(HttpContext context,
            CancellationToken cancellationToken)
        {
            var query = context.DeserializeInto<TRequest>(serializer);

            if (query == null)
            {
                query = new TRequest();
            }

            return await handler.HandleAsync(query, cancellationToken);
        }
    }
}
