namespace BusinessApp.Domain.UnitTest
{
    using System.Collections.Generic;
    using FakeItEasy;
    using Xunit;

    public class EventTrackingIdTests
    {
        public class Constructor : EventTrackingIdTests
        {
            public static IEnumerable<object[]> InvalidArgs => new[]
            {
                new object[] { null, A.Dummy<MetadataId>() },
                new object[] { A.Dummy<MetadataId>(), null }
            };

            [Theory, MemberData(nameof(InvalidArgs))]
            public void InvalidArgs_ExceptionThrown(MetadataId i, MetadataId c)
            {
                /* Arrange */
                void shouldThrow() => new EventTrackingId(i, c);

                /* Act */
                var ex = Record.Exception(shouldThrow);

                /* Assert */
                Assert.NotNull(ex);
            }

            [Fact]
            public void SetsIdProperty()
            {
                /* Arrange */
                var id = (MetadataId)1;

                /* Act */
                var sut = new EventTrackingId(id, A.Dummy<MetadataId>());

                /* Assert */
                Assert.Equal(id, sut.Id);
            }

            [Fact]
            public void SetsCorrelationIdProperty()
            {
                /* Arrange */
                var correlationId = (MetadataId)1;

                /* Act */
                var sut = new EventTrackingId(A.Dummy<MetadataId>(), correlationId);

                /* Assert */
                Assert.Equal(correlationId, sut.CorrelationId);
            }

            [Fact]
            public void SetsCausationIdFromCorrelationIdProperty()
            {
                /* Arrange */
                var correlationId = (MetadataId)1;

                /* Act */
                var sut = new EventTrackingId(A.Dummy<MetadataId>(), correlationId);

                /* Assert */
                Assert.Equal(correlationId, sut.CausationId);
            }
        }
    }
}
