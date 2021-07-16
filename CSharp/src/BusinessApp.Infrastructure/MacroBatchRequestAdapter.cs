using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BusinessApp.Kernel;

namespace BusinessApp.Infrastructure
{
    /// <summary>
    /// Accepts a single request and expands it into an `IEnumerable` request
    /// using the <see cref="IMacro{TRequest}" />
    /// </summary>
    /// <remarks>
    /// This is useful when you need to change many values based on a query
    //  and those changes are the same for all objects returned by the query.
    // The alternative would be sending many requests.
    /// <summary>
    public class MacroBatchRequestAdapter<TMacro, TRequest, TResponse>
        : IRequestHandler<TMacro, TResponse>
        where TMacro : IMacro<TRequest>
    {
        private readonly IBatchMacro<TMacro, TRequest> expander;
        private readonly IRequestHandler<IEnumerable<TRequest>, TResponse> handler;

        public MacroBatchRequestAdapter(
            IBatchMacro<TMacro, TRequest> expander,
            IRequestHandler<IEnumerable<TRequest>, TResponse> handler)
        {
            this.expander = expander.NotNull().Expect(nameof(expander));
            this.handler = handler.NotNull().Expect(nameof(handler));
        }

        public async Task<Result<TResponse, Exception>> HandleAsync(TMacro request,
            CancellationToken cancelToken)
        {
            var payloads = await expander.ExpandAsync(request, cancelToken);

            return !payloads.Any()
                ? Result.Error<TResponse>(new BusinessAppException(
                    "The macro you ran expected to find records to change, but none were " +
                    "found"))
                : await handler.HandleAsync(payloads, cancelToken);
        }
    }
}
