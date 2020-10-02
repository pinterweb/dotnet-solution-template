namespace BusinessApp.App
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using BusinessApp.Domain;

    public class BatchMacroCommandDecorator<TMacro, TRequest, TResponse>
        : IRequestHandler<TMacro, TResponse>
        where TMacro : IMacro<TRequest>
    {
        private readonly IBatchMacro<TMacro, TRequest> expander;
        private readonly IRequestHandler<IEnumerable<TRequest>, TResponse> handler;

        public BatchMacroCommandDecorator(
            IBatchMacro<TMacro, TRequest> expander,
            IRequestHandler<IEnumerable<TRequest>, TResponse> handler)
        {
            this.expander = Guard.Against.Null(expander).Expect(nameof(expander));
            this.handler = Guard.Against.Null(handler).Expect(nameof(handler));
        }

        public async Task<Result<TResponse, IFormattable>> HandleAsync(
            TMacro macro,
            CancellationToken cancellationToken)
        {
            Guard.Against.Null(macro).Expect(nameof(macro));

            var payloads = await expander.ExpandAsync(macro, cancellationToken);

            if (!payloads.Any())
            {
                throw new BusinessAppAppException(
                    "The macro you ran expected to find records to change, but none were " +
                    "found"
                );
            }

            return await handler.HandleAsync(payloads, cancellationToken);
        }
    }
}
