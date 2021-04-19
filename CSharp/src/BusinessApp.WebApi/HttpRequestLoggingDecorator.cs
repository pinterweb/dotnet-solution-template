using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using BusinessApp.Domain;

namespace BusinessApp.WebApi
{
    /// <summary>
    /// Logs request errors
    /// </summary>
    public class HttpRequestLoggingDecorator : IHttpRequestHandler
    {
        private readonly IHttpRequestHandler inner;
        private readonly ILogger logger;

        public HttpRequestLoggingDecorator(IHttpRequestHandler inner, ILogger logger)
        {
            this.inner = inner.NotNull().Expect(nameof(inner));
            this.logger = logger.NotNull().Expect(nameof(logger));
        }

        public async Task HandleAsync<T, R>(HttpContext context) where T : notnull
        {
            try
            {
                await inner.HandleAsync<T, R>(context);
            }
            catch (Exception exception)
            {
                logger.Log(LogEntry.FromException(exception));

                throw;
            }
        }
    }
}
