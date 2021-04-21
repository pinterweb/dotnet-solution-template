using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using BusinessApp.Kernel;
using System.Threading;
using BusinessApp.Infrastructure;

namespace BusinessApp.WebApi
{
    /// <summary>
    /// Logs request errors and converts it to a <see cref="Result"/> type
    /// </summary>
    public class HttpRequestLoggingDecorator<T, R> : IHttpRequestHandler<T, R>
        where T : notnull
    {
        private readonly IHttpRequestHandler<T, R> inner;
        private readonly ILogger logger;

        public HttpRequestLoggingDecorator(IHttpRequestHandler<T, R> inner,
            ILogger logger)
        {
            this.inner = inner.NotNull().Expect(nameof(inner));
            this.logger = logger.NotNull().Expect(nameof(logger));
        }

        public async Task<Result<HandlerContext<T, R>, Exception>> HandleAsync(
            HttpContext context, CancellationToken cancelToken)
        {
            try
            {
                return await inner.HandleAsync(context, cancelToken);
            }
            catch (ArgumentException exception) when (exception.InnerException is FormatException)
            {
                Log(exception);

                return Result.Error<HandlerContext<T, R>>(
                    new BadStateException("Your request could not be read because some " +
                        "arguments may be in the wrong format. Please review your request " +
                        "and try again"));
            }
            catch (Exception exception)
            {
                Log(exception);

                return Result.Error<HandlerContext<T, R>>(
                    new BusinessAppWebApiException("An unknown error occurred while processing " +
                        "your request. Please try again or contact support if this continues"));
            }
        }

        private void Log(Exception exception)
        {
            logger.Log(LogEntry.FromException(exception));
        }
    }
}
