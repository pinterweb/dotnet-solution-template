namespace BusinessApp.App
{
    using System.Threading;
    using System.Threading.Tasks;
    using BusinessApp.Domain;
    using System;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// Wraps a request for a single response object in an IEnumerable request handler
    /// when batch requests are supported
    /// </summary>
    /// <remarks>
    /// This help reduce the number of handlers/decorators needed when all we care about
    /// is returning one resource
    /// </remarks>
    public class SingleQueryDelegator<TRequest, TResponse> : IRequestHandler<TRequest, TResponse>
    {
        private readonly IRequestHandler<TRequest, IEnumerable<TResponse>> handler;

        public SingleQueryDelegator(
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
