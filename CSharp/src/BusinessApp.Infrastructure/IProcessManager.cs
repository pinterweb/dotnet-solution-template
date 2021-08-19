using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using BusinessApp.Kernel;

namespace BusinessApp.Infrastructure
{
    /// <summary>
    /// Handles of multiple events, normally emitting new commands to facilitate
    /// automation
    /// </summary>
    public interface IProcessManager
    {
        Task<Result<Unit, Exception>> HandleNextAsync(IEnumerable<IEvent> events,
            CancellationToken cancelToken);
    }
}
