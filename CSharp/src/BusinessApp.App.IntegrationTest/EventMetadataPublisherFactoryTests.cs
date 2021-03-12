namespace BusinessApp.App.IntegrationTest
{
    using FakeItEasy;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using System.Threading;
    using Xunit;
    using BusinessApp.App;
    using BusinessApp.Domain;
    using System;

    public class EventMetadataPublisherFactoryTests
    {
        private readonly CancellationToken cancelToken;
        private readonly EventMetadataPublisherFactory sut;
        private readonly IEventPublisher inner;
        private readonly IEventStore store;
        private readonly IEntityIdFactory<EventId> idFactory;

        public EventMetadataPublisherFactoryTests()
        {
            cancelToken = A.Dummy<CancellationToken>();
            inner = A.Fake<IEventPublisher>();
            store = A.Fake<IEventStore>();
            idFactory = A.Fake<IEntityIdFactory<EventId>>();

            sut = new EventMetadataPublisherFactory(inner, store, idFactory);
        }

        public class Constructor : EventMetadataPublisherFactoryTests
        {
            public static IEnumerable<object[]> InvalidCtorArgs => new[]
            {
                new object[]
                {
                    null,
                    A.Dummy<IEventStore>(),
                    A.Dummy<IEntityIdFactory<EventId>>()
                },
                new object[]
                {
                    A.Dummy<IEventPublisher>(),
                    null,
                    A.Dummy<IEntityIdFactory<EventId>>()
                },
                new object[]
                {
                    A.Dummy<IEventPublisher>(),
                    A.Dummy<IEventStore>(),
                    null
                }
            };

            [Theory, MemberData(nameof(InvalidCtorArgs))]
            public void InvalidCtorArgs_ExceptionThrown(IEventPublisher p, IEventStore s,
                IEntityIdFactory<EventId> f)
            {
                /* Arrange */
                void shouldThrow() => new EventMetadataPublisherFactory(p, s, f);

                /* Act */
                var ex = Record.Exception(shouldThrow);

                /* Assert */
                Assert.IsType<BadStateException>(ex);
            }
        }

        public class Create : EventMetadataPublisherFactoryTests
        {
            [Fact]
            public void AddMetadataStreamToStore()
            {
                /* Arrange */
                var originator = A.Dummy<object>();
                EventMetadataStream<object> metadata = null;
                A.CallTo(() => store.Add(A<EventMetadataStream<object>>._))
                    .Invokes(c => metadata = c.GetArgument<EventMetadataStream<object>>(0));

                /* Act */
                var publisher = sut.Create(originator);

                /* Assert */
                Assert.Same(originator, metadata.Trigger);
            }

            [Fact]
            public void CreatesIdOnMetadata()
            {
                /* Arrange */
                var originator = A.Dummy<object>();
                var id = A.Dummy<EventId>();
                A.CallTo(() => idFactory.Create()).Returns(id);
                EventMetadataStream<object> metadata = null;
                A.CallTo(() => store.Add(A<EventMetadataStream<object>>._))
                    .Invokes(c => metadata = c.GetArgument<EventMetadataStream<object>>(0));

                /* Act */
                var publisher = sut.Create(originator);

                /* Assert */
                Assert.Equal(id, metadata.Id);
            }
        }

        public class PublishAsync : EventMetadataPublisherFactoryTests
        {
            private readonly IEventPublisher publisher;
            private readonly EventId originatorId;
            private EventMetadataStream<object> metadata;

            public PublishAsync()
            {
                var originator = A.Dummy<object>();
                originatorId = A.Dummy<EventId>();
                A.CallTo(() => idFactory.Create()).Returns(originatorId).Once();
                A.CallTo(() => store.Add(A<EventMetadataStream<object>>._))
                    .Invokes(c => metadata = c.GetArgument<EventMetadataStream<object>>(0));
                publisher = sut.Create(originator);
            }

            [Fact]
            public async Task CreatesIdOnMetadata()
            {
                /* Arrange */
                var e = A.Dummy<IDomainEvent>();
                var id = A.Dummy<EventId>();
                A.CallTo(() => idFactory.Create()).Returns(id);
                A.CallTo(() => inner.PublishAsync(e, cancelToken))
                    .Returns(Result.Ok<IEnumerable<IDomainEvent>>(new IDomainEvent[0]));

                /* Act */
                var events = await publisher.PublishAsync(e, cancelToken);

                /* Assert */
                var eventMeta = Assert.Single(metadata.EventsSeen);
                Assert.Same(id, eventMeta.Value.Id.Id);
            }

            [Fact]
            public async Task NewEvent_UsesOriginatorIdForCausationId()
            {
                /* Arrange */
                var e = A.Dummy<IDomainEvent>();
                A.CallTo(() => inner.PublishAsync(e, cancelToken))
                    .Returns(Result.Ok<IEnumerable<IDomainEvent>>(new IDomainEvent[0]));

                /* Act */
                var events = await publisher.PublishAsync(e, cancelToken);

                /* Assert */
                var eventMeta = Assert.Single(metadata.EventsSeen);
                Assert.Same(originatorId, eventMeta.Value.Id.CausationId);
            }

            [Fact]
            public async Task NewEvent_UsesOriginatorIdForCorrelationId()
            {
                /* Arrange */
                var e = A.Dummy<IDomainEvent>();
                A.CallTo(() => inner.PublishAsync(e, cancelToken))
                    .Returns(Result.Ok<IEnumerable<IDomainEvent>>(new IDomainEvent[0]));

                /* Act */
                var events = await publisher.PublishAsync(e, cancelToken);

                /* Assert */
                var eventMeta = Assert.Single(metadata.EventsSeen);
                Assert.Same(originatorId, eventMeta.Value.Id.CorrelationId);
            }

            [Fact]
            public async Task NewEvent_HasEventInMetadata()
            {
                /* Arrange */
                var e = A.Dummy<IDomainEvent>();
                A.CallTo(() => inner.PublishAsync(e, cancelToken))
                    .Returns(Result.Ok<IEnumerable<IDomainEvent>>(new IDomainEvent[0]));

                /* Act */
                var events = await publisher.PublishAsync(e, cancelToken);

                /* Assert */
                var eventMeta = Assert.Single(metadata.EventsSeen);
                Assert.Same(e, eventMeta.Value.Event);
            }

            [Fact]
            public async Task SameEventMultipleTimes_OnlyOneMetadataCreated()
            {
                /* Arrange */
                var e = A.Dummy<IDomainEvent>();
                A.CallTo(() => inner.PublishAsync(e, cancelToken))
                    .Returns(Result.Ok<IEnumerable<IDomainEvent>>(new IDomainEvent[0]));

                /* Act */
                var tasks = Enumerable.Range(0, 50).Select(i => publisher.PublishAsync(e, cancelToken));
                await Task.WhenAll(tasks);

                /* Assert */
                var eventMeta = Assert.Single(metadata.EventsSeen);
            }

            [Fact]
            public async Task NewEvent_AddsOutcomesToMetadataStream()
            {
                /* Arrange */
                var originalEvent = A.Dummy<IDomainEvent>();
                var outcomes = A.CollectionOfDummy<IDomainEvent>(1);
                A.CallTo(() => inner.PublishAsync(originalEvent, cancelToken))
                    .Returns(Result.Ok<IEnumerable<IDomainEvent>>(outcomes));

                /* Act */
                var events = await publisher.PublishAsync(originalEvent, cancelToken);

                /* Assert */
                Assert.Contains(originalEvent, metadata.EventsSeen);
                Assert.Contains(outcomes.First(), metadata.EventsSeen);
            }

            [Fact]
            public async Task EventOutcome_HasOwnId()
            {
                /* Arrange */
                var originalEvent = A.Dummy<IDomainEvent>();
                var outcomes = A.CollectionOfDummy<IDomainEvent>(1);
                var id = A.Dummy<EventId>();
                A.CallTo(() => idFactory.Create())
                    .Returns(A.Dummy<EventId>()) .Once()
                    .Then.Returns(id);
                A.CallTo(() => inner.PublishAsync(originalEvent, cancelToken))
                    .Returns(Result.Ok<IEnumerable<IDomainEvent>>(outcomes));

                /* Act */
                var events = await publisher.PublishAsync(originalEvent, cancelToken);

                /* Assert */
                var target = Assert.Contains(outcomes.First(), metadata.EventsSeen);
                Assert.Same(id, target.Id.Id);
            }

            [Fact]
            public async Task EventOutcome_HasOriginalEventIdAsCausationId()
            {
                /* Arrange */
                var originalEvent = A.Dummy<IDomainEvent>();
                var outcomes = A.CollectionOfDummy<IDomainEvent>(1);
                var id = A.Dummy<EventId>();
                A.CallTo(() => idFactory.Create())
                    .Returns(A.Dummy<EventId>()) .Once()
                    .Then.Returns(id);
                A.CallTo(() => inner.PublishAsync(originalEvent, cancelToken))
                    .Returns(Result.Ok<IEnumerable<IDomainEvent>>(outcomes));

                /* Act */
                var events = await publisher.PublishAsync(originalEvent, cancelToken);

                /* Assert */
                var target = Assert.Contains(outcomes.First(), metadata.EventsSeen);
                var originalMetadata = metadata.EventsSeen.Single(s => !s.Value.Equals(target));
                Assert.Same(originalMetadata.Value.Id.Id, target.Id.CausationId);
            }

            [Fact]
            public async Task EventOutcome_HasOriginalOriginatorIdAsCorrelationId()
            {
                /* Arrange */
                var originalEvent = A.Dummy<IDomainEvent>();
                var outcomes = A.CollectionOfDummy<IDomainEvent>(1);
                var id = A.Dummy<EventId>();
                A.CallTo(() => idFactory.Create())
                    .Returns(A.Dummy<EventId>()) .Once()
                    .Then.Returns(id);
                A.CallTo(() => inner.PublishAsync(originalEvent, cancelToken))
                    .Returns(Result.Ok<IEnumerable<IDomainEvent>>(outcomes));

                /* Act */
                var events = await publisher.PublishAsync(originalEvent, cancelToken);

                /* Assert */
                var target = Assert.Contains(outcomes.First(), metadata.EventsSeen);
                var originalMetadata = metadata.EventsSeen.Single(s => !s.Value.Equals(target));
                Assert.Same(originatorId, target.Id.CorrelationId);
            }

            [Fact]
            public async Task InnerEventError_ThatErrorReturned()
            {
                /* Arrange */
                var error = new Exception();
                A.CallTo(() => inner.PublishAsync(A<IDomainEvent>._, A<CancellationToken>._))
                    .Returns(Result.Error<IEnumerable<IDomainEvent>>(error));

                /* Act */
                var result = await publisher.PublishAsync(A.Dummy<IDomainEvent>(), cancelToken);

                /* Assert */
                Assert.Equal(Result.Error<IEnumerable<IDomainEvent>>(error), result);
            }
        }
    }
}
