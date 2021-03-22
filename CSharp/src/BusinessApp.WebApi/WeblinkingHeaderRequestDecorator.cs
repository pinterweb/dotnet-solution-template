namespace BusinessApp.WebApi
{
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Http;
    using BusinessApp.Domain;
    using System;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// Decorator to add the web link headers for the particular response
    /// </summary>
    public class WeblinkingHeaderRequestDecorator<TRequest, TResponse> : IHttpRequestHandler<TRequest, TResponse>
    {
        private readonly IHttpRequestHandler<TRequest, TResponse> handler;
        private readonly IEnumerable<HateoasLink<TResponse>> links;

        public WeblinkingHeaderRequestDecorator(IHttpRequestHandler<TRequest, TResponse> handler,
            IEnumerable<HateoasLink<TResponse>> links)
        {
            this.handler = handler.NotNull().Expect(nameof(handler));
            this.links = links.NotNull().Expect(nameof(links));
        }

        public virtual async Task<Result<TResponse, Exception>> HandleAsync(HttpContext context,
            CancellationToken cancelToken)
        {
            return (await handler.HandleAsync(context, cancelToken))
                .Map(data =>
                {
                    var headerLinks = links.Select(l => l.ToHeaderValue(context.Request, data));

                    if (headerLinks.Any())
                    {
                        context.Response.Headers.Add("Link", headerLinks.ToArray());
                    }

                    return data;
                });
        }
    }
}
