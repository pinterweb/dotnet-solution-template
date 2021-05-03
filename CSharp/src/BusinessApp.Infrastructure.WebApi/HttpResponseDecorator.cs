using Microsoft.AspNetCore.Http;
using System;
using System.Threading.Tasks;
using BusinessApp.Kernel;
using BusinessApp.Infrastructure.WebApi.ProblemDetails;
using System.Threading;

namespace BusinessApp.Infrastructure.WebApi
{
    /// <summary>
    /// Decorator that writes an http response
    /// </summary>
    public class HttpResponseDecorator<TRequest, TResponse> : IHttpRequestHandler<TRequest, TResponse>
        where TRequest : notnull
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

        public async Task<Result<HandlerContext<TRequest, TResponse>, Exception>> HandleAsync(HttpContext context,
            CancellationToken cancelToken)
        {
            var result = await inner.HandleAsync(context, cancelToken);

            var canWrite = StartResponse(context);

            if (result.Kind == ValueKind.Error)
            {
                var problem = problemFactory.Create(result.UnwrapError());

                context.Response.StatusCode = problem.StatusCode;

                await WriteResponse(context, problem, cancelToken);
            }
            else if (canWrite)
            {
                var model = result.Unwrap().Response;

                await WriteResponse(context, model, cancelToken);
            }

            return result;
        }

        /// <summary>
        /// Starts the response by checking the current status and setting a status
        /// based on the method if applicable
        /// </summary>
        /// <returns>true if can write</returns>
        private static bool StartResponse(HttpContext context)
        {
            if (context.Response.HasStarted)
            {
                throw new BusinessAppException("The response has already started outside " +
                    "the expected write decorator. You cannot write it more than once.");
            }

            if (
                context.Response.StatusCode == 200 &&
                (string.Compare(context.Request.Method, "put", StringComparison.OrdinalIgnoreCase) == 0 ||
                string.Compare(context.Request.Method, "delete", StringComparison.OrdinalIgnoreCase) == 0)
            )
            {
                context.Response.StatusCode = StatusCodes.Status204NoContent;
                return false;
            }

            return true;
        }

        private async Task WriteResponse<TModel>(HttpContext context, TModel model,
            CancellationToken cancelToken) => await context.Response.BodyWriter.WriteAsync(
                serializer.Serialize(model),
                cancelToken);
    }
}
