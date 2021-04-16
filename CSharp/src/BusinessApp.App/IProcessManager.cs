namespace BusinessApp.App
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using BusinessApp.Domain;

    public interface IProcessManager
    {
        Task<Result<Unit, Exception>> HandleNextAsync(IEventStream stream,
            CancellationToken cancelToken);
    }
}
