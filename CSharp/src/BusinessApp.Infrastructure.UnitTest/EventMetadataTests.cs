using System;
using System.Collections.Generic;
using FakeItEasy;
using BusinessApp.Kernel;
using Xunit;
using BusinessApp.Test.Shared;

namespace BusinessApp.Infrastructure.UnitTest
{
    public class EventMetadataTests
    {
        public class Constructor : EventMetadataTests
        {
            public static IEnumerable<object[]> InvalidArgs => new[]
            {
                new object[] { null, A.Dummy<EventTrackingId>() },
                new object[] { A.Dummy<EventStub>(), null},
            };

            [Theory, MemberData(nameof(InvalidArgs))]
            public void InvalidArgs_ExceptionThrown(EventStub e, EventTrackingId i)
            {
                /* Arrange */
                void shouldThrow() => new EventMetadata<EventStub>(i, e);

                /* Act */
                var ex = Record.Exception(shouldThrow);

                /* Assert */
                Assert.IsType<BusinessAppException>(ex);
            }

            [Fact]
            public void EventToStringNull_ExceptionThrown()
            {
                /* Arrange */
                var i = A.Dummy<EventTrackingId>();
                var e = A.Fake<EventStub>();
                A.CallTo(() => e.ToString()).Returns(null);
                void shouldThrow() => new EventMetadata<EventStub>(i, e);

                /* Act */
                var ex = Record.Exception(shouldThrow);

                /* Assert */
                Assert.IsType<BusinessAppException>(ex);
                Assert.Equal("Event ToString must return a value: object cannot be null", ex.Message);
            }

            [Fact]
            public void EventArg_EventPropertySet()
            {
                /* Arrange */
                var @event = A.Dummy<EventStub>();

                /* Act */
                var sut = new EventMetadata<EventStub>(A.Dummy<EventTrackingId>(), @event);

                /* Assert */
                Assert.Same(@event, sut.Event);
            }

            [Fact]
            public void EventArg_EventNamePropertySet()
            {
                /* Arrange */
                var @event = A.Fake<EventStub>();
                A.CallTo(() => @event.ToString()).Returns("foobar");

                /* Act */
                var sut = new EventMetadata<EventStub>(A.Dummy<EventTrackingId>(), @event);

                /* Assert */
                Assert.Equal("foobar", sut.EventName);
            }

            [Fact]
            public void EventArg_OccurredUtcPropertySet()
            {
                /* Arrange */
                var now = DateTimeOffset.UtcNow;
                var @event = new EventStub()
                {
                    OccurredUtc = now
                };

                /* Act */
                var sut = new EventMetadata<EventStub>(A.Dummy<EventTrackingId>(), @event);

                /* Assert */
                Assert.Equal(now, sut.OccurredUtc);
            }

            [Fact]
            public void EvenTrackingIdArg_IdPropertySet()
            {
                /* Arrange */
                var eventId = new MetadataId(1);
                var id = new EventTrackingId(eventId, A.Dummy<MetadataId>());

                /* Act */
                var sut = new EventMetadata<EventStub>(id, A.Dummy<EventStub>());

                /* Assert */
                Assert.Same(eventId, sut.Id);
            }

            [Fact]
            public void EvenTrackingIdArg_CorrelationIdPropertySet()
            {
                /* Arrange */
                var correlationId = new MetadataId(1);
                var id = new EventTrackingId(A.Dummy<MetadataId>(), correlationId);

                /* Act */
                var sut = new EventMetadata<EventStub>(id, A.Dummy<EventStub>());

                /* Assert */
                Assert.Same(correlationId, sut.CorrelationId);
            }

            [Fact]
            public void EvenTrackingIdArg_CausationIdPropertySet()
            {
                /* Arrange */
                var causationId = new MetadataId(1);
                var id = new EventTrackingId(A.Dummy<MetadataId>(), A.Dummy<MetadataId>())
                {
                    CausationId = causationId
                };

                /* Act */
                var sut = new EventMetadata<EventStub>(id, A.Dummy<EventStub>());

                /* Assert */
                Assert.Same(causationId, sut.CausationId);
            }
        }
    }
}
