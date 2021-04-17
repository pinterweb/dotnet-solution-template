namespace BusinessApp.App.UnitTest
{
    using FakeItEasy;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using System.Threading;
    using System;
    using Xunit;
    using BusinessApp.App;
    using BusinessApp.Domain;
    using BusinessApp.Test.Shared;

    public class EventConsumingRequestDecoratorTests
    {
        private readonly CancellationToken cancelToken;
        private readonly EventConsumingRequestDecorator<CommandStub, CompositeEventStub> sut;
        private readonly IRequestHandler<CommandStub, CompositeEventStub> inner;
        private readonly IEventPublisher publisher;
        private readonly IEventPublisherFactory publisherFactory;

        public EventConsumingRequestDecoratorTests()
        {
            cancelToken = A.Dummy<CancellationToken>();
            inner = A.Fake<IRequestHandler<CommandStub, CompositeEventStub>>();
            publisher = A.Fake<IEventPublisher>();
            publisherFactory = A.Fake<IEventPublisherFactory>();

            sut = new EventConsumingRequestDecorator<CommandStub, CompositeEventStub>(inner,
                publisherFactory);
        }

        public class Constructor : EventConsumingRequestDecoratorTests
        {
            public static IEnumerable<object[]> InvalidCtorArgs => new[]
            {
                new object[]
                {
                    null,
                    A.Dummy<IEventPublisherFactory>()
                },
                new object[]
                {
                    A.Dummy<IRequestHandler<CommandStub, CompositeEventStub>>(),
                    null
                }
            };

            [Theory, MemberData(nameof(InvalidCtorArgs))]
            public void InvalidCtorArgs_ExceptionThrown(IRequestHandler<CommandStub, CompositeEventStub> i,
                IEventPublisherFactory p)
            {
                /* Arrange */
                void shouldThrow() => new EventConsumingRequestDecorator<CommandStub, CompositeEventStub>(i, p);

                /* Act */
                var ex = Record.Exception(shouldThrow);

                /* Assert */
                Assert.IsType<BusinessAppException>(ex);
            }
        }

        public class HandleAsync : EventConsumingRequestDecoratorTests
        {
            private readonly CommandStub request;

            public HandleAsync()
            {
                request = A.Dummy<CommandStub>();

                A.CallTo(() => publisherFactory.Create(request)).Returns(publisher);
            }

            [Fact]
            public async Task Factory_CalledBeforeInnerHandle()
            {
                /* Arrange */
                var result = Result.Ok(new CompositeEventStub());
                A.CallTo(() => inner.HandleAsync(request, cancelToken))
                    .Returns(result);

                /* Act */
                var handlerResult = await sut.HandleAsync(request, cancelToken);

                /* Assert */
                A.CallTo(() => publisherFactory.Create(request)).MustHaveHappened()
                    .Then(A.CallTo(() => inner.HandleAsync(request, cancelToken)).MustHaveHappened());
            }

            [Fact]
            public async Task InnerHandlerHasError_EventsNotPublished()
            {
                /* Arrange */
                var result = Result.Error<CompositeEventStub>(A.Dummy<Exception>());
                A.CallTo(() => inner.HandleAsync(request, cancelToken))
                    .Returns(result);

                /* Act */
                var handlerResult = await sut.HandleAsync(request, cancelToken);

                /* Assert */
                Assert.Empty(Fake.GetCalls(publisher));
            }

            [Fact]
            public async Task InnerHandlerHasError_ThatErrorReturned()
            {
                /* Arrange */
                var result = Result.Error<CompositeEventStub>(A.Dummy<Exception>());
                A.CallTo(() => inner.HandleAsync(request, cancelToken))
                    .Returns(result);

                /* Act */
                var returnedResult = await sut.HandleAsync(request, cancelToken);

                /* Assert */
                Assert.Equal(result, returnedResult);
            }

            [Fact]
            public async Task EventCreatesMoreEvents_AllEventsPublished()
            {
                /* Arrange */
                var twoEvents = new[] { new DomainEventStub(), new DomainEventStub() };
                IEnumerable<IDomainEvent> noEvents = A.CollectionOfDummy<IDomainEvent>(0);
                var firstEventEvents = new[] { new DomainEventStub(), new DomainEventStub() };
                var @event = new CompositeEventStub
                {
                    Events = twoEvents.ToList()
                };
                A.CallTo(() => inner.HandleAsync(request, cancelToken))
                    .Returns(Result.Ok(@event));
                A.CallTo(() => publisher.PublishAsync(twoEvents.First(), cancelToken))
                    .Returns(Result.Ok<IEnumerable<IDomainEvent>>(firstEventEvents));
                A.CallTo(() => publisher.PublishAsync(twoEvents.Last(), cancelToken))
                    .Returns(Result.Ok(noEvents));
                A.CallTo(() => publisher.PublishAsync(firstEventEvents.First(), cancelToken))
                    .Returns(Result.Ok(noEvents));
                A.CallTo(() => publisher.PublishAsync(firstEventEvents.Last(), cancelToken))
                    .Returns(Result.Ok(noEvents));

                /* Act */
                var handlerResult = await sut.HandleAsync(request, cancelToken);

                /* Assert */
                Assert.Collection(handlerResult.Unwrap().Events,
                    e => Assert.Same(twoEvents.First(), e),
                    e => Assert.Same(twoEvents.Last(), e),
                    e => Assert.Same(firstEventEvents.First(), e),
                    e => Assert.Same(firstEventEvents.Last(), e));
            }

            [Fact]
            public async Task AtLeastOnEventErrored_ErrorReturned()
            {
                var exception = new Exception();
                var noEvents = A.CollectionOfDummy<DomainEventStub>(0);
                var twoEvents = new[] { new DomainEventStub(), new DomainEventStub() };
                var @event = new CompositeEventStub
                {
                    Events = twoEvents
                };
                A.CallTo(() => inner.HandleAsync(request, cancelToken))
                    .Returns(Result.Ok(@event));
                A.CallTo(() => publisher.PublishAsync(twoEvents.First(), cancelToken))
                    .Returns(Result.Ok<IEnumerable<IDomainEvent>>(noEvents));
                A.CallTo(() => publisher.PublishAsync(twoEvents.Last(), cancelToken))
                    .Returns(Result.Error<IEnumerable<IDomainEvent>>(exception));

                /* Act */
                var handlerResult = await sut.HandleAsync(request, cancelToken);

                /* Assert */
                Assert.Same(exception, handlerResult.UnwrapError());
            }

            private class E : IEntityId
            {
                public int Id { get; set; }
                public TypeCode GetTypeCode()
                {
                    throw new NotImplementedException();
                }
            }
        }
    }
}
