using System.Collections.Generic;

namespace BusinessApp.Domain
{
    // public class EventOriginId : IEntityId
    // {
    //     public EventOriginId(long id)
    //     {
    //         Id = id;
    //     }

    //     public long Id { get; }

    //     public TypeCode GetTypeCode() => Id.GetTypeCode();
    // }

    // public interface IEventOriginator<T>
    // {
    //     public EventOriginId Id { get; }
    //     public T Originator { get; }
    //     public IEnumerable<EventMetadata> PublishedEvents { get; }
    // }

    public interface IEventStream
    {
        IEnumerable<IDomainEvent> Events { get; set; }
    }

    // public class EventMetadata : IDomainEvent
    // {
    //     public EventMetadata(EventTrackingId id, IDomainEvent e)
    //     {
    //         Event = e.NotNull().Expect(nameof(e));
    //         EventName = e.ToString();
    //     }

    //     public EventTrackingId Id { get; }
    //     public string EventName { get; }
    //     public IDomainEvent Event { get; set; }
    //     public DateTimeOffset OccurredUtc => Event.OccurredUtc;
    // }

    // public class EventTrackingId
    // {
    //     public EventTrackingId(IEntityId id, IEntityId causationId, IEntityId correlationId)
    //     {
    //         Id = id.NotNull().Expect(nameof(id));
    //         CausationId = causationId.NotNull().Expect(nameof(causationId));
    //         CorrelationId = correlationId.NotNull().Expect(nameof(correlationId));
    //     }

    //     public IEntityId Id { get; }
    //     public IEntityId CausationId { get; }
    //     public IEntityId CorrelationId { get; }
    // }
}
