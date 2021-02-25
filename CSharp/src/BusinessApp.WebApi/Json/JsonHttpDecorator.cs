namespace BusinessApp.WebApi.Json
{
    using Microsoft.AspNetCore.Http;
    using System;
    using System.Threading.Tasks;
    using BusinessApp.Domain;
    using System.Threading;

    /// <summary>
    /// Runs logic on the request/response for json requests
    /// </summary>
    public class JsonHttpDecorator<TRequest, TResponse> : IHttpRequestHandler<TRequest, TResponse>
    {
        private readonly IHttpRequestHandler<TRequest, TResponse> inner;

        public JsonHttpDecorator(IHttpRequestHandler<TRequest, TResponse> inner)
        {
            this.inner = inner.NotNull().Expect(nameof(inner));
        }

        public async Task<Result<TResponse, IFormattable>> HandleAsync(HttpContext context,
            CancellationToken cancelToken)
        {
            var validContentType = !string.IsNullOrWhiteSpace(context.Request.ContentType)
                && context.Request.ContentType.Contains("application/json");

            if (context.Request.MayHaveContent() && !validContentType)
            {
                context.Response.StatusCode = StatusCodes.Status415UnsupportedMediaType;

                return Result<TResponse, IFormattable>.Error(
                    $"Expected content-type to be application/json");
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
