namespace BusinessApp.Data.IntegrationTest
{
    using Xunit;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.EntityFrameworkCore;
    using BusinessApp.Test;
    using BusinessApp.Domain;

    [Collection(nameof(DatabaseCollection))]
    public class EventRepositoryTests : IDisposable
    {
        private EventRepository sut;
        private BusinessAppDbContext db;
        private DomainEventStub @event;

        public EventRepositoryTests(DatabaseFixture fixture)
        {
            db = fixture.DbContext;
            sut = new EventRepository(db);
            @event = new DomainEventStub();
        }

        public void Dispose() => db.Entry(@event).State = EntityState.Detached;

        public class Constructor : EventRepositoryTests
        {
            public Constructor(DatabaseFixture fixture) : base(fixture) {}

            public static IEnumerable<object[]> InvalidCtorArgs
            {
                get
                {
                    return new []
                    {
                        new object[] { null },
                    };
                }
            }

            [Theory, MemberData(nameof(InvalidCtorArgs))]
            public void InvalidCtorArgs_ExceptionThrown(BusinessAppDbContext db)
            {
                /* Arrange */
                void shouldThrow() => new EventRepository(db);

                /* Act */
                var ex = Record.Exception((Action)shouldThrow);

                /* Assert */
                Assert.IsType<BadStateException>(ex);
            }
        }

        public class Add : EventRepositoryTests
        {
            public Add(DatabaseFixture fixture) : base(fixture) {}

            [Fact]
            public void WithNullEvent_ExceptionThrown()
            {
                /* Arrange */
                Action add = () => sut.Add(null);

                /* Act */
                var exception = Record.Exception(add);

                /* Assert */
                Assert.IsType<BadStateException>(exception);
            }

            [Fact]
            public void Add_Event_RegisteredWithDb()
            {
                sut.Add(@event);

                var state = db
                    .ChangeTracker
                    .Entries()
                    .Single()
                    .State;
                Assert.Equal(EntityState.Added, state);
            }
        }
    }
}
