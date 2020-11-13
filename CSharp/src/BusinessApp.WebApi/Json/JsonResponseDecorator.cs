using Microsoft.AspNetCore.Http;
using System;
using System.Threading.Tasks;
using BusinessApp.Domain;
using System.Threading;

namespace BusinessApp.WebApi.Json
{
    /// <summary>
    /// Writes out the final response after handling the request
    /// </summary>
    public class JsonResponseDecorator<TRequest, TResponse> : IHttpRequestHandler<TRequest, TResponse>
    {
        private readonly IHttpRequestHandler<TRequest, TResponse> inner;
        private readonly IResponseWriter modelWriter;

        public JsonResponseDecorator(IHttpRequestHandler<TRequest, TResponse> inner,
            IResponseWriter modelWriter)
        {
            this.inner = Guard.Against.Null(inner).Expect(nameof(inner));
            this.modelWriter = Guard.Against.Null(modelWriter).Expect(nameof(modelWriter));
        }

        public async Task<Result<TResponse, IFormattable>> HandleAsync(HttpContext context, CancellationToken cancellationToken)
        {
            var validContentType = !string.IsNullOrWhiteSpace(context.Request.ContentType)
                && context.Request.ContentType.Contains("application/json");

            if (context.Request.MayHaveContent() && !validContentType)
            {
                context.Response.StatusCode = StatusCodes.Status415UnsupportedMediaType;
                return Result<TResponse, IFormattable>.Error($"Expected content-type to be application/json");
            }

            var result = await inner.HandleAsync(context, cancellationToken);

            context.Response.ContentType = result.Kind switch
            {
                ValueKind.Error => "application/problem+json",
                _ => "application/json",
            };

            await modelWriter.WriteResponseAsync(context, result);

            return result;
        }
    }
}
