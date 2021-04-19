using System;
using System.Threading;
using System.Threading.Tasks;
using BusinessApp.Domain;

namespace BusinessApp.App
{
    /// <summary>
    /// Null request handler for requests that have no business logic
    /// </summary>
    /// <remarks>
    /// Set this handler up to target command requests that have no business logic
    /// and simple save the incoming request. Most of the needed logic is already
    /// baked into the decorators
    /// </remarks>
    public class NoBusinessLogicRequestHandler<TRequest> :
        IRequestHandler<TRequest, TRequest>
        where TRequest : notnull
    {
        public Task<Result<TRequest, Exception>> HandleAsync(TRequest request,
            CancellationToken cancelToken)
        {
            return Task.FromResult(Result.Ok(request));
        }
    }
}
