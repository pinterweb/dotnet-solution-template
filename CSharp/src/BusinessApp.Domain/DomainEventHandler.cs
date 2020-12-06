namespace BusinessApp.Domain
{
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Handles events emitted from the domain
    /// </summary>
    public class DomainEventHandler<TEvent> : IEventHandler<TEvent>
        where TEvent : IDomainEvent
    {
        private readonly IEventRepository repo;

        public DomainEventHandler(IEventRepository repo)
        {
            this.repo = repo.NotNull().Expect(nameof(repo));
        }

        public Task HandleAsync(TEvent @event, CancellationToken cancellationToken)
        {
            repo.Add(@event);

            return Task.CompletedTask;
        }
    }
}
