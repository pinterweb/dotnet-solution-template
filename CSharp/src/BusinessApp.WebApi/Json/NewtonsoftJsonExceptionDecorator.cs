namespace BusinessApp.WebApi.Json
{
    using System;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Http;
    using BusinessApp.Domain;
    using System.Threading;
    using Newtonsoft.Json;

    /// <summary>
    /// Logs certains aspects of a request
    /// </summary>
    public class NewtonsoftJsonExceptionDecorator<T, R> : IHttpRequestHandler<T, R>
    {
        private readonly IHttpRequestHandler<T, R> inner;
        private readonly ILogger logger;

        public NewtonsoftJsonExceptionDecorator(IHttpRequestHandler<T, R> inner, ILogger logger)
        {
            this.inner = inner.NotNull().Expect(nameof(inner));
            this.logger = logger.NotNull().Expect(nameof(logger));
        }

        public async Task<Result<HandlerContext<T, R>, Exception>> HandleAsync(HttpContext context,
            CancellationToken cancelToken)
        {
            try
            {
                return await inner.HandleAsync(context, cancelToken);
            }
            catch (JsonSerializationException exception)
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
            logger.Log(new LogEntry(LogSeverity.Error, exception.Message, exception));
        }
    }
}
