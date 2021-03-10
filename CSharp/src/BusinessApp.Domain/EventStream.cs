namespace BusinessApp.Domain
{
    using System.Collections;
    using System.Collections.Generic;

    /// <summary>
    /// Handles command data in the <typeparam name="TCommand">TCommand</typeparam>
    /// </summary>
    /// <implementers>
    /// Logic in this pipeline can modify data
    /// </implementers>
    public class EventStream : IEnumerable<IDomainEvent>
    {
        private readonly EventEnumerator enumerator;

        public EventStream()
        {
            enumerator = new EventEnumerator(new Queue<IDomainEvent>());
        }

        public EventStream(IEnumerable<IDomainEvent> events)
        {
            events.NotNull().Expect(nameof(events));
            enumerator = new EventEnumerator(new Queue<IDomainEvent>(events));
        }

        public IEnumerator<IDomainEvent> GetEnumerator() => enumerator;
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        private class EventEnumerator : IEnumerator<IDomainEvent>
        {
            private readonly Queue<IDomainEvent> enumeratored;
            private Queue<IDomainEvent> queue;
            private IDomainEvent currentEvent;

            public EventEnumerator(Queue<IDomainEvent> queue)
            {
                this.queue = queue;
                enumeratored = new Queue<IDomainEvent>();
            }

            public IDomainEvent Current
            {
                get
                {
                    lock (queue)
                    {
                        return currentEvent;
                    }
                }
            }

            object IEnumerator.Current => Current;

            public void Dispose() {}

            public bool MoveNext()
            {
                lock (queue)
                {
                    if (queue.Count == 0)
                    {
                        currentEvent = null;
                        return false;
                    }

                    var e = queue.Dequeue();
                    enumeratored.Enqueue(e);
                    currentEvent = e;

                    return true;
                }
            }

            public void Reset()
            {
                lock (queue)
                {
                    queue.Clear();
                    queue = new Queue<IDomainEvent>(enumeratored);
                    enumeratored.Clear();
                }
            }
        }
    }
}
