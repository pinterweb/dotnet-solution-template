using Microsoft.AspNetCore.Http;
using System;
using System.Threading.Tasks;
using BusinessApp.Kernel;
using System.Threading;

namespace BusinessApp.Infrastructure.WebApi.Json
{
    /// <summary>
    /// Runs logic on the request/response for json requests
    /// </summary>
    public class JsonHttpDecorator<TRequest, TResponse> : IHttpRequestHandler<TRequest, TResponse>
       where TRequest : notnull
    {
        private readonly IHttpRequestHandler<TRequest, TResponse> inner;

        public JsonHttpDecorator(IHttpRequestHandler<TRequest, TResponse> inner)
            => this.inner = inner.NotNull().Expect(nameof(inner));

        public async Task<Result<HandlerContext<TRequest, TResponse>, Exception>> HandleAsync(
            HttpContext context, CancellationToken cancelToken)
        {
            var validContentType = !string.IsNullOrWhiteSpace(context.Request.ContentType)
                && context.Request.ContentType.Contains("application/json");

            if (context.Request.MayHaveContent() && !validContentType)
            {
                context.Response.StatusCode = StatusCodes.Status415UnsupportedMediaType;

                return Result.Error<HandlerContext<TRequest, TResponse>>(
                    new BusinessAppException("Expected content-type to be application/json"));
            }

            try
            {
                var result = await inner.HandleAsync(context, cancelToken);

                context.Response.ContentType = result.Kind switch
                {
                    ValueKind.Error => "application/problem+json",
                    _ => "application/json",
                };

                return result;
            }
            catch
            {
                context.Response.ContentType = "application/problem+json";
                throw;
            };
        }
    }
}
