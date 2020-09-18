namespace BusinessApp.App
{
    using System.Collections.Concurrent;
    using System.Threading;
    using System.Threading.Tasks;
    using BusinessApp.Domain;

    /// <summary>
    /// Caches the query results for the lifetime of the class
    /// </summary>
    public class QueryLifetimeCacheDecorator<TQuery, TResult> : IQueryHandler<TQuery, TResult>
        where TQuery : IQuery<TResult>
    {
        private readonly ConcurrentDictionary<TQuery, TResult> cache;
        private readonly IQueryHandler<TQuery, TResult> inner;

        public QueryLifetimeCacheDecorator(IQueryHandler<TQuery, TResult> inner)
        {
            this.inner = GuardAgainst.Null(inner, nameof(inner));
            cache = new ConcurrentDictionary<TQuery, TResult>();
        }

        public async Task<TResult> HandleAsync(TQuery query, CancellationToken cancellationToken)
        {
            if (cache.TryGetValue(query, out TResult cachedResult))
            {
                return cachedResult;
            }

            var result = await inner.HandleAsync(query, cancellationToken);

            var _ = cache.TryAdd(query, result);

            return result;
        }
    }
}
