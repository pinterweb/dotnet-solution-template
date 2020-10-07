namespace BusinessApp.App
{
    using System.Threading;
    using System.Threading.Tasks;
    using BusinessApp.Domain;
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public class SingleQueryHandlerDelegator<TRequest, TResponse> : IRequestHandler<TRequest, TResponse>
    {
        private readonly IRequestHandler<TRequest, IEnumerable<TResponse>> handler;

        public SingleQueryHandlerDelegator(
            IRequestHandler<TRequest, IEnumerable<TResponse>> handler)
        {
            this.handler = Guard.Against.Null(handler).Expect(nameof(handler));
        }

        public async Task<Result<TResponse, IFormattable>> HandleAsync(TRequest request,
            CancellationToken cancellationToken)
        {
            var response = await handler.HandleAsync(request, cancellationToken);

            return Result<TResponse, IFormattable>.Ok(response.Unwrap().SingleOrDefault());
        }
    }
}
