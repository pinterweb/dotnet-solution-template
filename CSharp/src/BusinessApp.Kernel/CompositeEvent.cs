using System.Collections;
using System.Collections.Generic;

namespace BusinessApp.Kernel
{
    /// <summary>
    /// Composite pattern to represent many events
    /// </summary>
    public class CompositeEvent : IEnumerable<IEvent>
    {
        private readonly IEnumerable<IEvent> events;

        public CompositeEvent() => events = new List<IEvent>();

        public CompositeEvent(IEnumerable<IEvent> events)
            => this.events = events.NotNull().Expect(nameof(events));

        public IEnumerator<IEvent> GetEnumerator() => events.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
