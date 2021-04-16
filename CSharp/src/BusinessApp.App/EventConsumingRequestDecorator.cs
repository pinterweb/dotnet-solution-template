using System;
using System.Collections.Generic;
using BusinessApp.Domain;

namespace BusinessApp.App
{
    using System.Linq;
    using System.Reflection;
    using System.Threading;
    using System.Threading.Tasks;

    using EventResult = Result<IEnumerable<IDomainEvent>, Exception>;

    /// <summary>
    /// Consume event data from the prior handlers
    /// </summary>
    public class EventConsumingRequestDecorator<TRequest, TResponse> : IRequestHandler<TRequest, TResponse>
        where TRequest : class
        where TResponse : IEventStream
    {
        private static MethodInfo PublishMethod = typeof(IEventPublisher).GetMethod(nameof(IEventPublisher.PublishAsync))!;
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

            return await inner.HandleAsync(request, cancelToken);
            var streamResult = await inner.HandleAsync(request, cancelToken);

            return await streamResult
                .Map(s => s.Events)
                .AndThenAsync(events => ConsumeAsync(publisher, events, cancelToken))
                .MapOrElseAsync(
                    err => Result.Error<TResponse>(err),
                    ok =>
                    {
                        streamResult.Unwrap().Events = ok;
                        return streamResult;
                    }
                );
        }

        private async Task<EventResult> ConsumeAsync(
            IEventPublisher publisher, IEnumerable<IDomainEvent> events, CancellationToken cancelToken)
        {
            var consumedEvents = events.Select(s => s).ToList();

            if (!consumedEvents.Any()) return Result.Ok<IEnumerable<IDomainEvent>>(consumedEvents);

            return await Task.WhenAll(consumedEvents.Select(e => PublishAsync(publisher, e, cancelToken)))
                .CollectAsync()
                .MapAsync(v => v.SelectMany(s => s))
                .AndThenAsync(events => ConsumeAsync(publisher, events, cancelToken))
                .MapAsync(e => consumedEvents.Concat(e));
        }

        public Task<EventResult> PublishAsync(IEventPublisher publisher, IDomainEvent @event,
            CancellationToken cancelToken)
        {
            var generic = PublishMethod.MakeGenericMethod(@event.GetType());

            return (Task<EventResult>)generic.Invoke(publisher,
                new object[] { @event, cancelToken })!;
        }
    }
}
