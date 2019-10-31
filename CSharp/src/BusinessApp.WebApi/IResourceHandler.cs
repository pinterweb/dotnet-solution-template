namespace BusinessApp.WebApi
{
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Http;

    /// <summary>
    /// Handles a request to a resource, similar to a controller's action
    /// </summary>
    /// <typeparam name="TResponse"></typeparam>
    /// <remarks>
    /// Can be used if there is specific Http logic that need to be dealt
    /// with either before or after the application handler
    /// </remarks>
    public interface IResourceHandler<TRequest, TResponse>
    {
        Task<TResponse> HandleAsync(HttpContext context, CancellationToken cancellationToken);
    }
}
