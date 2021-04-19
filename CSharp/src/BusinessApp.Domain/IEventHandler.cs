using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace BusinessApp.Domain
{
    using HandlerResult = Result<IEnumerable<IDomainEvent>, Exception>;

    /// <summary>
    /// Interface to handle event logic
    /// </summary>
    public interface IEventHandler<TEvent> where TEvent : notnull, IDomainEvent
    {
        Task<HandlerResult> HandleAsync(TEvent @event, CancellationToken cancelToken);
    }
}
