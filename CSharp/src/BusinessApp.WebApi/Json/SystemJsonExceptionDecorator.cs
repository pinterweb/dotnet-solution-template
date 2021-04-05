namespace BusinessApp.WebApi.Json
{
    using System;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Http;
    using BusinessApp.Domain;
    using System.Threading;
    using System.Text.Json;

    /// <summary>
    /// Logs JSON exception and convert the response to a <see cref="Result"/> type
    /// </summary>
    public class SystemJsonExceptionDecorator<T, R> : IHttpRequestHandler<T, R>
       where T : notnull
    {
        private readonly IHttpRequestHandler<T, R> inner;
        private readonly ILogger logger;

        public SystemJsonExceptionDecorator(IHttpRequestHandler<T, R> inner, ILogger logger)
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
            catch (JsonException exception)
            {
                Log(exception);

                return Result.Error<HandlerContext<T, R>>(
                    new BadStateException("Your request could not be read because " +
                        "your payload is in an invalid format. Please review your data " +
                        "and try again"));
            }
        }

        private void Log(Exception exception)
        {
            logger.Log(LogEntry.FromException(exception));
        }
    }
}
