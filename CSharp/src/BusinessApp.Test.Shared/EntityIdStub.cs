namespace BusinessApp.Test.Shared
{
    using System;
    using BusinessApp.Domain;

    [System.ComponentModel.TypeConverter(typeof(EntityIdTypeConverter<EntityIdStub, int>))]
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
