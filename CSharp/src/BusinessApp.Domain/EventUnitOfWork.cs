namespace BusinessApp.Domain
{
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
            this.eventPublisher = GuardAgainst.Null(eventPublisher, nameof(eventPublisher));
        }

        public void Add(IEventEmitter emitter)
        {
            emitters.Add(emitter);
        }

        public virtual void Add(AggregateRoot aggregate)
        {
            emitters.Add(aggregate);
        }

        public virtual async Task CommitAsync(CancellationToken cancellationToken)
        {
            var emitting = emitters.ToArray();
            emitters.Clear();

            for (int a = 0; a < emitting.Length; a++)
            {
                await eventPublisher.PublishAsync(emitting[a], cancellationToken);
            }
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
    }
}
