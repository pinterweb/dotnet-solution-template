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

        public Startup(IConfiguration configuration, IWebHostEnvironment env)
        {
            container.Options.ResolveUnregisteredConcreteTypes = false;
            Configuration = configuration;
            container.Options.DefaultLifestyle = Lifestyle.Scoped;
            container.Options.DefaultScopedLifestyle = new AsyncScopedLifestyle();
            SetupEnvironment(env);
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
//#if DEBUG
            services.AddLogging(configure => configure.AddConsole().AddDebug());
//#endif
            services.AddRouting();
#if winauth
            services.AddAuthentication(HttpSysDefaults.AuthenticationScheme);
#endif
            services.AddAuthorization();
            services.AddSimpleInjector(container, options => options.AddAspNetCore());
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseSimpleInjector(container);
            app.UseMiddleware<HttpRequestExceptionMiddleware>(container);

            app.SetupEndpoints(container);

#if winauth
            app.UseAuthentication();
#endif
            app.UseAuthorization();

            WebApiBootstrapper.Bootstrap(app, env, container);
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

        private static void SetupEnvironment(IWebHostEnvironment env)
        {
            // it is not a requirement and IO will throw if not found
            try
            {
                using (var stream = System.IO.File.OpenRead(env.ContentRootPath + "./.env"))
                {
                    DotNetEnv.Env.Load(stream);
                }
            }
            catch(Exception e)
            {
                Console.WriteLine("Did not load .env because: " + e.Message);
            }
        }
    }
}
