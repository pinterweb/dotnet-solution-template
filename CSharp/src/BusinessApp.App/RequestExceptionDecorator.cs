namespace BusinessApp.App
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using BusinessApp.Domain;

    /// <summary>
    /// Handles uncaught errors during handling so it can transform the response into
    /// a <see cref="Result"/> type
    /// </summary>
    public class RequestExceptionDecorator<TRequest, TResponse> : IRequestHandler<TRequest, TResponse>
    {
        private readonly IRequestHandler<TRequest, TResponse> inner;
        private readonly ILogger logger;

        public RequestExceptionDecorator(IRequestHandler<TRequest, TResponse> inner,
            ILogger logger)
        {
            this.inner = Guard.Against.Null(inner).Expect(nameof(inner));
            this.logger = Guard.Against.Null(logger).Expect(nameof(logger));
        }

        public async Task<Result<TResponse, IFormattable>> HandleAsync(
            TRequest request, CancellationToken cancellationToken)
        {
            try
            {
                return await inner.HandleAsync(request, cancellationToken);
            }
            catch(Exception e)
            {
                logger.Log(new LogEntry(LogSeverity.Critical, e.Message, e, request));

                return Result<TResponse, IFormattable>.Error(new UnhandledRequestException(e));
            }
        }
    }
}
