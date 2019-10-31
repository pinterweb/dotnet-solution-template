namespace BusinessApp.WebApi
{
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Http;
    using BusinessApp.Domain;

    /// <summary>
    /// Similar to the <see cref="App.EntityNotFoundQueryDecorator{TQuery, TResult}" />
    /// but is a safeguard to throw if the resource returned is null, which is an invalid response.
    /// </summary>
    public class ResourceNotFoundRequestDecorator<TRequest, TResponse> : IResourceHandler<TRequest, TResponse>
    {
        private readonly IResourceHandler<TRequest, TResponse> decorated;

        public ResourceNotFoundRequestDecorator(IResourceHandler<TRequest, TResponse> decorated)
        {
            this.decorated = GuardAgainst.Null(decorated, nameof(decorated));
        }

        public async Task<TResponse> HandleAsync(HttpContext context, CancellationToken cancellationToken)
        {
            var resource = await decorated.HandleAsync(context, cancellationToken);

            if (resource == null)
            {
                throw new ResourceNotFoundException();
            }

            return resource;
        }
    }
}
