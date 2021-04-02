namespace BusinessApp.Domain
{
    /// <summary>
    /// Represents IDs to a track events in an <see cref="IEventStream" />
    /// </summary>
    public class EventTrackingId
    {
        private MetadataId causationId;

        public EventTrackingId(MetadataId id, MetadataId correlationId)
        {
            Id = id.NotNull().Expect(nameof(id));
            CorrelationId = correlationId.NotNull().Expect(nameof(correlationId));
            causationId = correlationId;
        }

        /// <summary>
        /// The Id of the event
        /// </summary>
        public MetadataId Id { get; }

        /// <summary>
        /// Represents the id of all related events in a given scope.
        /// </summary>
        /// <remarks>
        /// The correlation id is normally the original id of the trigger object
        /// for all events that fire directly or indirectly
        /// </remarks>
        public MetadataId CorrelationId { get; }

        /// <summary>
        /// The id of the object that triggered the event
        /// </summary>
        public MetadataId CausationId
        {
            get => causationId;
            set => causationId = value.NotNull().Expect(nameof(CausationId));
        }
    }
}
