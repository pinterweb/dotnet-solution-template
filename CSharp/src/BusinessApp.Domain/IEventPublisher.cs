using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace BusinessApp.Domain
{
    using EventsResult = Result<IEnumerable<IDomainEvent>, System.Exception>;

    /// <summary>
    /// Interface to publish events
    /// </summary>
    public interface IEventPublisher
    {
        Task<EventsResult> PublishAsync<T>(T @event, CancellationToken cancelToken)
            where T : notnull, IDomainEvent;
    }
}
