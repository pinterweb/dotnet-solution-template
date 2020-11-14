namespace BusinessApp.WebApi
{
    using System;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Http;
    using BusinessApp.Domain;

    /// <summary>
    /// Catches all errors during the execution of a request
    /// </summary>
    public sealed class HttpRequestExceptionMiddleware : IMiddleware
    {
        private readonly ILogger logger;
        private readonly IResponseWriter writer;

        public HttpRequestExceptionMiddleware(ILogger logger, IResponseWriter writer)
        {
            this.logger = Guard.Against.Null(logger).Expect(nameof(logger));
            this.writer = Guard.Against.Null(writer).Expect(nameof(writer));
        }

        public async Task InvokeAsync(HttpContext context, RequestDelegate next)
        {
            try
            {
                await next(context);
            }
            catch (Exception exception)
            {
                logger.Log(new LogEntry(LogSeverity.Error, exception.Message, exception));

                if (!context.Response.HasStarted)
                {
                    context.Response.StatusCode = StatusCodes.Status500InternalServerError;

                    if (exception is IFormattable f)
                    {
                        await writer.WriteResponseAsync(context, Result.Error(f).Into());
                    }
                    else
                    {
                        await writer.WriteResponseAsync(context);
                    }
                }
            }
        }
    }
}
