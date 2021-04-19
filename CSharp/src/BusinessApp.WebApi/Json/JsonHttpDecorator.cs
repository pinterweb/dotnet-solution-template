using Microsoft.AspNetCore.Http;
using System;
using System.Threading.Tasks;
using BusinessApp.Domain;
using System.Threading;

namespace BusinessApp.WebApi.Json
{
    /// <summary>
    /// Runs logic on the request/response for json requests
    /// </summary>
    public class JsonHttpDecorator<T, R> : IHttpRequestHandler<T, R>
       where T : notnull
    {
        private readonly IHttpRequestHandler<T, R> inner;

        public JsonHttpDecorator(IHttpRequestHandler<T, R> inner)
        {
            this.inner = inner.NotNull().Expect(nameof(inner));
        }

        public async Task<Result<HandlerContext<T, R>, Exception>> HandleAsync(
            HttpContext context, CancellationToken cancelToken)
        {
            var validContentType = !string.IsNullOrWhiteSpace(context.Request.ContentType)
                && context.Request.ContentType.Contains("application/json");

            if (context.Request.MayHaveContent() && !validContentType)
            {
                context.Response.StatusCode = StatusCodes.Status415UnsupportedMediaType;

                return Result.Error<HandlerContext<T, R>>(
                    new BusinessAppWebApiException("Expected content-type to be application/json"));
            }

            try
            {
                var result = await inner.HandleAsync(context, cancelToken);

                context.Response.ContentType = result.Kind switch {
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
