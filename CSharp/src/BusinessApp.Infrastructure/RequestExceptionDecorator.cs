using System;
using System.Threading;
using System.Threading.Tasks;
using BusinessApp.Kernel;

namespace BusinessApp.Infrastructure
{
    /// <summary>
    /// Handles uncaught errors during handling so it can transform the response into
    /// a <see cref="Result"/> type
    /// </summary>
    public class RequestExceptionDecorator<TRequest, TResponse> : IRequestHandler<TRequest, TResponse>
        where TRequest : notnull
    {
        private readonly IRequestHandler<TRequest, TResponse> inner;
        private readonly ILogger logger;

        public RequestExceptionDecorator(IRequestHandler<TRequest, TResponse> inner,
            ILogger logger)
        {
            this.inner = inner.NotNull().Expect(nameof(inner));
            this.logger = logger.NotNull().Expect(nameof(logger));
        }

        public async Task<Result<TResponse, Exception>> HandleAsync(
            TRequest request, CancellationToken cancelToken)
        {
            try
            {
                return await inner.HandleAsync(request, cancelToken)
                    .OrElseAsync(e => Log(e, request, LogSeverity.Error));
            }
            catch (Exception e)
            {
                return Log(e, request, LogSeverity.Critical);
            }
        }

        private Result<TResponse, Exception> Log(Exception e, TRequest request, LogSeverity severity)
        {
            logger.Log(new LogEntry(severity, e.Message)
            {
                Exception = e,
                Data = request
            });

            return Result.Error<TResponse>(e);
        }
    }
}
