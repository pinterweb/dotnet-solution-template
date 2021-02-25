namespace BusinessApp.Test
{
    using System;
    using BusinessApp.Domain;
    using BusinessApp.WebApi;

    [System.ComponentModel.TypeConverter(typeof(EntityIdTypeConverter<EntityId, int>))]
    public class EntityIdStub : IEntityId
    {
        public EntityIdStub(int id)
        {
            Id = id;
        }

        public int Id { get; set; }

        public int ToInt32(IFormatProvider provider) => Id;
        public TypeCode GetTypeCode() => Id.GetTypeCode();
    }
}
