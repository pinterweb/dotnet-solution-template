namespace BusinessApp.Data
{
    using System;
    using System.ComponentModel;
    using BusinessApp.Domain;

    /// <summary>
    /// Unique id of an event
    /// </summary>
    /// <remarks>Uses a class to share id values on save</remarks>
    [TypeConverter(typeof(EntityIdTypeConverter<EventId, long>))]
    public class EventId : IEntityId
    {
        [KeyId]
        public long Id { get; set; }

        TypeCode IConvertible.GetTypeCode() => Id.GetTypeCode();

        long IConvertible.ToInt64(IFormatProvider provider) => Id;
    }
}
