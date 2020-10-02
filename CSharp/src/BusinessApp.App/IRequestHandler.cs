namespace BusinessApp.App
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using BusinessApp.Domain;

    public interface IRequestHandler<in TRequest, TResponse>
    {
        Task<Result<TResponse, IFormattable>> HandleAsync(TRequest request, CancellationToken cancellationToken);
    }
}
