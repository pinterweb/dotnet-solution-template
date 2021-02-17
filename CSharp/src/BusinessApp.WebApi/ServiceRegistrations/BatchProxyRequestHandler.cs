namespace BusinessApp.WebApi
{
    using BusinessApp.App;
    using System.Threading.Tasks;
    using BusinessApp.Domain;
    using System;
    using System.Threading;

    /// <summary>
    /// Wraps the actual request handler implementation that is used
    /// by the <see cref="BatchRequestAdapter{TRequest, TResponse}" /> so the decorator
    /// pipeline is not injected.
    /// </summary>
    internal class BatchProxyRequestHandler<TInner, TRequest, TResponse> :
        IRequestHandler<TRequest, TResponse>
        where TInner : IRequestHandler<TRequest, TResponse>
    {
        private readonly TInner inner;

        public BatchProxyRequestHandler(TInner inner)
        {
            this.inner = inner;
        }

        public Task<Result<TResponse, IFormattable>> HandleAsync(TRequest command,
            CancellationToken cancelToken)
        {
            return inner.HandleAsync(command, cancelToken);
        }
    }
}
