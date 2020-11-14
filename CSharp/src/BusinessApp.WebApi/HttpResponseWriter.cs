namespace BusinessApp.WebApi
{
    using Microsoft.AspNetCore.Http;
    using System;
    using System.Threading.Tasks;
    using BusinessApp.Domain;
    using BusinessApp.WebApi.ProblemDetails;
    using BusinessApp.App;

    public class HttpResponseWriter : IResponseWriter
    {
        private readonly IProblemDetailFactory problemFactory;
        private readonly ISerializer serializer;

        public HttpResponseWriter(IProblemDetailFactory problemFactory, ISerializer serializer)
        {
            this.problemFactory = Guard.Against.Null(problemFactory).Expect(nameof(problemFactory));
            this.serializer = Guard.Against.Null(serializer).Expect(nameof(serializer));
        }

        public Task WriteResponseAsync<T, E>(HttpContext context, Result<T, E> result)
            where E : IFormattable
        {
            StartResponse(context);

            object model = result.Kind switch
            {
                ValueKind.Error => problemFactory.Create(result.UnwrapError()),
                ValueKind.Ok => result.Unwrap(),
                _ => throw new NotImplementedException(),
            };

            serializer.Serialize(context.Response.Body, model);

            return Task.CompletedTask;
        }

        public Task WriteResponseAsync(HttpContext context)
        {
            StartResponse(context);

            if (!context.Response.IsSuccess())
            {
                var model = problemFactory.Create(null);
                serializer.Serialize(context.Response.Body, model);
            }

            return Task.CompletedTask;
        }

        private static void StartResponse(HttpContext context)
        {
            if (context.Response.HasStarted)
            {
                throw new BusinessAppWebApiException("The response has already started. You cannot " +
                    "write it more than once");
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
    }
}