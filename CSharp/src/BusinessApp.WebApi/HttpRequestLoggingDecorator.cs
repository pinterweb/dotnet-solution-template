using System;
using System.Threading.Tasks;
using BusinessApp.Infrastructure;
using BusinessApp.Kernel;
using Microsoft.AspNetCore.Http;

namespace BusinessApp.WebApi
{
    /// <summary>
    /// Deocrator that logs request errors
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

        public async Task HandleAsync<TRequest, TResponse>(HttpContext context) where TRequest : notnull
        {
            try
            {
                await inner.HandleAsync<TRequest, TResponse>(context);
            }
            catch (Exception exception)
            {
                logger.Log(LogEntry.FromException(exception));

                throw;
            }
        }
    }
}
