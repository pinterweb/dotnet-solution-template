using System;
using System.Collections.Generic;
namespace BusinessApp.Domain
{
    using System.Threading;
    using System.Threading.Tasks;

    using HandlerResult = Result<IEnumerable<IDomainEvent>, Exception>;

    /// <summary>
    /// Interface to handle event logic
    /// </summary>
    public interface IEventHandler<TEvent> where TEvent : IDomainEvent
    {
        Task<HandlerResult> HandleAsync(TEvent @event, CancellationToken cancelToken);
    }
}
