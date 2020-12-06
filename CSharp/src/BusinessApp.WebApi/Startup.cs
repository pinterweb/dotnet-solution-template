namespace BusinessApp.WebApi
{
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
    using BusinessApp.Data;
//#if DEBUG
    using Microsoft.Extensions.Logging;
//#endif
#if winauth
    using Microsoft.AspNetCore.Server.HttpSys;
#endif

    public class Startup
    {
        private readonly Container container;
        private readonly BootstrapOptions options = new BootstrapOptions();

        public Startup(IConfiguration configuration, Container container)
        {
            this.container = Guard.Against.Null(container).Expect(nameof(container));
            container.Options.ResolveUnregisteredConcreteTypes = false;
            container.Options.DefaultLifestyle = Lifestyle.Scoped;
            container.Options.DefaultScopedLifestyle = new AsyncScopedLifestyle();

            options.LogFilePath = configuration.GetSection("Logging")
                .GetValue<string>("LogFilePath");
            options.DbConnectionString =
#if docker
                configuration.GetConnectionString("docker");
#else
                configuration.GetConnectionString("local");
#endif
            options.AppAssemblies = new[] { typeof(App.IQuery).Assembly };
            options.DataAssemblies = new[] { typeof(IQueryVisitor<>).Assembly };
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
//#if DEBUG
            services.AddLogging(configure => configure.AddConsole().AddDebug());
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
            services.AddAuthentication(HttpSysDefaults.AuthenticationScheme);
            services.AddAuthorization();
#endif
            services.AddSimpleInjector(container, options => options.AddAspNetCore());
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseSimpleInjector(container);
            // until ISerializer interface is changed
            app.Use(async (ctx, next) =>
            {
                var syncIOFeature = ctx.Features.Get<Microsoft.AspNetCore.Http.Features.IHttpBodyControlFeature>();
                if (syncIOFeature != null)
                {
                    syncIOFeature.AllowSynchronousIO = true;
                }
                await next();
            });

            app.UseMiddleware<HttpExceptionMiddleware>(container);

#if staticfiles
            app.UseDefaultFiles();
            app.UseStaticFiles();
#endif
            app.SetupEndpoints(container);

            WebApiBootstrapper.Bootstrap(app, env, container, options);
            container.Verify();

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
