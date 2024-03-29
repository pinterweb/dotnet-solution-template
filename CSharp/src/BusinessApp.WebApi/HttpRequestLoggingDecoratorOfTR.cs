using System;
using System.Threading;
using System.Threading.Tasks;
using BusinessApp.Infrastructure;
using BusinessApp.Kernel;
using Microsoft.AspNetCore.Http;

namespace BusinessApp.WebApi
{
    /// <summary>
    /// Decorator that logs request errors and converts it to a <see cref="Result"/> type
    /// </summary>
    public class HttpRequestLoggingDecorator<TRequest, TResponse> : IHttpRequestHandler<TRequest, TResponse>
        where TRequest : notnull
    {
        private readonly IHttpRequestHandler<TRequest, TResponse> inner;
        private readonly ILogger logger;

        public HttpRequestLoggingDecorator(IHttpRequestHandler<TRequest, TResponse> inner,
            ILogger logger)
        {
            this.inner = inner.NotNull().Expect(nameof(inner));
            this.logger = logger.NotNull().Expect(nameof(logger));
        }

        public async Task<Result<HandlerContext<TRequest, TResponse>, Exception>> HandleAsync(
            HttpContext context, CancellationToken cancelToken)
        {
            try
            {
                return await inner.HandleAsync(context, cancelToken);
            }
            catch (ArgumentException exception) when (exception.InnerException is FormatException)
            {
                Log(exception);

                return Result.Error<HandlerContext<TRequest, TResponse>>(
                    new BadStateException("Your request could not be read because some " +
                        "arguments may be in the wrong format. Please review your request " +
                        "and try again"));
            }
            catch (Exception exception)
            {
                Log(exception);

                return Result.Error<HandlerContext<TRequest, TResponse>>(exception);
            }
        }

        private void Log(Exception exception) => logger.Log(LogEntry.FromException(exception));
    }
}
