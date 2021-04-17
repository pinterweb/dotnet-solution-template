namespace BusinessApp.App
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using BusinessApp.Domain;

    public interface IProcessManager
    {
        Task<Result<Unit, Exception>> HandleNextAsync(IEnumerable<IDomainEvent> events,
            CancellationToken cancelToken);
    }
}
