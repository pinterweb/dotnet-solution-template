namespace BusinessApp.App
{
    using System.Collections.Generic;
    using BusinessApp.Domain;
    using System.Collections.Concurrent;
    using System.Linq;

    public class EventMetadataStream<T> : IEventStream
    {
        public EventMetadataStream(EventId id, T trigger)
        {
            Id = id.NotNull().Expect(nameof(id));
            Trigger = trigger.NotDefault().Expect(nameof(trigger));
        }

        public EventId Id { get; }
        public T Trigger { get; }
        public IDictionary<IDomainEvent, EventMetadata> EventsSeen { get; private set; }
            = new ConcurrentDictionary<IDomainEvent, EventMetadata>();
        public IEnumerable<IDomainEvent> Events
        {
            get => EventsSeen.Select(kvp => kvp.Value);

            set => EventsSeen = new ConcurrentDictionary<IDomainEvent, EventMetadata>(
                value.ToDictionary(e => e, e => new EventMetadata(
                        new EventTrackingId(Id, Id, Id), e)));
        }
    }
}
