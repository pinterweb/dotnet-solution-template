namespace BusinessApp.App
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Collections.Generic;
    using BusinessApp.Domain;

    public class EventMetadataPublisherFactory : IEventPublisherFactory
    {
        private readonly IEventPublisher inner;
        private readonly IEventStore store;
        private readonly IEntityIdFactory<EventId> idFactory;

        public EventMetadataPublisherFactory(IEventPublisher inner,
            IEventStore store,
            IEntityIdFactory<EventId> idFactory)
        {
            this.inner = inner.NotNull().Expect(nameof(inner));
            this.store = store.NotNull().Expect(nameof(store));
            this.idFactory = idFactory.NotNull().Expect(nameof(idFactory));
        }

        public IEventPublisher Create<T>(T trigger)
        {
            var metadata = new EventMetadataStream<T>(idFactory.Create(), trigger);

            store.Add(metadata);

            return new EventMetadataPublisher<T>(inner, metadata, idFactory);
        }

        private class EventMetadataPublisher<T> : IEventPublisher
        {
            private readonly EventMetadataStream<T> triggerMetadata;
            private readonly IEventPublisher inner;
            private readonly IEntityIdFactory<EventId> idFactory;

            public EventMetadataPublisher(IEventPublisher inner,
                EventMetadataStream<T> triggerMetadata,
                IEntityIdFactory<EventId> idFactory)
            {
                this.triggerMetadata = triggerMetadata.NotNull().Expect(nameof(triggerMetadata));
                this.inner = inner.NotNull().Expect(nameof(inner));
                this.idFactory = idFactory.NotNull().Expect(nameof(idFactory));
            }

            public async Task<Result<IEnumerable<IDomainEvent>, Exception>> PublishAsync(
                IDomainEvent @event, CancellationToken cancelToken)
            {
                if (!triggerMetadata.EventsSeen.TryGetValue(@event, out EventMetadata metadata))
                {
                    metadata = CreateInitialMetadata(@event);
                }

                var outcomes = await inner.PublishAsync(@event, cancelToken);

                return await inner.PublishAsync(@event, cancelToken)
                    .AndThenAsync(events => AddMetadata(metadata, events));
            }

            private EventMetadata CreateInitialMetadata(IDomainEvent @event)
            {
                return new EventMetadata(
                    new EventTrackingId(idFactory.Create(), triggerMetadata.Id, triggerMetadata.Id),
                    @event
                );
            }

            private Result<IEnumerable<IDomainEvent>, Exception> AddMetadata(EventMetadata published,
                IEnumerable<IDomainEvent> nextEvents)
            {
                var _ = triggerMetadata.EventsSeen.TryAdd(published.Event, published);

                foreach (var n in nextEvents)
                {
                    var metadata = new EventMetadata(
                        new EventTrackingId(idFactory.Create(), published.Id.Id, triggerMetadata.Id),
                        n
                    );
                    triggerMetadata.EventsSeen.TryAdd(n, metadata);
                }

                return Result.Ok(nextEvents);
            }
        }
    }
}
