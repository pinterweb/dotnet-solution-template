using System;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using BusinessApp.Kernel;
using System.Collections.Concurrent;

namespace BusinessApp.Infrastructure
{
    /// <summary>
    /// Publishes event data while capture metadata for each event published
    /// </summary>
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
            private readonly ConcurrentDictionary<IEvent, EventTrackingId> outcomeTracking;
            private readonly IEventPublisher inner;
            private readonly IEventStore store;

            public EventMetadataPublisher(IEventPublisher inner, IEventStore store)
            {
                this.inner = inner.NotNull().Expect(nameof(inner));
                this.store = store.NotNull().Expect(nameof(store));
                outcomeTracking = new ConcurrentDictionary<IEvent, EventTrackingId>();
            }

            public Task<Result<IEnumerable<IEvent>, Exception>> PublishAsync<T>(
                T @event, CancellationToken cancelToken) where T : IEvent
                => inner.PublishAsync(@event, cancelToken)
                    .AndThenAsync(o => TrackOutcomes(@event, o));

            /// <summary>
            /// Add the current event to the store and track unpublished events
            /// so we can associate causation id
            /// </summary>
            private Result<IEnumerable<IEvent>, Exception> TrackOutcomes<T>(
                T published, IEnumerable<IEvent> outcomes)
                where T : IEvent
            {
                var publishedTrackingId = store.Add(published);

                if (outcomeTracking.TryGetValue(published, out var causedById))
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
