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
                new object[] { A.Dummy<IDomainEvent>(), "", "foo" },
            };

            [Theory, MemberData(nameof(InvalidArgs))]
            public void InvalidArgs_ExceptionThrown(IDomainEvent e, string c)
            {
                /* Arrange */
                void shouldThrow() => new EventMetadata(A.Dummy<EventId>(),
                    A.Dummy<EventId>(),
                    e,
                    c);

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
                var eventId = new EventId { Id  = 11 };
                A.CallTo(() => @event.Id).Returns(eventId);

                /* Act */
                var sut = new EventMetadata(A.Dummy<EventId>(),
                    A.Dummy<EventId>(),
                    @event,
                    "foo");

                /* Assert */
                Assert.Equal(eventId, sut.Id);
            }

            [Fact]
            public void IDomainEventArg_EventDisplayTextPropertySet()
            {
                /* Arrange */
                var @event = A.Fake<IDomainEvent>();
                A.CallTo(() => @event.ToString("G", null)).Returns("foobar");

                /* Act */
                var sut = new EventMetadata(A.Dummy<EventId>(),
                    A.Dummy<EventId>(),
                    @event,
                    "f");

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
                var sut = new EventMetadata(A.Dummy<EventId>(),
                    A.Dummy<EventId>(),
                    @event,
                    "foo");

                /* Assert */
                Assert.Equal(now, sut.OccurredUtc);
            }

            [Fact]
            public void CorrelationIdArg_CorrelationIdPropertySet()
            {
                /* Arrange */
                var correlationId = new EventId { Id = 1 };

                /* Act */
                var sut = new EventMetadata(A.Dummy<EventId>(),
                    correlationId,
                    A.Dummy<IDomainEvent>(),
                    "foo");

                /* Assert */
                Assert.Same(correlationId, sut.CorrelationId);
            }

            [Fact]
            public void EventCreatorArg_EventCreatorPropertySet()
            {
                /* Arrange */
                var creator = "foobar";

                /* Act */
                var sut = new EventMetadata(A.Dummy<EventId>(),
                    A.Dummy<EventId>(),
                    A.Dummy<IDomainEvent>(),
                    creator);

                /* Assert */
                Assert.Equal("foobar", sut.EventCreator);
            }
        }

        public class IFormattableToString : EventMetadataTests
        {
            [Fact]
            public void ObjectToString_IFormattableInterfaceCalled()
            {
                /* Arrange */
                var @event = A.Fake<IDomainEvent>();
                A.CallTo(() => @event.ToString("G", null)).Returns("foobar");
                var sut = new EventMetadata(A.Dummy<EventId>(),
                    A.Dummy<EventId>(),
                    @event,
                    "f");

                /* Act */
                var str = sut.ToString();

                /* Assert */
                Assert.Equal("foobar", str);
            }

            [Fact]
            public void IFormattableToString_ReturnesDisplayText()
            {
                /* Arrange */
                var @event = A.Fake<IDomainEvent>();
                A.CallTo(() => @event.ToString("G", null)).Returns("foobar");
                var sut = new EventMetadata(A.Dummy<EventId>(),
                    A.Dummy<EventId>(),
                    @event,
                    "f");

                /* Act */
                var str = sut.ToString(A.Dummy<string>(), A.Dummy<IFormatProvider>());

                /* Assert */
                Assert.Equal("foobar", str);
            }
        }
    }
}
