namespace BusinessApp.Data.IntegrationTest
{
    using System;
    using System.ComponentModel;
    using BusinessApp.Domain;
    using Xunit;

    public class LongEntityIdFactoryTests
    {
        [Fact]
        public void CanConvertFromLong_HasLongId()
        {
            /* Arrange */
            var sut = new LongEntityIdFactory<LongId>();

            /* Act */
            var id = sut.Create();

            /* Assert */
            Assert.NotEqual(0, id.Id);
        }

        [Fact]
        public void NoTypeConverter_Throws()
        {
            /* Arrange */
            var sut = new LongEntityIdFactory<NoTypeConverter>();

            /* Act */
            var ex = Record.Exception(sut.Create);

            /* Assert */
            Assert.IsType<BusinessAppDataException>(ex);
            Assert.Equal("NoTypeConverter must be able to convert from an Int64", ex.Message);
        }

        [Fact]
        public void WrongTypeConverter_Throws()
        {
            /* Arrange */
            var sut = new LongEntityIdFactory<WrongTypeConverter>();

            /* Act */
            var ex = Record.Exception(sut.Create);

            /* Assert */
            Assert.IsType<BusinessAppDataException>(ex);
            Assert.Equal("WrongTypeConverter must be able to convert from an Int64", ex.Message);
        }

        [TypeConverter(typeof(EntityIdTypeConverter<LongId, long>))]
        public class LongId : IEntityId
        {
            public LongId(long id)
            {
                Id = id;
            }

            public long Id { get; set; }
            public TypeCode GetTypeCode() => Id.GetTypeCode();
        }

        public class NoTypeConverter : IEntityId
        {
            public long Id { get; set; }
            public TypeCode GetTypeCode() => Id.GetTypeCode();
        }

        [TypeConverter(typeof(EntityIdTypeConverter<WrongTypeConverter, int>))]
        public class WrongTypeConverter : IEntityId
        {
            public int Id { get; set; }
            public TypeCode GetTypeCode() => Id.GetTypeCode();
        }
    }
}
