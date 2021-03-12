namespace BusinessApp.Data.UnitTest
{
    using System;
    using System.ComponentModel;
    using BusinessApp.Domain;
    using Xunit;

    public class EventIdTests
    {
        public class Constructor : EventIdTests
        {
            [Fact]
            public void SetsIdProperty()
            {
                /* Arrange */
                long innerId = 1;

                /* Act */
                var sut = new Data.EventId(innerId);

                /* Assert */
                Assert.Equal(innerId, sut.Id);
            }
        }

        public class GetTypeCode : EventIdTests
        {
            [Fact]
            public void ReturnsInnerIdTypeCode()
            {
                /* Arrange */
                long innerId = 1;

                /* Act */
                IConvertible sut = new Data.EventId(innerId);

                /* Assert */
                Assert.Equal(innerId.GetTypeCode(), sut.GetTypeCode());
            }
        }

        public class ToInt64 : EventIdTests
        {
            [Fact]
            public void ReturnsInnerIdValue()
            {
                /* Arrange */
                long innerId = 1;

                /* Act */
                IConvertible sut = new Data.EventId(innerId);

                /* Assert */
                Assert.Equal(innerId, sut.ToInt64(null));
            }
        }

        public class TypeConverter : EventIdTests
        {
            [Fact]
            public void HasEntityIdTypeConverterWithInt32()
            {
                /* Arrange */
                var expectedType = typeof(EntityIdTypeConverter<Data.EventId, long>);

                /* Act */
                var converter = TypeDescriptor.GetConverter(typeof(Data.EventId));

                /* Assert */
                Assert.IsType(expectedType, converter);
            }
        }

        public class ImplicitCast : EventIdTests
        {
            [Fact]
            public void ReturnsInnerIdValue()
            {
                /* Arrange */
                long expectId = 1;
                var sut = new Data.EventId(expectId);

                /* Act */
                var actualId = (long)sut;

                /* Assert */
                Assert.Equal(expectId, actualId);
            }
        }
    }
}
