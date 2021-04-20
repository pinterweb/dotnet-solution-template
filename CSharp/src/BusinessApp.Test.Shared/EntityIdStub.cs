using System;
using BusinessApp.Kernel;

namespace BusinessApp.Test.Shared
{
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
