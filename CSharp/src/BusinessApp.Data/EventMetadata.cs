namespace BusinessApp.Data
{
    using System;
    using BusinessApp.Domain;

    /// <summary>
    /// Represents event data over time. Abstracts the time component away from
    /// the meaningful event data
    /// </summary>
    public sealed class EventMetadata : IDomainEvent
    {
        private EventMetadata() {  }

        public EventMetadata(EventId id,
            EventId correlationId,
            IDomainEvent originalEvent,
            string eventCreator)
        {
            originalEvent.NotNull().Expect(nameof(originalEvent));
            eventCreator.NotEmpty().Expect(nameof(eventCreator));

            Id = originalEvent.Id;
            CorrelationId = correlationId;
            EventDisplayText = originalEvent.ToString("G", null);
            EventCreator = eventCreator;
            OccurredUtc = originalEvent.OccurredUtc;
        }

        /// <summary>
        /// Unique Id of the event
        /// </summary>
        public IEntityId Id { get; set; }

        /// <summary>
        /// Shared Id of all related events
        /// </summary>
        public IEntityId CorrelationId { get; private set; }

        /// <summary>
        /// The display text of the original event
        /// </summary>
        public string EventDisplayText { get; private set; }

        /// <summary>
        /// The display text for the creator of the event
        /// </summary>
        public string EventCreator { get; private set; }

        /// <summary>
        /// The occurred UTC datetime of the original event
        /// </summary>
        public DateTimeOffset OccurredUtc { get; private set; }

        public override string ToString() => ToString("G", null);

        public string ToString(string format, IFormatProvider formatProvider) => EventDisplayText;
    }
}
