using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BusinessApp.Kernel;

namespace BusinessApp.Infrastructure
{
    /// <summary>
    /// Accepts an `IEnumerable` request and calls the "inner" handler that
    /// handles the single request.
    /// </summary>
    /// <remarks>
    /// This is useful so you do not have to write 2 handlers, one for the
    /// `IEnumerable` request and one for the single request. The single
    /// handler is your api logic to change data. With this adapter you do not
    /// have to write multiple handlers to handle one or many of the same request.
    /// </remarks>
    public class BatchRequestAdapter<TRequest, TResponse>
        : IRequestHandler<IEnumerable<TRequest>, IEnumerable<TResponse>>
        where TRequest : notnull
    {
        private readonly IRequestHandler<TRequest, TResponse> inner;

        public BatchRequestAdapter(IRequestHandler<TRequest, TResponse> inner)
            => this.inner = inner.NotNull().Expect(nameof(inner));

        public async Task<Result<IEnumerable<TResponse>, Exception>> HandleAsync(
            IEnumerable<TRequest> request, CancellationToken cancelToken)
        {
            var results = new List<Result<TResponse, Exception>>();

            foreach (var msg in request)
            {
                results.Add(await inner.HandleAsync(msg, cancelToken));
            }

            return results.Any(r => r.Kind == ValueKind.Error)
                ? Result.Error<IEnumerable<TResponse>>(BatchException.FromResults(results))
                : Result.Ok(results.Select(o => o.Unwrap()));
        }
    }
}
