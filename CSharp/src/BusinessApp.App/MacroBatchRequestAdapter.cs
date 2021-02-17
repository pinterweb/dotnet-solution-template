namespace BusinessApp.App
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using BusinessApp.Domain;

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

        public async Task<Result<TResponse, IFormattable>> HandleAsync(TMacro macro,
            CancellationToken cancelToken)
        {
            macro.NotNull().Expect(nameof(macro));

            var payloads = await expander.ExpandAsync(macro, cancelToken);

            if (!payloads.Any())
            {
                throw new BusinessAppAppException(
                    "The macro you ran expected to find records to change, but none were " +
                    "found"
                );
            }

            return await handler.HandleAsync(payloads, cancelToken);
        }
    }
}
