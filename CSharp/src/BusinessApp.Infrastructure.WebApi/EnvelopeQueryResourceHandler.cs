using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using BusinessApp.Kernel;
using System;

namespace BusinessApp.Infrastructure.WebApi
{
    /// <summary>
    /// Handles IEnumerable responses by adding metadata in the header
    /// </summary>
    public class EnvelopeQueryResourceHandler<TRequest, TResponse> : IHttpRequestHandler<TRequest, IEnumerable<TResponse>>
        where TRequest : notnull, IQuery
    {
        private readonly IRequestHandler<TRequest, EnvelopeContract<TResponse>> handler;
        private readonly ISerializer serializer;

        public EnvelopeQueryResourceHandler(
            IRequestHandler<TRequest, EnvelopeContract<TResponse>> handler,
            ISerializer serializer)
        {
            this.handler = handler.NotNull().Expect(nameof(handler));
            this.serializer = serializer.NotNull().Expect(nameof(serializer));
        }

        public async Task<Result<HandlerContext<TRequest, IEnumerable<TResponse>>, Exception>> HandleAsync(
            HttpContext context, CancellationToken cancelToken)
        {
            var query = await context.Request.DeserializeAsync<TRequest>(serializer, cancelToken);

            return query == null
                ? throw new BusinessAppException("Query cannot be null")
                : await handler.HandleAsync(query, cancelToken)
                .MapAsync(envelope =>
                {
#if !DEBUG
                    context.Response.Headers.Add("Access-Control-Expose-Headers", new[] { "VND.$(lower_appname).pagination" });
                    context.Response.Headers.Add("VND.$(lower_appname).pagination",
                        new StringValues(envelope.Pagination.ToHeaderValue()));
#else
                    context.Response.Headers.Add("Access-Control-Expose-Headers", new[] { "VND.foobar.pagination" });
                    context.Response.Headers.Add("VND.foobar.pagination",
                        new StringValues(envelope.Pagination.ToHeaderValue()));
#endif

                    return HandlerContext.Create(query, envelope.Data);
                });
        }
    }
}
