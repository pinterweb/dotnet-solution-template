using System;
using System.Threading;
using System.Threading.Tasks;
using BusinessApp.Kernel;

namespace BusinessApp.Infrastructure
{
    /// <summary>
    /// Service to handle request data and produce a response
    /// </summary>
    public interface IRequestHandler<in TRequest, TResponse>
        where TRequest : notnull
    {
        Task<Result<TResponse, Exception>> HandleAsync(TRequest request,
            CancellationToken cancelToken);
    }
}
