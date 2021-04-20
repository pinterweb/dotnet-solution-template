using Microsoft.AspNetCore.Http;
using System;
using System.Threading.Tasks;
using BusinessApp.Domain;
using BusinessApp.WebApi.ProblemDetails;
using BusinessApp.Infrastructure;
using System.Threading;

namespace BusinessApp.WebApi
{
    public class HttpResponseDecorator<T, R> : IHttpRequestHandler<T, R>
        where T : notnull
    {
        private readonly IHttpRequestHandler<T, R> inner;
        private readonly IProblemDetailFactory problemFactory;
        private readonly ISerializer serializer;

        public HttpResponseDecorator(IHttpRequestHandler<T, R> inner,
            IProblemDetailFactory problemFactory,
            ISerializer serializer)
        {
            this.problemFactory = problemFactory.NotNull().Expect(nameof(problemFactory));
            this.serializer = serializer.NotNull().Expect(nameof(serializer));
            this.inner = inner.NotNull().Expect(nameof(inner));
        }

        public async Task<Result<HandlerContext<T, R>, Exception>> HandleAsync(HttpContext context,
            CancellationToken cancelToken)
        {
            var result = await inner.HandleAsync(context, cancelToken);

            var canWrite = StartResponse(context);

            if (result.Kind == ValueKind.Error)
            {
                var problem = problemFactory.Create(result.UnwrapError());

                context.Response.StatusCode = problem.StatusCode;

                await WriteResponse(context, cancelToken, problem);
            }
            else if (canWrite)
            {
                var model = result.Unwrap().Response;

                await WriteResponse(context, cancelToken, model);
            }

            return result;
        }

        /// <summary>
        /// Starts the response by checking the current status and setting a status
        /// based on the method if applicable
        /// </summary>
        /// </returns>true if can write</returns>
        private static bool StartResponse(HttpContext context)
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
                return false;
            }

            return true;
        }

        private async Task WriteResponse<M>(HttpContext context, CancellationToken cancelToken,
            M model)
        {
            await context.Response.BodyWriter.WriteAsync(
                serializer.Serialize(model),
                cancelToken);
        }
    }
}
