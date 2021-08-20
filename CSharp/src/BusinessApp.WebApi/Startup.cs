using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
#if DEBUG
using Microsoft.EntityFrameworkCore;
#elif efcore
using Microsoft.EntityFrameworkCore;
#endif
using BusinessApp.Kernel;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SimpleInjector;
using SimpleInjector.Lifestyles;
#if DEBUG
using BusinessApp.Infrastructure.Persistence;
#elif efcore
using BusinessApp.Infrastructure.Persistence;
#endif
using Microsoft.Extensions.Logging;
using BusinessApp.CompositionRoot;
using BusinessApp.Infrastructure;
#if winauth
using Microsoft.AspNetCore.Authentication.Negotiate;
#elif winauth
using Microsoft.AspNetCore.Authentication.Negotiate;
#endif

namespace BusinessApp.WebApi
{
    /// <summary>
    /// Startup class to hook up the webapi infrastructure
    /// </summary>
    public class Startup
    {
        private readonly IConfiguration configuration;
        private readonly IWebHostEnvironment env;
        private readonly Container container;

        public static Container ConfigureContainer()
        {
            var container = new Container();
            container.Options.ResolveUnregisteredConcreteTypes = false;
            container.Options.DefaultLifestyle = Lifestyle.Scoped;
            container.Options.DefaultScopedLifestyle = new AsyncScopedLifestyle();

            return container;
        }

        public Startup(IConfiguration configuration, IWebHostEnvironment env)
        {
            container = ConfigureContainer();
            this.configuration = configuration;
            this.env = env;
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
            var options = CreateRegistrationOptions();

            Bootstrapper.RegisterServices(container, options, loggerFactory, configuration);

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

        private RegistrationOptions CreateRegistrationOptions()
        {
            var connStr = configuration.GetConnectionString("Main");

            return new(connStr, env.EnvironmentName)
            {
                RegistrationAssemblies = new[]
                {
                    typeof(IQuery).Assembly, // infrastructure
#if DEBUG
                    typeof(IQueryVisitor<>).Assembly,
#elif efcore
                    typeof(IQueryVisitor<>).Assembly, // persistence
#endif
                    typeof(ValueKind).Assembly, // Kernel
                    typeof(Startup).Assembly, // webapi
                    System.Reflection.Assembly.Load("BusinessApp.Api")
                }
            };
        }
    }
}
