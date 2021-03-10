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

    public class EventConsumerCommandDecoratorTests
    {
        private readonly CancellationToken cancelToken;
        private readonly EventConsumingRequestDecorator<CommandStub, EventStreamStub> sut;
        private readonly IRequestHandler<CommandStub, EventStreamStub> inner;
        private readonly IEventPublisher publisher;

        public EventConsumerCommandDecoratorTests()
        {
            cancelToken = A.Dummy<CancellationToken>();
            inner = A.Fake<IRequestHandler<CommandStub, EventStreamStub>>();
            publisher = A.Fake<IEventPublisher>();

            sut = new EventConsumingRequestDecorator<CommandStub, EventStreamStub>(inner,
                publisher);
        }

        public class Constructor : EventConsumerCommandDecoratorTests
        {
            public static IEnumerable<object[]> InvalidCtorArgs => new[]
            {
                new object[]
                {
                    null,
                    A.Dummy<IEventPublisher>()
                },
                new object[]
                {
                    A.Dummy<IRequestHandler<CommandStub, EventStreamStub>>(),
                    null
                }
            };

            [Theory, MemberData(nameof(InvalidCtorArgs))]
            public void InvalidCtorArgs_ExceptionThrown(IRequestHandler<CommandStub, EventStreamStub> i,
                IEventPublisher p)
            {
                /* Arrange */
                void shouldThrow() => new EventConsumingRequestDecorator<CommandStub, EventStreamStub>(i, p);

                /* Act */
                var ex = Record.Exception(shouldThrow);

                /* Assert */
                Assert.IsType<BadStateException>(ex);
            }
        }

        public class HandleAsync : EventConsumerCommandDecoratorTests
        {
            private readonly CommandStub request;

            public HandleAsync()
            {
                request = A.Dummy<CommandStub>();
            }

            [Fact]
            public async Task InnerHandlerHasError_EventsNotPublished()
            {
                /* Arrange */
                var result = Result.Error<EventStreamStub>(A.Dummy<Exception>());
                A.CallTo(() => inner.HandleAsync(request, cancelToken))
                    .Returns(result);

                /* Act */
                var handlerResult = await sut.HandleAsync(request, cancelToken);

                /* Assert */
                A.CallTo(() => publisher.PublishAsync(A<IDomainEvent>._, A<CancellationToken>._))
                    .MustNotHaveHappened();
            }

            [Fact]
            public async Task InnerHandlerHasError_ThatErrorReturned()
            {
                /* Arrange */
                var result = Result.Error<EventStreamStub>(A.Dummy<Exception>());
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
                var twoEvents = A.CollectionOfDummy<IDomainEvent>(2);
                IEnumerable<IDomainEvent> noEvents = A.CollectionOfDummy<IDomainEvent>(0);
                IEnumerable<IDomainEvent> firstEventEvents = A.CollectionOfDummy<IDomainEvent>(2);
                var eventStream = new EventStreamStub
                {
                    Events = twoEvents.ToList()
                };
                A.CallTo(() => inner.HandleAsync(request, cancelToken))
                    .Returns(Result.Ok(eventStream));
                A.CallTo(() => publisher.PublishAsync(twoEvents.First(), cancelToken))
                    .Returns(Result.Ok(firstEventEvents));
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
                IEnumerable<IDomainEvent> noEvents = A.CollectionOfDummy<IDomainEvent>(0);
                var twoEvents = new EventStreamStub
                {
                    Events = A.CollectionOfDummy<IDomainEvent>(2)
                };
                A.CallTo(() => inner.HandleAsync(request, cancelToken))
                    .Returns(Result.Ok(twoEvents));
                A.CallTo(() => publisher.PublishAsync(twoEvents.Events.First(), cancelToken))
                    .Returns(Result.Ok(noEvents));
                A.CallTo(() => publisher.PublishAsync(twoEvents.Events.Last(), cancelToken))
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
