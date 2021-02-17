namespace BusinessApp.WebApi.Json
{
    using Microsoft.AspNetCore.Http;
    using System;
    using System.Threading.Tasks;
    using BusinessApp.Domain;
    using System.Threading;

    /// <summary>
    /// Writes out the final response after handling the request
    /// </summary>
    public class JsonHttpDecorator<TRequest, TResponse> : IHttpRequestHandler<TRequest, TResponse>
    {
        private readonly IHttpRequestHandler<TRequest, TResponse> inner;
        private readonly IResponseWriter modelWriter;

        public JsonHttpDecorator(IHttpRequestHandler<TRequest, TResponse> inner,
            IResponseWriter modelWriter)
        {
            this.inner = inner.NotNull().Expect(nameof(inner));
            this.modelWriter = modelWriter.NotNull().Expect(nameof(modelWriter));
        }

        public async Task<Result<TResponse, IFormattable>> HandleAsync(HttpContext context,
            CancellationToken cancelToken)
        {
            var validContentType = !string.IsNullOrWhiteSpace(context.Request.ContentType)
                && context.Request.ContentType.Contains("application/json");

            if (context.Request.MayHaveContent() && !validContentType)
            {
                context.Response.StatusCode = StatusCodes.Status415UnsupportedMediaType;
                return Result<TResponse, IFormattable>.Error($"Expected content-type to be application/json");
            }

            try
            {
                var result = await inner.HandleAsync(context, cancelToken);

                context.Response.ContentType = result.Kind switch
                {
                    ValueKind.Error => "application/problem+json",
                    _ => "application/json",
                };

                await modelWriter.WriteResponseAsync(context, result);

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
