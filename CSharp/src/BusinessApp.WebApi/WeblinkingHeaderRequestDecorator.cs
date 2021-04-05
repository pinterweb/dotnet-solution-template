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
    public class WeblinkingHeaderRequestDecorator<T, R> : IHttpRequestHandler<T, R>
       where T : notnull
    {
        private readonly IHttpRequestHandler<T, R> handler;
        private readonly IEnumerable<HateoasLink<T, R>> links;

        public WeblinkingHeaderRequestDecorator(IHttpRequestHandler<T, R> handler,
            IEnumerable<HateoasLink<T, R>> links)
        {
            this.handler = handler.NotNull().Expect(nameof(handler));
            this.links = links.NotNull().Expect(nameof(links));
        }

        public virtual async Task<Result<HandlerContext<T, R>, Exception>> HandleAsync(
            HttpContext context, CancellationToken cancelToken)
        {
            return (await handler.HandleAsync(context, cancelToken))
                .Map(okVal =>
                {
                    var headerLinks = links.Select(l =>
                        l.ToHeaderValue(context.Request, okVal.Request, okVal.Response));

                    if (headerLinks.Any())
                    {
                        context.Response.Headers.Add("Link", headerLinks.ToArray());
                    }

                    return okVal;
                });
        }
    }
}
