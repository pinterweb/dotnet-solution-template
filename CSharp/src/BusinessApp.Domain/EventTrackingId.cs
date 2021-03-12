namespace BusinessApp.Domain
{
    public class EventTrackingId
    {
        public EventTrackingId(IEntityId id, IEntityId causationId, IEntityId correlationId)
        {
            Id = id.NotNull().Expect(nameof(id));
            CausationId = causationId.NotNull().Expect(nameof(causationId));
            CorrelationId = correlationId.NotNull().Expect(nameof(correlationId));
        }

        public IEntityId Id { get; }
        public IEntityId CausationId { get; }
        public IEntityId CorrelationId { get; }
    }
}
