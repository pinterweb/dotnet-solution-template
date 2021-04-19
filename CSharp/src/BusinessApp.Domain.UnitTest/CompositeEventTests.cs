using Xunit;
using FakeItEasy;
using System.Linq;

namespace BusinessApp.Domain.UnitTest
{
    public class CompositeEventTests
    {
        public class Constructor : CompositeEventTests
        {
            [Fact]
            public void NoArgs_EmptyEvents()
            {
                /* Act */
                var events = new CompositeEvent();

                /* Assert */
                Assert.Empty(events);
            }

            [Fact]
            public void MissingEvents_ExceptionThrown()
            {
                /* Act */
                var ex = Record.Exception(() => new CompositeEvent(null));

                /* Assert */
                Assert.IsType<BusinessAppException>(ex);
            }
        }

        public class IEnumerableImpl : CompositeEventTests
        {
            [Fact]
            public void HasEvents_ReturnsThoseEvents()
            {
                /* Arrange */
                var events = A.CollectionOfDummy<IDomainEvent>(2);

                /* Act */
                var @event = new CompositeEvent(events);

                /* Assert */
                Assert.Collection(@event,
                    e => Assert.Same(events.First(), e),
                    e => Assert.Same(events.Last(), e)
                );
            }
        }
    }
}
