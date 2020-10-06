namespace BusinessApp.App
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using BusinessApp.Domain;

    public class BatchRequestDelegator<TRequest, TResponse>
        : IRequestHandler<IEnumerable<TRequest>, IEnumerable<TResponse>>
    {
        private readonly IRequestHandler<TRequest, TResponse> inner;

        public BatchRequestDelegator(IRequestHandler<TRequest, TResponse> inner)
        {
            this.inner = Guard.Against.Null(inner).Expect(nameof(inner));
        }

        public async Task<Result<IEnumerable<TResponse>, IFormattable>> HandleAsync(
            IEnumerable<TRequest> request,
            CancellationToken cancellationToken)
        {
            Guard.Against.Null(request).Expect(nameof(request));

            var results = new List<Result<TResponse, IFormattable>>();

            foreach(var msg in request)
            {
                results.Add(await inner.HandleAsync(msg, cancellationToken));
            }

            if (results.Any(r => r.Kind == ValueKind.Error))
            {
                return Result<IEnumerable<TResponse>, IFormattable>
                    .Error(new BatchException(
                        results.Select(o => o.Into()
                    )));
            }

            return Result<IEnumerable<TResponse>, IFormattable>
                .Ok(results.Select(o => o.Unwrap()));
        }
    }
}
