using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace BusinessApp.Kernel
{
#pragma warning disable IDE0065
    using HandlerResult = Result<IEnumerable<IDomainEvent>, Exception>;
#pragma warning restore IDE0065

    /// <summary>
    /// Interface to handle event logic
    /// </summary>
    public interface IEventHandler<TEvent> where TEvent : notnull, IDomainEvent
    {
        Task<HandlerResult> HandleAsync(TEvent e, CancellationToken cancelToken);
    }
}
