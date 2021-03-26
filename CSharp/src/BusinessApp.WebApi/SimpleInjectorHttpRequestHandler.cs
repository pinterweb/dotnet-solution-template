namespace BusinessApp.WebApi
{
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Http;
    using BusinessApp.Domain;
    using SimpleInjector;

    public class SimpleInjectorHttpRequestHandler : IHttpRequestHandler
    {
        private readonly Container container;

        public SimpleInjectorHttpRequestHandler(Container container)
        {
            this.container = container.NotNull().Expect(nameof(container));
        }

        public Task HandleAsync<T, R>(HttpContext context)
        {
            return container.GetInstance<IHttpRequestHandler<T, R>>()
                .HandleAsync(context, default);
        }
    }
}