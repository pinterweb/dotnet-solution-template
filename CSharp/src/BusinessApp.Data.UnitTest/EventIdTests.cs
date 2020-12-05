using System;
using System.ComponentModel;
using BusinessApp.Domain;
using Xunit;

namespace BusinessApp.Data.UnitTest
{
    public class EventIdTests
    {
        public class GetTypeCode : EventIdTests
        {
            [Fact]
            public void ByDefault_ReturnsInnerIdTypeCode()
            {
                /* Arrange */
                long innerId = 1;

                /* Act */
                IConvertible sut = new EventId { Id = innerId };

                /* Assert */
                Assert.Equal(innerId.GetTypeCode(), sut.GetTypeCode());
            }
        }

        public class ToInt64 : EventIdTests
        {
            [Fact]
            public void ByDefault_ReturnsInnerIdValue()
            {
                /* Arrange */
                long innerId = 1;

                /* Act */
                IConvertible sut = new EventId { Id = innerId };

                /* Assert */
                Assert.Equal(innerId, sut.ToInt64(null));
            }
        }

        public class TypeConverter : EventIdTests
        {
            [Fact]
            public void ByDefault_HasEntityIdTypeConverterWithInt32()
            {
                /* Arrange */
                var expectedType = typeof(EntityIdTypeConverter<EventId, long>);

                /* Act */
                var converter = TypeDescriptor.GetConverter(typeof(EventId));

                /* Assert */
                Assert.IsType(expectedType, converter);
            }
        }
    }
}
