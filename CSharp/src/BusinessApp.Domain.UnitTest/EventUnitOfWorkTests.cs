namespace BusinessApp.Domain.UnitTest
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using FakeItEasy;
    using BusinessApp.Domain;
    using Xunit;
    using System.Threading;

    public class EventUnitOfWorkTests
    {
        private readonly EventUnitOfWork sut;
        private readonly IEventPublisher publisher;
        private readonly CancellationToken token;

        public EventUnitOfWorkTests()
        {
            token = A.Dummy<CancellationToken>();
            publisher = A.Fake<IEventPublisher>();
            sut = new EventUnitOfWork(publisher);
        }

        public class Constructor : EventUnitOfWorkTests
        {
            public static IEnumerable<object[]> InvalidCtorArgs
            {
                get
                {
                    return new[]
                    {
                        new object[] { null },
                    };
                }
            }

            [Theory, MemberData(nameof(InvalidCtorArgs))]
            public void InvalidArgs_ExceptionThrown(IEventPublisher p)
            {
                /* Arrange */
                void shouldThrow() => new EventUnitOfWork(p);

                /* Act */
                var ex = Record.Exception((Action)shouldThrow);

                /* Assert */
                Assert.IsType<ArgumentNullException>(ex);
            }
        }

        public class CommitAsync : EventUnitOfWorkTests
        {
            [Fact]
            public async Task WithNewAggregateRoot_EventsPublishBeforeOnCommit()
            {
                /* Arrange */
                var aggregate = new AggregateRootFake();
                aggregate.AddEvent();
                sut.Add(aggregate);
                A.CallTo(() => publisher.PublishAsync(aggregate, token))
                    .Invokes(ctx =>
                    {
                        aggregate.ClearEvents();
                    });

                /* Act */
                await sut.CommitAsync(token);

                /* Assert */
                A.CallTo(() => publisher.PublishAsync(aggregate, token))
                    .MustHaveHappenedOnceExactly();
            }

            [Fact]
            public async Task WithNewEventEmitter_EventsPublishBeforeOnCommit()
            {
                /* Arrange */
                var emitter = A.Fake<IEventEmitter>();
                A.CallTo(() => emitter.HasEvents()).Returns(true).Once();
                sut.Add(emitter);

                /* Act */
                await sut.CommitAsync(token);

                /* Assert */
                A.CallTo(() => publisher.PublishAsync(emitter, token))
                    .MustHaveHappenedOnceExactly();
            }

            [Fact]
            public async Task WithAggregate_EventsPublishWhileExists()
            {
                /* Arrange */
                var aggregate = new AggregateRootFake();
                int calls = 0;
                aggregate.AddEvent();
                sut.Add(aggregate);
                A.CallTo(() => publisher.PublishAsync(aggregate, token))
                    .Invokes(ctx =>
                    {
                        if (calls != 0)
                        {
                            aggregate.ClearEvents();
                        }

                        calls++;
                    });

                /* Act */
                await sut.CommitAsync(token);

                /* Assert */
                A.CallTo(() => publisher.PublishAsync(aggregate, token))
                    .MustHaveHappenedTwiceExactly();
            }

            [Fact]
            public async Task WithEventEmitter_EventsPublishWhileExists()
            {
                /* Arrange */
                var emitter = A.Fake<IEventEmitter>();
                A.CallTo(() => emitter.HasEvents()).Returns(true).Twice();
                sut.Add(emitter);

                /* Act */
                await sut.CommitAsync(token);

                /* Assert */
                A.CallTo(() => publisher.PublishAsync(emitter, token))
                    .MustHaveHappenedOnceExactly();
            }
        }

        public class Remove : EventUnitOfWorkTests
        {
            [Fact]
            public async Task WithExistingEmitter_RemovesIt()
            {
                /* Arrange */
                var aggregate = new AggregateRootFake();
                aggregate.AddEvent();
                sut.Add(aggregate);

                /* Act */
                sut.Remove(aggregate);

                /* Assert */
                await sut.CommitAsync(token);
                A.CallTo(() => publisher.PublishAsync(A<AggregateRoot>._, A<CancellationToken>._))
                    .MustNotHaveHappened();
            }
        }

        public class RevertAsync : EventUnitOfWorkTests
        {
            [Fact]
            public async Task WithPendingEmitters_RemovesAll()
            {
                /* Arrange */
                var first = new AggregateRootFake();
                var second = new AggregateRootFake();
                first.AddEvent();
                second.AddEvent();
                sut.Add(first);
                sut.Add(second);

                /* Act */
                await sut.RevertAsync(token);

                /* Assert */
                await sut.CommitAsync(token);
                A.CallTo(() => publisher.PublishAsync(A<AggregateRoot>._, A<CancellationToken>._))
                    .MustNotHaveHappened();
            }
        }
    }
}
