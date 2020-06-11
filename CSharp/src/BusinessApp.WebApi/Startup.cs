namespace BusinessApp.WebApi
{
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;
#if efcore
    using Microsoft.EntityFrameworkCore;
#endif
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using SimpleInjector;
    using SimpleInjector.Lifestyles;
    using System;
#if efcore
    using BusinessApp.Data;
#endif
//#if DEBUG
    using Microsoft.Extensions.Logging;
//#endif
#if winauth
    using Microsoft.AspNetCore.Server.HttpSys;
#endif

    public class Startup
    {
        private readonly Container container = new Container();
        private readonly BootstrapOptions options = new BootstrapOptions();

        public Startup(IConfiguration configuration, IWebHostEnvironment env)
        {
            container.Options.ResolveUnregisteredConcreteTypes = false;
            container.Options.DefaultLifestyle = Lifestyle.Scoped;
            container.Options.DefaultScopedLifestyle = new AsyncScopedLifestyle();

            options.LogFilePath = configuration.GetSection("Logginc")
                .GetValue<string>("LogFilePath");
            options.WriteConnectionString = options.ReadConnectionString =
                configuration.GetConnectionString("Main");
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
//#if DEBUG
            services.AddLogging(configure => configure.AddConsole().AddDebug());
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
            app.UseMiddleware<HttpRequestExceptionMiddleware>(container);

            app.SetupEndpoints(container);

            WebApiBootstrapper.Bootstrap(app, env, container, options);
            container.Verify();

            if (env.EnvironmentName.Equals("Development", StringComparison.OrdinalIgnoreCase))
            {
                app.UseDeveloperExceptionPage();
#if efcore
                using (AsyncScopedLifestyle.BeginScope(container))
                {
                    var db = container.GetInstance<BusinessAppReadOnlyDbContext>();
                    db.Database.Migrate();
                }
#endif
            }
        }
    }
}
