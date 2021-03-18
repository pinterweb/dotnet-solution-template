namespace BusinessApp.Data
{
    using System;
    using BusinessApp.Domain;

    public abstract class EventMetadata
    {
        protected EventMetadata()
        {}

        public EventMetadata(EventTrackingId id, IDomainEvent e)
        {
            id.NotNull().Expect(nameof(id));

            Id = id.Id;
            CorrelationId = id.CorrelationId;
            CausationId = id.CausationId;
            EventName = e.ToString();
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
        where T : IDomainEvent
    {
        private EventMetadata()
        {}

        public EventMetadata(EventTrackingId id, T e)
            : base(id, e)
        {
            id.NotNull().Expect(nameof(id));

            Event = e.NotNull().Expect(nameof(e));
        }

        public T Event { get; set; }
    }
}
