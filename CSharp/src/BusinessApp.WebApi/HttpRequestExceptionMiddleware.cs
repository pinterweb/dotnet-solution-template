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

        public HttpRequestExceptionMiddleware(ILogger logger)
        {
            this.logger = Guard.Against.Null(logger).Expect(nameof(logger));
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
            }
        }
    }
}
