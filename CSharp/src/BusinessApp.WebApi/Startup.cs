using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
#if efcore
using Microsoft.EntityFrameworkCore;
#endif
using BusinessApp.Kernel;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SimpleInjector;
using SimpleInjector.Lifestyles;
#if efcore
using BusinessApp.Infrastructure.Persistence;
#endif
using Microsoft.Extensions.Logging;
using BusinessApp.CompositionRoot;
using BusinessApp.Infrastructure;
#if winauth
using Microsoft.AspNetCore.Authentication.Negotiate;
#endif

namespace BusinessApp.WebApi
{
    /// <summary>
    /// Startup class to hook up the webapi infrastructure
    /// </summary>
    public class Startup
    {
        private readonly Container container;
        private readonly RegistrationOptions options;

        public Startup(IConfiguration configuration, Container container, IWebHostEnvironment env)
        {
            this.container = container.NotNull().Expect(nameof(container));
            container.Options.ResolveUnregisteredConcreteTypes = false;
            container.Options.DefaultLifestyle = Lifestyle.Scoped;
            container.Options.DefaultScopedLifestyle = new AsyncScopedLifestyle();

            var connStr = configuration.GetConnectionString("Main");

            options = new RegistrationOptions(connStr, env.EnvironmentName)
            {
                RegistrationAssemblies = new[]
                {
                    typeof(IQuery).Assembly,
#if efcore
                    typeof(IQueryVisitor<>).Assembly,
#endif
                    typeof(IEventHandler<>).Assembly,
                    typeof(Startup).Assembly,
                    System.Reflection.Assembly.Load("BusinessApp.Api")
                }
            };
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddLocalization(options => options.ResourcesPath = "Resources");
//#if DEBUG
#if cors
            services.AddCors(options =>
            {
                options.AddDefaultPolicy(
                    builder =>
                    {
#if winauth
                        builder.WithOrigins("http://localhost:8081")
#else
                        builder.AllowAnyOrigin()
#endif
                            .AllowAnyMethod()
#if winauth
                            .AllowCredentials()
#endif
                            .AllowAnyHeader();
                    });
            });
#endif
//#endif
            services.AddRouting();
#if winauth
            services.AddAuthentication(NegotiateDefaults.AuthenticationScheme).AddNegotiate();
            services.AddAuthorization();
#endif
            services.AddSimpleInjector(container, options => options.AddAspNetCore());
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, ILoggerFactory loggerFactory)
        {
            app.UseSimpleInjector(container);
            app.UseRequestLocalization(opt =>
            {
                opt.ApplyCurrentCultureToResponseHeaders = true;
                opt.SupportedUICultures = new[]
                {
                    new System.Globalization.CultureInfo("en-US"),
                };
            });

#if staticfiles
            app.UseDefaultFiles();
            app.UseStaticFiles();
#endif
            app.UseRouting();

#if cors
//#if DEBUG
            app.UseCors();
//#endif
#endif

#if winauth
            app.UseAuthentication();
            app.UseAuthorization();
#endif
            Bootstrapper.RegisterServices(container, options, loggerFactory);

            container.Collection.Register<IStartupConfiguration>(
                new[] { typeof(Startup).Assembly },
                Lifestyle.Singleton);

            container.Verify();

//#if DEBUG
            app.SetupTestEndpoints(container);
//#endif
            app.SetupEndpoints(builder => builder.CreateRoutes(container));

            foreach (var startup in container.GetAllInstances<IStartupConfiguration>())
            {
                startup.Configure();
            }
        }
    }
}
