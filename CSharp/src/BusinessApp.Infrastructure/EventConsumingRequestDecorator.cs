using System;
using System.Collections.Generic;
using BusinessApp.Kernel;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace BusinessApp.Infrastructure
{
#pragma warning disable IDE0065
    using EventResult = Result<IEnumerable<IDomainEvent>, Exception>;
#pragma warning restore IDE0065

    /// <summary>
    /// Consume event data from the prior handlers
    /// </summary>
    public class EventConsumingRequestDecorator<TRequest, TResponse> : IRequestHandler<TRequest, TResponse>
        where TRequest : class
        where TResponse : ICompositeEvent
    {
        private static readonly MethodInfo publishMethod = typeof(IEventPublisher)
            .GetMethod(nameof(IEventPublisher.PublishAsync))!;

        private readonly IEventPublisherFactory publisherFactory;
        private readonly IRequestHandler<TRequest, TResponse> inner;

        public EventConsumingRequestDecorator(IRequestHandler<TRequest, TResponse> inner,
            IEventPublisherFactory publisherFactory)
        {
            this.inner = inner.NotNull().Expect(nameof(inner));
            this.publisherFactory = publisherFactory.NotNull().Expect(nameof(inner));
        }

        public async Task<Result<TResponse, Exception>> HandleAsync(TRequest request,
            CancellationToken cancelToken)
        {
            var publisher = publisherFactory.Create(request);

            var handlerResult = await inner.HandleAsync(request, cancelToken);

            return await handlerResult
                .Map(s => s.Events)
                .AndThenAsync(events => ConsumeAsync(publisher, events, cancelToken))
                .MapOrElseAsync(
                    err => Result.Error<TResponse>(err),
                    ok =>
                    {
                        handlerResult.Unwrap().Events = ok;
                        return handlerResult;
                    }
                );
        }

        private async Task<EventResult> ConsumeAsync(
            IEventPublisher publisher, IEnumerable<IDomainEvent> events, CancellationToken cancelToken)
        {
            var consumedEvents = events.Select(s => s).ToList();

            return !consumedEvents.Any()
                ? Result.Ok<IEnumerable<IDomainEvent>>(consumedEvents)
                : await Task.WhenAll(consumedEvents.Select(e => PublishAsync(publisher, e, cancelToken)))
                    .CollectAsync()
                    .MapAsync(v => v.SelectMany(s => s))
                    .AndThenAsync(events => ConsumeAsync(publisher, events, cancelToken))
                    .MapAsync(e => consumedEvents.Concat(e));
        }

        public Task<EventResult> PublishAsync(IEventPublisher publisher, IDomainEvent @event,
            CancellationToken cancelToken)
        {
            var generic = publishMethod.MakeGenericMethod(@event.GetType());

            return (Task<EventResult>)generic.Invoke(publisher,
                new object[] { @event, cancelToken })!;
        }
    }
}
