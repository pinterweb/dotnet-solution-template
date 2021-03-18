namespace BusinessApp.Domain.UnitTest
{
    using System;
    using System.ComponentModel;
    using Xunit;

    public class MetadataIdTests
    {
        public class Constructor : MetadataIdTests
        {
            [Fact]
            public void SetsIdProperty()
            {
                /* Arrange */
                long innerId = 1;

                /* Act */
                var sut = new MetadataId(innerId);

                /* Assert */
                Assert.Equal(innerId, sut.Id);
            }
        }

        public class GetTypeCode : MetadataIdTests
        {
            [Fact]
            public void ReturnsInnerIdTypeCode()
            {
                /* Arrange */
                long innerId = 1;

                /* Act */
                IConvertible sut = new MetadataId(innerId);

                /* Assert */
                Assert.Equal(innerId.GetTypeCode(), sut.GetTypeCode());
            }
        }

        public class ToInt64 : MetadataIdTests
        {
            [Fact]
            public void ReturnsInnerIdValue()
            {
                /* Arrange */
                long innerId = 1;

                /* Act */
                IConvertible sut = new MetadataId(innerId);

                /* Assert */
                Assert.Equal(innerId, sut.ToInt64(null));
            }
        }

        public class TypeConverter : MetadataIdTests
        {
            [Fact]
            public void HasEntityIdTypeConverterWithInt32()
            {
                /* Arrange */
                var expectedType = typeof(EntityIdTypeConverter<MetadataId, long>);

                /* Act */
                var converter = TypeDescriptor.GetConverter(typeof(MetadataId));

                /* Assert */
                Assert.IsType(expectedType, converter);
            }
        }

        public class ExplicitCast : MetadataIdTests
        {
            [Fact]
            public void ToLong_ReturnsInnerIdValue()
            {
                /* Arrange */
                long expectId = 1;
                var sut = new MetadataId(expectId);

                /* Act */
                var actualId = (long)sut;

                /* Assert */
                Assert.Equal(expectId, actualId);
            }

            [Fact]
            public void FromLong_ReturnsNewMetadataId()
            {
                /* Arrange */
                long primitive = 1;

                /* Act */
                var sut = (MetadataId)primitive;

                /* Assert */
                Assert.Equal(1, sut.Id);
            }
        }
    }
}
