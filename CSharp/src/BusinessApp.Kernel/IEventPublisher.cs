using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace BusinessApp.Kernel
{
#pragma warning disable IDE0065
    using EventsResult = Result<IEnumerable<IEvent>, System.Exception>;
#pragma warning restore IDE0065

    /// <summary>
    /// Interface to publish events
    /// </summary>
    public interface IEventPublisher
    {
        Task<EventsResult> PublishAsync<T>(T e, CancellationToken cancelToken)
            where T : notnull, IEvent;
    }
}
