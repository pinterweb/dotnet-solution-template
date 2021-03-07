namespace BusinessApp.Data.IntegrationTest
{
    using Xunit;
    using System;
    using System.Collections.Generic;
    using BusinessApp.Domain;
    using FakeItEasy;
    using System.Security.Principal;
    using System.Linq;
    using BusinessApp.Test.Shared;

    [Collection(nameof(DatabaseCollection))]
    public class EventRepositoryTests
    {
        private EventRepository sut;
        private BusinessAppDbContext db;
        private readonly IPrincipal user;
        private readonly IEntityIdFactory<EventId> idFactory;

        public EventRepositoryTests(DatabaseFixture fixture)
        {
            user = A.Fake<IPrincipal>();
            idFactory = A.Fake<IEntityIdFactory<EventId>>();
            Setup(fixture);
        }

        public void Setup(DatabaseFixture fixture)
        {
            db = A.Fake<BusinessAppDbContext>();
            sut = new EventRepository(db, user, idFactory);

            A.CallTo(() => user.Identity.Name).Returns("f");
        }

        public class Constructor : EventRepositoryTests
        {
            public Constructor(DatabaseFixture f) : base(f)
            {}

            public static IEnumerable<object[]> InvalidCtorArgs
            {
                get
                {
                    return new []
                    {
                        new object[]
                        {
                            null,
                            A.Dummy<IPrincipal>(),
                            A.Dummy<IEntityIdFactory<EventId>>()
                        },
                        new object[]
                        {
                            A.Dummy<BusinessAppDbContext>(),
                            null,
                            A.Dummy<IEntityIdFactory<EventId>>()
                        },
                        new object[]
                        {
                            A.Dummy<BusinessAppDbContext>(),
                            A.Dummy<IPrincipal>(),
                            null
                        },
                    };
                }
            }

            [Theory, MemberData(nameof(InvalidCtorArgs))]
            public void InvalidCtorArgs_ExceptionThrown(BusinessAppDbContext db, IPrincipal p,
                IEntityIdFactory<EventId> i)
            {
                /* Arrange */
                void shouldThrow() => new EventRepository(db, p, i);

                /* Act */
                var ex = Record.Exception((Action)shouldThrow);

                /* Assert */
                Assert.IsType<BadStateException>(ex);
            }
        }

        public class Add : EventRepositoryTests
        {
            private readonly DatabaseFixture fixture;

            public Add(DatabaseFixture f) : base(f)
            {
                fixture = f;
            }

            [Fact]
            public void NullEventArg_ExceptionThrown()
            {
                /* Arrange */
                IDomainEvent @event = null;
                Action add = () => sut.Add(@event);

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
                A.CallTo(() => db.Add(A<EventMetadata>._))
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
                Setup(fixture);
                A.CallTo(() => db.Add(A<EventMetadata>._))
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
            public void EventDisplayText_SetOnEventMetadata()
            {
                /* Arrange */
                var @event = A.Fake<IDomainEvent>();
                EventMetadata metadata = null;
                A.CallTo(() => db.Add(A<EventMetadata>._))
                    .Invokes(ctx => metadata = ctx.GetArgument<EventMetadata>(0));
                A.CallTo(() => @event.ToString()).Returns("lorem");

                /* Act */
                sut.Add(@event);

                /* Assert */
                Assert.Equal("lorem", metadata.EventDisplayText);
            }

            [Fact]
            public void EventOccurredUtc_SetOnEventMetadata()
            {
                /* Arrange */
                var now = DateTime.Now;
                var @event = A.Fake<IDomainEvent>();
                EventMetadata metadata = null;
                A.CallTo(() => db.Add(A<EventMetadata>._))
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
                A.CallTo(() => db.Add(A<EventMetadata>._))
                    .Invokes(ctx => metadata = ctx.GetArgument<EventMetadata>(0));

                /* Act */
                sut.Add(A.Dummy<IDomainEvent>());

                /* Assert */
                Assert.Equal("foobar", metadata.EventCreator);
            }

            [Fact]
            public void IDomainEventArgument_AddedToDbToo()
            {
                /* Arrange */
                var @event = A.Dummy<IDomainEvent>();

                /* Act */
                sut.Add(@event);

                /* Assert */
                A.CallTo(() => db.Add((object)@event)).MustHaveHappenedOnceExactly();
            }
        }
    }
}
