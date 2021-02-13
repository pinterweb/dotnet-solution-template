namespace BusinessApp.Domain
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Manages domain events during the unit of work lifecycle
    /// </summary>
    public class EventUnitOfWork : IUnitOfWork
    {
        private readonly IEventPublisher eventPublisher;
        private readonly HashSet<IEventEmitter> emitters = new HashSet<IEventEmitter>();

        public EventUnitOfWork(IEventPublisher eventPublisher)
        {
            this.eventPublisher = eventPublisher.NotNull().Expect(nameof(eventPublisher));
        }

        public event EventHandler Committing = delegate { };
        public event EventHandler Committed = delegate { };

        public virtual void Add(AggregateRoot aggregate)
        {
            emitters.Add(aggregate);
        }

        public void Add(IDomainEvent @event)
        {
            emitters.Add(new EventEmitter(@event));
        }

        public virtual void Track(AggregateRoot aggregate)
        {
            emitters.Add(aggregate);
        }

        public virtual async Task CommitAsync(CancellationToken cancellationToken)
        {
            Volatile.Read(ref Committing).Invoke(this, EventArgs.Empty);

            do
            {
                var pending = emitters.Where(i => i.HasEvents()).ToArray();

                for (int a = 0; a < pending.Count(); a++)
                {
                    await eventPublisher.PublishAsync(pending[a], cancellationToken);
                }
            } while (emitters.Any(i => i.HasEvents()));

            Volatile.Read(ref Committed).Invoke(this, EventArgs.Empty);
        }

        public virtual void Remove(AggregateRoot aggregate)
        {
            emitters.Remove(aggregate);
        }

        public virtual Task RevertAsync(CancellationToken cancellationToken)
        {
            emitters.Clear();
            return Task.CompletedTask;
        }

        public TRoot Find<TRoot>(Func<TRoot, bool> filter) where TRoot : AggregateRoot
        {
            return emitters
                .Where(e => e.GetType() == typeof(TRoot))
                .Cast<TRoot>()
                .SingleOrDefault(filter);
        }

        private sealed class EventEmitter : IEventEmitter
        {
            private IDomainEvent e;

            public EventEmitter(IDomainEvent e)
            {
                this.e = e;
            }

            public bool HasEvents() => e != null;

            public IEnumerable<IDomainEvent> PublishEvents()
            {
                if (e != null)
                {
                    var emitted = e;
                    e = null;

                    yield return emitted;
                }
            }
        }
    }
}
