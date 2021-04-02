namespace BusinessApp.App
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using BusinessApp.Domain;

    public interface IRequestHandler<in TRequest, TResponse>
        where TRequest : notnull
    {
        Task<Result<TResponse, Exception>> HandleAsync(TRequest request,
            CancellationToken cancelToken);
    }
}
