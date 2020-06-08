namespace BusinessApp.Data.IntegrationTest
{
    using Xunit;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.EntityFrameworkCore;

    public class EventRepositoryTests
    {
        private EventRepository sut;
        private BusinessAppDbContext db;

        public EventRepositoryTests()
        {
            db = BusinessAppDbContextDummyFactory.Create();
            sut = new EventRepository(db);
        }

        public class Constructor : EventRepositoryTests
        {
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
                Assert.IsType<ArgumentNullException>(ex);
            }

        }

        public class Add : EventRepositoryTests
        {
            [Fact]
            public void WithNullEvent_ExceptionThrown()
            {
                /* Arrange */
                Action add = () => sut.Add(null);

                /* Act */
                var exception = Record.Exception(add);

                /* Assert */
                Assert.IsType<ArgumentNullException>(exception);
            }

            [Fact]
            public void Add_Event_RegisteredWithDb()
            {
                var @event = new DummyEventModel();

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
