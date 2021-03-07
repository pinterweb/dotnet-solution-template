using System;
using System.Collections.Generic;
using FakeItEasy;
using BusinessApp.Domain;
using Xunit;

namespace BusinessApp.Data.UnitTest
{
    public class EventMetadataTests
    {
        public class Constructor : EventMetadataTests
        {
            public static IEnumerable<object[]> InvalidArgs => new[]
            {
                new object[] { null, "foo" },
                new object[] { A.Dummy<IDomainEvent>(), null },
                new object[] { A.Dummy<IDomainEvent>(), "" },
            };

            [Theory, MemberData(nameof(InvalidArgs))]
            public void InvalidArgs_ExceptionThrown(IDomainEvent e, string c)
            {
                /* Arrange */
                void shouldThrow() => new EventMetadata(e, A.Dummy<EventId>(), c);

                /* Act */
                var ex = Record.Exception(shouldThrow);

                /* Assert */
                Assert.NotNull(ex);
            }

            [Fact]
            public void IDomainEventArg_IdPropertySet()
            {
                /* Arrange */
                var @event = A.Fake<IDomainEvent>();
                var eventId = A.Dummy<IEntityId>();
                A.CallTo(() => @event.Id).Returns(eventId);

                /* Act */
                var sut = new EventMetadata(@event, A.Dummy<EventId>(), "foo");

                /* Assert */
                Assert.Same(eventId, sut.Id);
            }

            [Fact]
            public void IDomainEventArg_EventDisplayTextPropertySet()
            {
                /* Arrange */
                var @event = A.Fake<IDomainEvent>();
                A.CallTo(() => @event.ToString()).Returns("foobar");

                /* Act */
                var sut = new EventMetadata(@event, A.Dummy<EventId>(), "f");

                /* Assert */
                Assert.Equal("foobar", sut.EventDisplayText);
            }

            [Fact]
            public void IDomainEventArg_OccurredUtcPropertySet()
            {
                /* Arrange */
                var @event = A.Fake<IDomainEvent>();
                var now = DateTimeOffset.UtcNow;
                A.CallTo(() => @event.OccurredUtc).Returns(now);

                /* Act */
                var sut = new EventMetadata(@event, A.Dummy<EventId>(), "foo");

                /* Assert */
                Assert.Equal(now, sut.OccurredUtc);
            }

            [Fact]
            public void CorrelationIdArg_CorrelationIdPropertySet()
            {
                /* Arrange */
                var correlationId = new EventId(1);

                /* Act */
                var sut = new EventMetadata(A.Dummy<IDomainEvent>(), correlationId, "foo");

                /* Assert */
                Assert.Same(correlationId, sut.CorrelationId);
            }

            [Fact]
            public void EventCreatorArg_EventCreatorPropertySet()
            {
                /* Arrange */
                var creator = "foobar";

                /* Act */
                var sut = new EventMetadata(A.Dummy<IDomainEvent>(), A.Dummy<EventId>(), creator);

                /* Assert */
                Assert.Equal("foobar", sut.EventCreator);
            }
        }

        public class ObjectToString : EventMetadataTests
        {
            [Fact]
            public void ReturnsDisplayText()
            {
                /* Arrange */
                var @event = A.Fake<IDomainEvent>();
                A.CallTo(() => @event.ToString()).Returns("foobar");
                var sut = new EventMetadata(@event, A.Dummy<EventId>(), "f");

                /* Act */
                var str = sut.ToString();

                /* Assert */
                Assert.Equal("foobar", str);
            }
        }
    }
}
