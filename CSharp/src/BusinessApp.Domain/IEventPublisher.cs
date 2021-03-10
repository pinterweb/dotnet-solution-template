using System.Collections.Generic;

namespace BusinessApp.Domain
{
    using System.Threading;
    using System.Threading.Tasks;

    using EventsResult = Result<IEnumerable<IDomainEvent>, System.Exception>;

    /// <summary>
    /// Interface to publish events
    /// </summary>
    public interface IEventPublisher
    {
        Task<EventsResult> PublishAsync(IDomainEvent @event, CancellationToken cancelToken);
    }
}
