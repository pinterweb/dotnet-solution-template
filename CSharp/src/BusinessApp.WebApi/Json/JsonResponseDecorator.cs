namespace BusinessApp.WebApi.Json
{
    using Microsoft.AspNetCore.Http;
    using System;
    using System.Threading.Tasks;
    using BusinessApp.Domain;
    using System.Threading;
    using BusinessApp.App;

    /// <summary>
    /// Writes out the final response after handling the request
    /// </summary>
    public class JsonResponseDecorator<TRequest, TResponse> : IResourceHandler<TRequest, TResponse>
    {
        private readonly IResourceHandler<TRequest, TResponse> decorated;
        private readonly IResponseWriter modelWriter;

        public JsonResponseDecorator(IResourceHandler<TRequest, TResponse> decorated,
            IResponseWriter modelWriter)
        {
            this.decorated = Guard.Against.Null(decorated).Expect(nameof(decorated));
            this.modelWriter = Guard.Against.Null(modelWriter).Expect(nameof(modelWriter));
        }

        public async Task<TResponse> HandleAsync(HttpContext context, CancellationToken cancellationToken)
        {
            try
            {
                var validContentType =
                    !string.IsNullOrWhiteSpace(context.Request.ContentType) &&
                    context.Request.ContentType.Contains("application/json");

                var requestHasBody = await context.Request.HasBody();

                if (requestHasBody && !validContentType)
                {
                    context.Response.StatusCode = StatusCodes.Status415UnsupportedMediaType;
                    throw new NotSupportedException("Expected content-type to be application/json");
                }

                var resource = await decorated.HandleAsync(context, cancellationToken);

                await modelWriter.WriteResponseAsync(context, Result<TResponse, _>.Ok(resource));

                return resource;
            }
            catch (Exception e)
            {
                await modelWriter.WriteResponseAsync(
                    context,
                    Result<TResponse, IFormattable>
                    .Error(e is IFormattable f ? f : new UnhandledRequestException(e))
                );

                throw;
            }
        }
    }
}
