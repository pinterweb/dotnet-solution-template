namespace BusinessApp.Data.IntegrationTest
{
    using Xunit;
    using System;
    using System.Collections.Generic;
    using BusinessApp.Domain;
    using BusinessApp.App;
    using FakeItEasy;
    using System.Security.Principal;
    using System.Linq;

    public class EventRepositoryTests
    {
        private EventRepository sut;
        private readonly IUnitOfWork uow;
        private readonly IPrincipal user;

        public EventRepositoryTests()
        {
            uow = A.Fake<IUnitOfWork>();
            user = A.Fake<IPrincipal>();
            sut = new EventRepository(uow, user);

            A.CallTo(() => user.Identity.Name).Returns("f");
        }

        public class Constructor : EventRepositoryTests
        {
            public static IEnumerable<object[]> InvalidCtorArgs
            {
                get
                {
                    return new []
                    {
                        new object[]
                        {
                            null,
                            A.Dummy<IPrincipal>()
                        },
                        new object[]
                        {
                            A.Dummy<IUnitOfWork>(),
                            null
                        },
                    };
                }
            }

            [Theory, MemberData(nameof(InvalidCtorArgs))]
            public void InvalidCtorArgs_ExceptionThrown(IUnitOfWork db, IPrincipal p)
            {
                /* Arrange */
                void shouldThrow() => new EventRepository(db, p);

                /* Act */
                var ex = Record.Exception((Action)shouldThrow);

                /* Assert */
                Assert.IsType<BadStateException>(ex);
            }
        }

        public class Add : EventRepositoryTests
        {
            [Fact]
            public void WithNullEventArg_ExceptionThrown()
            {
                /* Arrange */
                Action add = () => sut.Add(null);

                /* Act */
                var exception = Record.Exception(add);

                /* Assert */
                Assert.IsType<BadStateException>(exception);
            }

            [Fact]
            public void IDomainEventEventIdProperty_SharedWithEventMetadata()
            {
                /* Arrange */
                EventMetadata @event = null;
                var eventInput = A.Fake<IDomainEvent>();
                A.CallTo(() => uow.Add(A<EventMetadata>._))
                    .Invokes(ctx => @event = ctx.GetArgument<EventMetadata>(0));

                /* Act */
                sut.Add(eventInput);

                /* Assert */
                Assert.Same(@event.Id, eventInput.Id);
            }

            [Fact]
            public void EventCorrelationId_SetForAllAddedEvents()
            {
                /* Arrange */
                var @events = new List<EventMetadata>();
                sut = new EventRepository(uow, user);
                A.CallTo(() => uow.Add(A<EventMetadata>._))
                    .Invokes(ctx => events.Add(ctx.GetArgument<EventMetadata>(0)));

                /* Act */
                sut.Add(A.Dummy<IDomainEvent>());
                sut.Add(A.Dummy<IDomainEvent>());

                /* Assert */
                Assert.All(
                    @events,
                    e => Assert.Same(@events.First().CorrelationId, e.CorrelationId)
                );
            }

            [Fact]
            public void EventDisplayText_SerFromOriginalEventIFormattableToStringCall()
            {
                /* Arrange */
                var @event = A.Fake<IDomainEvent>();
                EventMetadata metadata = null;
                A.CallTo(() => uow.Add(A<EventMetadata>._))
                    .Invokes(ctx => metadata = ctx.GetArgument<EventMetadata>(0));
                A.CallTo(() => @event.ToString("G", null)).Returns("lorem");

                /* Act */
                sut.Add(@event);

                /* Assert */
                Assert.Equal("lorem", metadata.EventDisplayText);
            }

            [Fact]
            public void EventOccurredUtc_SetFromOriginalEventOccurredUtc()
            {
                /* Arrange */
                var now = DateTime.Now;
                var @event = A.Fake<IDomainEvent>();
                EventMetadata metadata = null;
                A.CallTo(() => uow.Add(A<EventMetadata>._))
                    .Invokes(ctx => metadata = ctx.GetArgument<EventMetadata>(0));
                A.CallTo(() => @event.OccurredUtc).Returns(now);

                /* Act */
                sut.Add(@event);

                /* Assert */
                Assert.Equal(now, metadata.OccurredUtc);
            }

            [Fact]
            public void DomainEventCreator_SetFromIPrincipal()
            {
                /* Arrange */
                A.CallTo(() => user.Identity.Name).Returns("foobar");
                EventMetadata metadata = null;
                A.CallTo(() => uow.Add(A<EventMetadata>._))
                    .Invokes(ctx => metadata = ctx.GetArgument<EventMetadata>(0));

                /* Act */
                sut.Add(A.Dummy<IDomainEvent>());

                /* Assert */
                Assert.Equal("foobar", metadata.EventCreator);
            }

            [Fact]
            public void IDomainEventInputArg_AddedToIUnitOfWork()
            {
                /* Arrange */
                var @event = A.Dummy<IDomainEvent>();

                /* Act */
                sut.Add(@event);

                /* Assert */
                A.CallTo(() => uow.Add(@event)).MustHaveHappenedOnceExactly();
            }
        }
    }
}
