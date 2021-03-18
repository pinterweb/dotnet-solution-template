namespace BusinessApp.App
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Collections.Generic;
    using BusinessApp.Domain;
    using System.Collections.Concurrent;

    public class EventMetadataPublisherFactory : IEventPublisherFactory
    {
        private readonly IEventPublisher inner;
        private readonly IEventStoreFactory storeFactory;

        public EventMetadataPublisherFactory(IEventPublisher inner,
            IEventStoreFactory storeFactory)
        {
            this.inner = inner.NotNull().Expect(nameof(inner));
            this.storeFactory = storeFactory.NotNull().Expect(nameof(storeFactory));
        }

        public IEventPublisher Create<T>(T trigger) where T : class
        {
            var store = storeFactory.Create(trigger);

            return new EventMetadataPublisher(inner, store);
        }

        private class EventMetadataPublisher : IEventPublisher
        {
            private readonly ConcurrentDictionary<IDomainEvent, EventTrackingId> outcomeTracking;
            private readonly IEventPublisher inner;
            private readonly IEventStore store;

            public EventMetadataPublisher(IEventPublisher inner, IEventStore store)
            {
                this.inner = inner.NotNull().Expect(nameof(inner));
                this.store = store.NotNull().Expect(nameof(store));
                outcomeTracking = new ConcurrentDictionary<IDomainEvent, EventTrackingId>();
            }

            public async Task<Result<IEnumerable<IDomainEvent>, Exception>> PublishAsync<T>(
                T @event, CancellationToken cancelToken)
                where T : IDomainEvent
            {
                return await inner.PublishAsync(@event, cancelToken)
                    .AndThenAsync(o => TrackOutcomes(@event, o));
            }

            /// <summary>
            /// Add the current event to the store and track unpublished events
            /// so we can associate causation id
            /// </summary>
            private Result<IEnumerable<IDomainEvent>, Exception> TrackOutcomes<T>(
                T published, IEnumerable<IDomainEvent> outcomes)
                where T : IDomainEvent
            {
                var publishedTrackingId = store.Add(published);

                if (outcomeTracking.TryGetValue(published, out EventTrackingId causedById))
                {
                    publishedTrackingId.CausationId = causedById.Id;
                }

                foreach (var o in outcomes)
                {
                    var _ = outcomeTracking.TryAdd(o, publishedTrackingId);
                }

                return Result.Ok(outcomes);
            }
        }
    }
}
