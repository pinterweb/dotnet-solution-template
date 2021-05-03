using System.Collections;
using System.Collections.Generic;

namespace BusinessApp.Kernel
{
    /// <summary>
    /// Composite pattern to represent many events
    /// </summary>
    public class CompositeEvent : IEnumerable<IDomainEvent>
    {
        private readonly IEnumerable<IDomainEvent> events;

        public CompositeEvent() => events = new List<IDomainEvent>();

        public CompositeEvent(IEnumerable<IDomainEvent> events)
            => this.events = events.NotNull().Expect(nameof(events));

        public IEnumerator<IDomainEvent> GetEnumerator() => events.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
