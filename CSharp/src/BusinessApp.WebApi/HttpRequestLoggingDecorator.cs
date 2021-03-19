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

        public HttpRequestLoggingDecorator(IHttpRequestHandler<TRequest, TResponse> inner,
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
            catch (ArgumentException exception) when (exception.InnerException is FormatException)
            {
                Log(exception);

                return Result.Error<TResponse>(
                    new BadStateException("Your request could not be read because some " +
                        "arguments may be in the wrong format. Please review your requets " +
                        "and try again"));
            }
            catch (Exception exception)
            {
                Log(exception);

                return Result.Error<TResponse>(new BusinessAppWebApiException(
                    "An unknown error occurred while processing your request. Please try " +
                    "again or contact support if this continues"));
            }
        }

        private void Log(Exception exception)
        {
            logger.Log(new LogEntry(LogSeverity.Error, exception.Message, exception));
        }
    }
}
