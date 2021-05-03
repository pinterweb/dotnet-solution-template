using System;
using BusinessApp.Kernel;

namespace BusinessApp.Infrastructure
{
    /// <summary>
    /// Data describing an event
    /// </summary>
    public abstract class EventMetadata
    {
#nullable disable
        protected EventMetadata()
        { }
#nullable restore

        public EventMetadata(EventTrackingId id, IDomainEvent e)
        {
            Id = id.NotNull().Expect(nameof(id)).Id;
            CorrelationId = id.CorrelationId;
            CausationId = id.CausationId;
            EventName = e.NotNull().Expect(nameof(e))
                .ToString()
                .Expect("Event ToString must return a value");
            OccurredUtc = e.OccurredUtc;
        }

        public string EventName { get; }
        public MetadataId Id { get; }
        public MetadataId CausationId { get; }
        public MetadataId CorrelationId { get; }
        public DateTimeOffset OccurredUtc { get; }
    }

    /// <summary>
    /// Model to store event metadata.
    /// </summary>
    public class EventMetadata<T> : EventMetadata
        where T : notnull, IDomainEvent
    {
#nullable disable
        private EventMetadata()
        { }
#nullable restore

        public EventMetadata(EventTrackingId id, T e)
            : base(id, e) => Event = e.NotNull().Expect(nameof(e));

        public T Event { get; set; }
    }
}
