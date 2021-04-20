using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using BusinessApp.Domain;

namespace BusinessApp.Infrastructure
{
    /// <summary>
    /// Caches the query results for the lifetime of the class
    /// </summary>
    public class InstanceCacheQueryDecorator<TQuery, TResult> : IRequestHandler<TQuery, TResult>
        where TQuery : notnull, IQuery
    {
        private readonly ConcurrentDictionary<TQuery, Result<TResult, Exception>> cache;
        private readonly IRequestHandler<TQuery, TResult> inner;

        public InstanceCacheQueryDecorator(IRequestHandler<TQuery, TResult> inner)
        {
            this.inner = inner.NotNull().Expect(nameof(inner));
            cache = new ConcurrentDictionary<TQuery, Result<TResult, Exception>>();
        }

        public async Task<Result<TResult, Exception>> HandleAsync(
            TQuery query, CancellationToken cancelToken)
        {
            if (cache.TryGetValue(query, out Result<TResult, Exception> cachedResult))
            {
                return cachedResult;
            }

            var result = await inner.HandleAsync(query, cancelToken);

            var _ = cache.TryAdd(query, result);

            return result;
        }
    }
}
