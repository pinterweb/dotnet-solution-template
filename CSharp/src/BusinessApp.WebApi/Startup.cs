using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
#if efcore
using Microsoft.EntityFrameworkCore;
#endif
using BusinessApp.Domain;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SimpleInjector;
using SimpleInjector.Lifestyles;
using System;
using BusinessApp.Infrastructure.EntityFramework;
using Microsoft.Extensions.Logging;
using BusinessApp.CompositionRoot;
#if winauth
using Microsoft.AspNetCore.Authentication.Negotiate;
#endif

namespace BusinessApp.WebApi
{
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

            var loggingPath = configuration.GetSection("Logging")
                .GetValue<string>("LogFilePath");
#if docker
            var connStr = configuration.GetConnectionString("docker");
#else
            var connStr = configuration.GetConnectionString("local");
#endif
            options = new RegistrationOptions(connStr, env.EnvironmentName)
            {
                RegistrationAssemblies = new[]
                {
                    typeof(Infrastructure.IQuery).Assembly,
                    typeof(IQueryVisitor<>).Assembly,
                    typeof(IEventHandler<>).Assembly,
                    typeof(Startup).Assembly
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
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env,
            ILoggerFactory loggerFactory)
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
            CompositionRoot.Bootstrapper.RegisterServices(container, options, loggerFactory);

            container.Verify();

            app.SetupEndpoints(container);

            if (env.EnvironmentName.Equals("Development", StringComparison.OrdinalIgnoreCase))
            {
                app.UseDeveloperExceptionPage();
#if efcore
                using (AsyncScopedLifestyle.BeginScope(container))
                {
                    var db = container.GetInstance<BusinessAppDbContext>();
                    db.Database.Migrate();
                }
#endif
            }
        }
    }
}
