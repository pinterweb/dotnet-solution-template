namespace BusinessApp.WebApi
{
    using Microsoft.AspNetCore.Http;
    using System;
    using System.Threading.Tasks;
    using BusinessApp.Domain;
    using BusinessApp.WebApi.ProblemDetails;
    using BusinessApp.App;
    using System.Threading;

    public class HttpResponseDecorator<TRequest, TResponse>
        : IHttpRequestHandler<TRequest, TResponse>
    {
        private readonly IHttpRequestHandler<TRequest, TResponse> inner;
        private readonly IProblemDetailFactory problemFactory;
        private readonly ISerializer serializer;

        public HttpResponseDecorator(IHttpRequestHandler<TRequest, TResponse> inner,
            IProblemDetailFactory problemFactory,
            ISerializer serializer)
        {
            this.problemFactory = problemFactory.NotNull().Expect(nameof(problemFactory));
            this.serializer = serializer.NotNull().Expect(nameof(serializer));
            this.inner = inner.NotNull().Expect(nameof(inner));
        }

        public async Task<Result<TResponse, IFormattable>> HandleAsync(HttpContext context,
            CancellationToken cancelToken)
        {
            var result = await inner.HandleAsync(context, cancelToken);

            StartResponse(context);

            if (result.Kind == ValueKind.Error)
            {
                var problem = problemFactory.Create(result.UnwrapError());

                await WriteResponse(context, cancelToken, problem);
            }
            else
            {
                var model = result.Unwrap();

                await WriteResponse(context, cancelToken, model);
            }

            return result;
        }

        private static void StartResponse(HttpContext context)
        {
            if (context.Response.HasStarted)
            {
                throw new BusinessAppWebApiException("The response has already started outside " +
                    "the expected write decorator. You cannot write it more than once.");
            }

            if (
                context.Response.StatusCode == 200 &&
                (string.Compare(context.Request.Method, "put", true) == 0 ||
                string.Compare(context.Request.Method, "delete", true) == 0)
            )
            {
                context.Response.StatusCode = StatusCodes.Status204NoContent;
            }
        }

        private async Task WriteResponse<T>(HttpContext context, CancellationToken cancelToken,
            T model)
        {
            await context.Response.BodyWriter.WriteAsync(
                serializer.Serialize(model),
                cancelToken);
        }
    }
}
