using FakeItEasy;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading;
using Xunit;
using BusinessApp.Kernel;
using System;
using System.Linq;

namespace BusinessApp.Infrastructure.IntegrationTest
{
    public class EventMetadataPublisherFactoryTests
    {
        private readonly CancellationToken cancelToken;
        private readonly EventMetadataPublisherFactory sut;
        private readonly IEventPublisher inner;
        private readonly IEventStoreFactory storeFactory;

        public EventMetadataPublisherFactoryTests()
        {
            cancelToken = A.Dummy<CancellationToken>();
            inner = A.Fake<IEventPublisher>();
            storeFactory = A.Fake<IEventStoreFactory>();

            sut = new EventMetadataPublisherFactory(inner, storeFactory);
        }

        public class Constructor : EventMetadataPublisherFactoryTests
        {
            public static IEnumerable<object[]> InvalidCtorArgs => new[]
            {
                new object[]
                {
                    null,
                    A.Dummy<IEventStoreFactory>(),
                },
                new object[]
                {
                    A.Dummy<IEventPublisher>(),
                    null,
                },
            };

            [Theory, MemberData(nameof(InvalidCtorArgs))]
            public void InvalidCtorArgs_ExceptionThrown(IEventPublisher p,
                IEventStoreFactory s)
            {
                /* Arrange */
                void shouldThrow() => new EventMetadataPublisherFactory(p, s);

                /* Act */
                var ex = Record.Exception(shouldThrow);

                /* Assert */
                Assert.IsType<BusinessAppException>(ex);
            }
        }

        public class PublishAsync : EventMetadataPublisherFactoryTests
        {
            private readonly IEventPublisher publisher;
            private readonly IEventStore store;
            private readonly MetadataId triggerId;

            public PublishAsync()
            {
                var trigger = A.Dummy<object>();
                store = A.Fake<IEventStore>();
                triggerId = A.Dummy<MetadataId>();

                A.CallTo(() => storeFactory.Create(trigger)).Returns(store);

                publisher = sut.Create(trigger);
            }

            [Fact]
            public async Task IsOriginalEvent_CausationIdNotSet()
            {
                /* Arrange */
                var trackingId = new EventTrackingId((MetadataId)1, (MetadataId)2)
                {
                    CausationId = (MetadataId)3
                };
                var e = A.Dummy<IEvent>();
                A.CallTo(() => store.Add(e)).Returns(trackingId);
                A.CallTo(() => inner.PublishAsync(e, cancelToken))
                    .Returns(Result.Ok<IEnumerable<IEvent>>(new IEvent[0]));

                /* Act */
                var events = await publisher.PublishAsync(e, cancelToken);

                /* Assert */
                Assert.Equal(3, trackingId.CausationId.Id);
            }

            [Fact]
            public async Task HasEventOutcome_ThoseEventsReturns()
            {
                /* Arrange */
                var originalEvent = A.Dummy<IEvent>();
                var outcomes = A.CollectionOfDummy<IEvent>(1);
                A.CallTo(() => inner.PublishAsync(originalEvent, cancelToken))
                    .Returns(Result.Ok<IEnumerable<IEvent>>(outcomes));

                /* Act */
                var events = await publisher.PublishAsync(originalEvent, cancelToken);

                /* Assert */
                Assert.Same(outcomes, events.Unwrap());
            }

            [Fact]
            public async Task HasEventOutcome_OriginalEventIdSetAsCausationId()
            {
                /* Arrange */
                var originalEvent = A.Dummy<IEvent>();
                var outcomes = A.CollectionOfDummy<IEvent>(1);
                var firstTrackingId = new EventTrackingId((MetadataId)1, (MetadataId)2)
                {
                    CausationId = (MetadataId)3
                };
                var nextTrackingId = new EventTrackingId((MetadataId)4, (MetadataId)5)
                {
                    CausationId = (MetadataId)6
                };
                var trackingIds = new[] { firstTrackingId, nextTrackingId };
                A.CallTo(() => store.Add(originalEvent)).Returns(firstTrackingId);
                A.CallTo(() => store.Add(outcomes.First())).Returns(nextTrackingId);
                A.CallTo(() => inner.PublishAsync(originalEvent, cancelToken))
                    .Returns(Result.Ok<IEnumerable<IEvent>>(outcomes));
                A.CallTo(() => inner.PublishAsync(outcomes.First(), cancelToken))
                    .Returns(Result.Ok<IEnumerable<IEvent>>(A.CollectionOfDummy<IEvent>(0)));
                var _ = await publisher.PublishAsync(originalEvent, cancelToken);

                /* Act */
                await publisher.PublishAsync(outcomes.First(), cancelToken);

                /* Assert */
                Assert.Collection(trackingIds,
                    t => Assert.Equal(firstTrackingId.CausationId, t.CausationId),
                    t => Assert.Equal(1, t.CausationId.Id));
            }

            [Fact]
            public async Task InnerEventError_ThatErrorReturned()
            {
                /* Arrange */
                var error = new Exception();
                A.CallTo(() => inner.PublishAsync(A<IEvent>._, A<CancellationToken>._))
                    .Returns(Result.Error<IEnumerable<IEvent>>(error));

                /* Act */
                var result = await publisher.PublishAsync(A.Dummy<IEvent>(), cancelToken);

                /* Assert */
                Assert.Equal(Result.Error<IEnumerable<IEvent>>(error), result);
            }
        }
    }
}
