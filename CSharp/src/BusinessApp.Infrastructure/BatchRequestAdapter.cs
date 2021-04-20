using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BusinessApp.Kernel;

namespace BusinessApp.Infrastructure
{
    public class BatchRequestAdapter<TRequest, TResponse>
        : IRequestHandler<IEnumerable<TRequest>, IEnumerable<TResponse>>
        where TRequest : notnull
    {
        private readonly IRequestHandler<TRequest, TResponse> inner;

        public BatchRequestAdapter(IRequestHandler<TRequest, TResponse> inner)
        {
            this.inner = inner.NotNull().Expect(nameof(inner));
        }

        public async Task<Result<IEnumerable<TResponse>, Exception>> HandleAsync(
            IEnumerable<TRequest> request,
            CancellationToken cancelToken)
        {
            var results = new List<Result<TResponse, Exception>>();

            foreach(var msg in request)
            {
                results.Add(await inner.HandleAsync(msg, cancelToken));
            }

            if (results.Any(r => r.Kind == ValueKind.Error))
            {
                return Result.Error<IEnumerable<TResponse>>(BatchException.FromResults(results));
            }

            return Result.Ok(results.Select(o => o.Unwrap()));
        }
    }
}
