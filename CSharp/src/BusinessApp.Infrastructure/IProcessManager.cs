using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using BusinessApp.Kernel;

namespace BusinessApp.Infrastructure
{
    public interface IProcessManager
    {
        Task<Result<Unit, Exception>> HandleNextAsync(IEnumerable<IDomainEvent> events,
            CancellationToken cancelToken);
    }
}
