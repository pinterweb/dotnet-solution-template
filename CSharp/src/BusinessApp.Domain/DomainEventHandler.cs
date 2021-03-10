using System;
using System.Collections.Generic;

namespace BusinessApp.Domain
{
    using System.Threading;
    using System.Threading.Tasks;

    using HandlerResult = Result<IEnumerable<IDomainEvent>, Exception>;

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

        public Task<HandlerResult> HandleAsync(TEvent @event, CancellationToken cancelToken)
        {
            repo.Add(@event);

            return Task.FromResult(HandlerResult.Ok(new IDomainEvent[0]));
        }
    }
}
