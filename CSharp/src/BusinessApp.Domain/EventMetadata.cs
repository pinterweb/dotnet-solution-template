namespace BusinessApp.Domain
{
    using System;

    public class EventMetadata : IDomainEvent
    {
        public EventMetadata(EventTrackingId id, IDomainEvent e)
        {
            Id = id.NotNull().Expect(nameof(id));
            Event = e.NotNull().Expect(nameof(e));
            EventName = e.ToString();
        }

        public EventTrackingId Id { get; }
        public string EventName { get; }
        public IDomainEvent Event { get; set; }
        public DateTimeOffset OccurredUtc => Event.OccurredUtc;
    }
}
