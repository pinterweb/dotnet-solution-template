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
    public class SingleQueryRequestAdapter<TConsumer, TRequest, TResponse> :
        IRequestHandler<TRequest, TResponse>
        where TConsumer : IRequestHandler<TRequest, IEnumerable<TResponse>>
        where TRequest : IQuery
    {
        private readonly TConsumer handler;

        public SingleQueryRequestAdapter(TConsumer handler)
        {
            this.handler = handler.NotNull().Expect(nameof(handler));
        }

        public async Task<Result<TResponse, IFormattable>> HandleAsync(TRequest request,
            CancellationToken cancellationToken)
        {
            var response = await handler.HandleAsync(request, cancellationToken);

            // TODO return error if more than one
            return Result<TResponse, IFormattable>.Ok(response.Unwrap().SingleOrDefault());
        }
    }
}
