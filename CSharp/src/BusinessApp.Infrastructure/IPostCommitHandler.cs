using System;
using System.Threading;
using System.Threading.Tasks;
using BusinessApp.Kernel;

namespace BusinessApp.Infrastructure
{
    /// <summary>
    /// Service to handle request data after the request has been committed
    /// </summary>
    public interface IPostCommitHandler<in TRequest, TResponse>
        where TRequest : notnull
    {
        Task<Result<Unit, Exception>> HandleAsync(TRequest request, TResponse response,
            CancellationToken cancelToken);
    }
}
