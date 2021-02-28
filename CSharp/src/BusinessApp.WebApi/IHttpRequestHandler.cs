namespace BusinessApp.WebApi
{
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Http;
    using BusinessApp.Domain;
    using System;

    public interface IHttpRequestHandler<TRequest, TResponse>
    {
        Task<Result<TResponse, Exception>> HandleAsync(HttpContext context,
            CancellationToken cancelToken);
    }
}
