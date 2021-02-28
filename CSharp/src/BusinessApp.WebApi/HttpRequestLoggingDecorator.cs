namespace BusinessApp.WebApi
{
    using System;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Http;
    using BusinessApp.Domain;
    using System.Threading;

    /// <summary>
    /// Logs certains aspects of a request
    /// </summary>
    public class HttpRequestLoggingDecorator<TRequest, TResponse>
        : IHttpRequestHandler<TRequest, TResponse>
    {
        private readonly IHttpRequestHandler<TRequest, TResponse> inner;
        private readonly ILogger logger;

        public HttpRequestLoggingDecorator(
            IHttpRequestHandler<TRequest, TResponse> inner,
            ILogger logger)
        {
            this.inner = inner.NotNull().Expect(nameof(inner));
            this.logger = logger.NotNull().Expect(nameof(logger));
        }

        public async Task<Result<TResponse, Exception>> HandleAsync(HttpContext context,
            CancellationToken cancelToken)
        {
            try
            {
                return await inner.HandleAsync(context, cancelToken);
            }
            catch (Exception exception)
            {
                logger.Log(new LogEntry(LogSeverity.Error, exception.Message, exception));
                throw;
            }
        }
    }
}
