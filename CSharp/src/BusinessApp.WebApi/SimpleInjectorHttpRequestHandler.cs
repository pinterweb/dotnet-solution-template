using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using BusinessApp.Domain;
using SimpleInjector;

namespace BusinessApp.WebApi
{
    public class SimpleInjectorHttpRequestHandler : IHttpRequestHandler
    {
        private readonly Container container;

        public SimpleInjectorHttpRequestHandler(Container container)
        {
            this.container = container.NotNull().Expect(nameof(container));
        }

        public Task HandleAsync<T, R>(HttpContext context) where T : notnull
        {
            return container.GetInstance<IHttpRequestHandler<T, R>>()
                .HandleAsync(context, default);
        }
    }
}
