namespace BusinessApp.Domain
{
    using System;

    public class EventId : IEntityId
    {
        public EventId(long id)
        {
            Id = id;
        }

        public long Id { get; }

        public TypeCode GetTypeCode() => Id.GetTypeCode();

        public static explicit operator long (EventId id) => id.Id;
    }
}
