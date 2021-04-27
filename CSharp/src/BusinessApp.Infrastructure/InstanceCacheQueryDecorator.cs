using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using BusinessApp.Kernel;

namespace BusinessApp.Infrastructure
{
    /// <summary>
    /// Caches the query results for the lifetime of the class
    /// </summary>
    public class InstanceCacheQueryDecorator<TRequest, TResponse> : IRequestHandler<TRequest, TResponse>
        where TRequest : notnull, IQuery
    {
        private readonly ConcurrentDictionary<TRequest, Result<TResponse, Exception>> cache;
        private readonly IRequestHandler<TRequest, TResponse> inner;

        public InstanceCacheQueryDecorator(IRequestHandler<TRequest, TResponse> inner)
        {
            this.inner = inner.NotNull().Expect(nameof(inner));
            cache = new ConcurrentDictionary<TRequest, Result<TResponse, Exception>>();
        }

        public async Task<Result<TResponse, Exception>> HandleAsync(
            TRequest request, CancellationToken cancelToken)
        {
            if (cache.TryGetValue(request, out var cachedResult))
            {
                return cachedResult;
            }

            var result = await inner.HandleAsync(request, cancelToken);

            var _ = cache.TryAdd(request, result);

            return result;
        }
    }
}
