using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using BusinessApp.Kernel;
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Primitives;

namespace BusinessApp.WebApi
{
    /// <summary>
    /// Decorator to add the web link headers for generate events
    /// </summary>
    public class WeblinkingHeaderEventRequestDecorator<TRequest, TResponse> : IHttpRequestHandler<TRequest, TResponse>
       where TRequest : notnull
       where TResponse : ICompositeEvent
    {
        private readonly IHttpRequestHandler<TRequest, TResponse> handler;
        private readonly IDictionary<Type, HateoasLink<TRequest, IEvent>> lookup;

        public WeblinkingHeaderEventRequestDecorator(IHttpRequestHandler<TRequest, TResponse> handler,
            IDictionary<Type, HateoasLink<TRequest, IEvent>> links)
        {
            this.handler = handler.NotNull().Expect(nameof(handler));
            lookup = links.NotNull().Expect(nameof(links));
        }

        public Task<Result<HandlerContext<TRequest, TResponse>, Exception>> HandleAsync(
            HttpContext context, CancellationToken cancelToken)
            => handler.HandleAsync(context, cancelToken)
                    .MapAsync(okVal =>
                    {
                        var headerLinks = okVal.Response.Events
                            .Select(e => (e, e.GetType()))
                            .Where(HasLink)
                            .Select(l => lookup[l.Item2]
                                .ToHeaderValue(context.Request, okVal.Request, l.e))
                            .ToArray();

                        if (!headerLinks.Any()) return okVal;

                        if (context.Response.Headers.TryGetValue("Link", out var sv))
                        {
                            context.Response.Headers["Link"] =
                                StringValues.Concat(sv, new StringValues(headerLinks));
                        }
                        else
                        {
                            context.Response.Headers.Add("Link", headerLinks);
                        }

                        return okVal;
                    });

        private bool HasLink((IEvent e, Type eventType) eventLookup)
            => lookup.TryGetValue(eventLookup.eventType, out var _);
    }
}
