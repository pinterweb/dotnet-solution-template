namespace BusinessApp.WebApi
{
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Http;
    using BusinessApp.Domain;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.Extensions.Primitives;

    /// <summary>
    /// Decorator to add the web link headers for the particular response
    /// </summary>
    public class WeblinkingEventRequestDecorator<TRequest, TResponse> : IHttpRequestHandler<TRequest, TResponse>
       where TResponse : IEventStream
    {
        private readonly IHttpRequestHandler<TRequest, TResponse> handler;
        private readonly IEnumerable<HateoasLink<TResponse>> links;
        private readonly IEventLinkFactory factory;

        public WeblinkingEventRequestDecorator(IHttpRequestHandler<TRequest, TResponse> handler,
            IEnumerable<HateoasLink<TResponse>> links,
            IEventLinkFactory factory)
        {
            this.handler = handler.NotNull().Expect(nameof(handler));
            this.links = links.NotNull().Expect(nameof(links));
        }

        public async Task<Result<TResponse, Exception>> HandleAsync(HttpContext context,
            CancellationToken cancelToken)
        {
            return (await handler.HandleAsync(context, cancelToken))
                .Map(data =>
                {
                    var headerLinks = data.Events
                        .Select(e => factory.Create(e).ToHeaderValue(context.Request, e))
                        .ToArray();

                    if (context.Response.Headers.TryGetValue("Link", out StringValues sv))
                    {
                        context.Response.Headers["Link"] = StringValues.Concat(sv, new StringValues(headerLinks));
                    }
                    else
                    {
                        context.Response.Headers.Add("Link", headerLinks);
                    }

                    return data;
                });
        }
    }

    public interface IEventLinkFactory
    {
        HateoasLink<IDomainEvent> Create(IDomainEvent e);
    }
}
