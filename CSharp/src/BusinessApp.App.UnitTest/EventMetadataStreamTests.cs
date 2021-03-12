namespace BusinessApp.App.UnitTest
{
    using FakeItEasy;
    using System.Collections.Generic;
    using Xunit;
    using BusinessApp.App;
    using BusinessApp.Domain;

    public class EventMetadataStreamTests
    {
        private EventMetadataStream<object> sut;

        public class Constructor : EventMetadataStreamTests
        {
            public static IEnumerable<object[]> InvalidCtorArgs => new[]
            {
                new object[]
                {
                    null,
                    A.Dummy<object>(),
                },
                new object[]
                {
                    A.Dummy<EventId>(),
                    null,
                },
            };

            [Theory, MemberData(nameof(InvalidCtorArgs))]
            public void InvalidCtorArgs_ExceptionThrown(EventId i, object o)
            {
                /* Arrange */
                void shouldThrow() => new EventMetadataStream<object>(i, o);

                /* Act */
                var ex = Record.Exception(shouldThrow);

                /* Assert */
                Assert.IsType<BadStateException>(ex);
            }

            [Fact]
            public void EventIdArg_PropertySet()
            {
                /* Arrange */
                var id = new EventId(1);

                /* Act */
                var sut = new EventMetadataStream<object>(id, A.Dummy<object>());

                /* Assert */
                Assert.Same(id, sut.Id);
            }

            [Fact]
            public void TriggerArg_PropertySet()
            {
                /* Arrange */
                var o = new object();

                /* Act */
                var sut = new EventMetadataStream<object>(A.Dummy<EventId>(), o);

                /* Assert */
                Assert.Same(o, sut.Trigger);
            }
        }

        public class EventsProperty : EventMetadataStreamTests
        {
            private readonly IDomainEvent e;
            private readonly EventMetadata metadata;

            public EventsProperty()
            {
                var id = A.Dummy<IEntityId>();
                sut = new EventMetadataStream<object>(A.Dummy<EventId>(), A.Dummy<object>());

                e = A.Dummy<IDomainEvent>();
                metadata = new EventMetadata(new EventTrackingId(id, id, id), e);
            }

            [Fact]
            public void Gets_FromEventsSeen()
            {
                /* Arrange */
                sut.EventsSeen.Add(e, metadata);

                /* Act */
                var stream = sut.Events;

                /* Assert */
                Assert.Collection(stream,
                    s => Assert.Same(s, metadata));
            }

            [Fact]
            public void Set_ResetsEventsSeenEventWithEvent()
            {
                /* Arrange */
                sut.Events = new[] { e };

                /* Act */
                var stream = sut.EventsSeen;

                /* Assert */
                Assert.Collection(stream,
                    s => Assert.Same(s.Key, e));
            }

            [Fact]
            public void Set_ResetsEventsSeenEventWithNewMetadataId()
            {
                /* Arrange */
                sut.Events = new[] { e };

                /* Act */
                var stream = sut.EventsSeen;

                /* Assert */
                Assert.Collection(stream,
                    s => Assert.Same(s.Value.Id.Id, sut.Id));
            }

            [Fact]
            public void Set_ResetsEventsSeenEventWithNewMetadataCausationId()
            {
                /* Arrange */
                sut.Events = new[] { e };

                /* Act */
                var stream = sut.EventsSeen;

                /* Assert */
                Assert.Collection(stream,
                    s => Assert.Same(s.Value.Id.CausationId, sut.Id));
            }

            [Fact]
            public void Set_ResetsEventsSeenEventWithNewMetadataCorrelationId()
            {
                /* Arrange */
                sut.Events = new[] { e };

                /* Act */
                var stream = sut.EventsSeen;

                /* Assert */
                Assert.Collection(stream,
                    s => Assert.Same(s.Value.Id.CorrelationId, sut.Id));
            }
        }
    }
}
