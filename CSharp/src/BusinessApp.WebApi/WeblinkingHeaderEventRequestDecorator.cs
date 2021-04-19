using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using BusinessApp.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Primitives;

namespace BusinessApp.WebApi
{
    /// <summary>
    /// Decorator to add the web link headers for generate events
    /// </summary>
    public class WeblinkingHeaderEventRequestDecorator<T, R> : IHttpRequestHandler<T, R>
       where T : notnull
       where R : ICompositeEvent
    {
        private readonly IHttpRequestHandler<T, R> handler;
        private readonly IDictionary<Type, HateoasLink<T, IDomainEvent>> lookup;

        public WeblinkingHeaderEventRequestDecorator(IHttpRequestHandler<T, R> handler,
            IDictionary<Type, HateoasLink<T, IDomainEvent>> links)
        {
            this.handler = handler.NotNull().Expect(nameof(handler));
            this.lookup = links.NotNull().Expect(nameof(links));
        }

        public async Task<Result<HandlerContext<T, R>, Exception>> HandleAsync(
            HttpContext context, CancellationToken cancelToken)
        {
            return (await handler.HandleAsync(context, cancelToken))
                .Map(okVal =>
                {
                    var headerLinks = okVal.Response.Events
                        .Select(e => (e, e.GetType()))
                        .Where(HasLink)
                        .Select(l => lookup[l.Item2]
                            .ToHeaderValue(context.Request, okVal.Request, l.Item1))
                        .ToArray();

                    if (!headerLinks.Any()) return okVal;

                    if (context.Response.Headers.TryGetValue("Link", out StringValues sv))
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
        }

        private bool HasLink((IDomainEvent e, Type eventType) eventLookup)
            => lookup.TryGetValue(eventLookup.Item2, out HateoasLink<T, IDomainEvent> _);
    }
}
