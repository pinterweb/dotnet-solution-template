using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using BusinessApp.Kernel;
using SimpleInjector;

namespace BusinessApp.WebApi
{
    /// <summary>
    /// Uses SimpleInjector to run the <see cref="IHttpRequestHandler{TRequest, TResponse}" />
    /// </summary>
    public class SimpleInjectorHttpRequestHandler : IHttpRequestHandler
    {
        private readonly Container container;

        public SimpleInjectorHttpRequestHandler(Container container)
            => this.container = container.NotNull().Expect(nameof(container));

        public Task HandleAsync<TRequest, TResponse>(HttpContext context) where TRequest : notnull
            => container.GetInstance<IHttpRequestHandler<TRequest, TResponse>>()
                .HandleAsync(context, default);
    }
}
