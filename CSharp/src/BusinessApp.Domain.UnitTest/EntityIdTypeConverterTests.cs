using System;
using BusinessApp.Domain;
using Xunit;

namespace BusinessApp.Domain.UnitTest
{
    public class EntityIdTypeConverterTests
    {
        public class CanConvertFrom : EntityIdTypeConverterTests
        {
            [Fact]
            public void WithoutInnerValueConstructor_FalseReturned()
            {
                /* Arrange */
                var sut = new EntityIdTypeConverter<BadEntityIdStub, int>();

                /* Act */
                var canConvert = sut.CanConvertFrom(null, typeof(int));

                /* Assert */
                Assert.False(canConvert);
            }

            [Fact]
            public void EqualSourceType_TrueReturned()
            {
                /* Arrange */
                var sut = new EntityIdTypeConverter<EntityIdStub, int>();

                /* Act */
                var canConvert = sut.CanConvertFrom(null, typeof(int));

                /* Assert */
                Assert.True(canConvert);
            }

            [Fact]
            public void DifferentType_WhenInnerConverterCanConvert_TrueReturned()
            {
                /* Arrange */
                var sut = new EntityIdTypeConverter<EntityIdStub, int>();

                /* Act */
                var canConvert = sut.CanConvertFrom(null, typeof(string));

                /* Assert */
                Assert.True(canConvert);
            }
        }

        public class ConvertFrom : EntityIdTypeConverterTests
        {
            [Fact]
            public void WithoutInnerValueConstructor_ExceptionThrown()
            {
                /* Arrange */
                var sut = new EntityIdTypeConverter<BadEntityIdStub, int>();
                int source = 1;

                /* Act */
                var ex = Record.Exception(() => sut.ConvertFrom(null, null, source));

                /* Assert */
                Assert.IsType<FormatException>(ex);
                Assert.Equal(
                    "To convert from 'System.Int32' to 'BadEntityIdStub', the IEntityId " +
                    "needs a constructor that has an 'System.Int32' argument only",
                    ex.Message);
            }

            [Fact]
            public void EqualSourceType_TrueReturned()
            {
                /* Arrange */
                var sut = new EntityIdTypeConverter<EntityIdStub, int>();
                int source = 1;

                /* Act */
                var result = sut.ConvertFrom(null, null, source);

                /* Assert */
                var obj = Assert.IsType<EntityIdStub>(result);
                Assert.Equal(1, obj.Id);
            }

            [Fact]
            public void DifferentType_WhenInnerConverterCanConvert_TrueReturned()
            {
                /* Arrange */
                var sut = new EntityIdTypeConverter<EntityIdStub, int>();
                string source = "+123";

                /* Act */
                var result = sut.ConvertFrom(null, null, source);

                /* Assert */
                var obj = Assert.IsType<EntityIdStub>(result);
                Assert.Equal(123, obj.Id);
            }
        }

        public class CanConvertTo : EntityIdTypeConverterTests
        {
            [Fact]
            public void DestinationTypeIsInnerType_TrueReturned()
            {
                /* Arrange */
                var sut = new EntityIdTypeConverter<BadEntityIdStub, int>();

                /* Act */
                var canConvert = sut.CanConvertTo(null, typeof(int));

                /* Assert */
                Assert.True(canConvert);
            }

            [Fact]
            public void DifferentType_WhenInnerConverterCanConvert_TrueReturned()
            {
                /* Arrange */
                var sut = new EntityIdTypeConverter<EntityIdStub, int>();

                /* Act */
                var canConvert = sut.CanConvertTo(null, typeof(string));

                /* Assert */
                Assert.True(canConvert);
            }
        }

        public class ConvertTo : EntityIdTypeConverterTests
        {
            [Fact]
            public void ValueIsNotEntityIdStub_ExceptionThrown()
            {
                /* Arrange */
                var sut = new EntityIdTypeConverter<EntityIdStub, int>();
                var source = new BadEntityIdStub();

                /* Act */
                var ex = Record.Exception(() => sut.ConvertTo(null, null, source, typeof(int)));

                /* Assert */
                Assert.IsType<FormatException>(ex);
                Assert.Equal("Source value must be 'EntityIdStub'", ex.Message);
            }

            [Fact]
            public void ValueIsEntityIdStub_PrimitiveTypeReturned()
            {
                /* Arrange */
                var sut = new EntityIdTypeConverter<EntityIdStub, int>();
                var source = new EntityIdStub(1);

                /* Act */
                var val = sut.ConvertTo(null, null, source, typeof(int));

                /* Assert */
                var intVal = Assert.IsType<int>(val);
                Assert.Equal(1, intVal);
            }

            [Fact]
            public void ValueIsEntityIdStub_DestinationTypeReturned()
            {
                /* Arrange */
                var sut = new EntityIdTypeConverter<EntityIdStub, int>();
                var source = new EntityIdStub(1);

                /* Act */
                var val = sut.ConvertTo(null, null, source, typeof(string));

                /* Assert */
                var strVal = Assert.IsType<string>(val);
                Assert.Equal("1", strVal);
            }
        }

        public struct EntityIdStub : IEntityId
        {
            public EntityIdStub(int id)
            {
                Id = id;
            }

            public int Id { get; set; }
            public TypeCode GetTypeCode() => Id.GetTypeCode();

            int IConvertible.ToInt32(IFormatProvider provider) => Id;
        }

        public struct BadEntityIdStub : IEntityId
        {
            public int Id { get; set; }
            public TypeCode GetTypeCode() => Id.GetTypeCode();

            int IConvertible.ToInt32(IFormatProvider provider) => Id;
        }
    }
}
