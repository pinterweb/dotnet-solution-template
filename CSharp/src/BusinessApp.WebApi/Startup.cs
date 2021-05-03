using BusinessApp.Kernel;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SimpleInjector;

namespace BusinessApp.WebApi
{
    /// <summary>
    /// The main startup class for your webapi
    /// </summary>
    public class Startup
    {
        private readonly Container container;
        private readonly Infrastructure.WebApi.Startup inner;

        public Startup(IConfiguration configuration, Container container, IWebHostEnvironment env)
        {
            this.container = container.NotNull().Expect(nameof(container));
            inner = new Infrastructure.WebApi.Startup(configuration, container, env);
        }

        public void ConfigureServices(IServiceCollection services) => inner.ConfigureServices(services);

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env,
            ILoggerFactory loggerFactory)
                => inner.Configure(app,
                    env,
                    loggerFactory,
                    builder => builder.CreateRoutes(container));
    }
}
