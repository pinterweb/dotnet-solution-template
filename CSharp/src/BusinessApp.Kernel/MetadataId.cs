using System;
using System.ComponentModel;

namespace BusinessApp.Kernel
{
    /// <summary>
    /// Unique id of an event
    /// </summary>
    /// <remarks>Uses a class to share id values on save</remarks>
    [TypeConverter(typeof(EntityIdTypeConverter<MetadataId, long>))]
    public class MetadataId : IEntityId
    {
        public MetadataId(long id) => Id = id;

        [Id]
        public long Id { get; set; }

        TypeCode IConvertible.GetTypeCode() => Id.GetTypeCode();

        long IConvertible.ToInt64(IFormatProvider? provider) => Id;

        public static explicit operator long(MetadataId id) => id.Id;

        public static explicit operator MetadataId(long id) => new(id);
    }
}
