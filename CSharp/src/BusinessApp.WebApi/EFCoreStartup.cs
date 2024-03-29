using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using BusinessApp.Kernel;
using SimpleInjector;
using SimpleInjector.Lifestyles;
using System;
using BusinessApp.Infrastructure.Persistence;

namespace BusinessApp.WebApi
{
    /// <summary>
    /// Entity Framework Core startup logic
    /// </summary>
    public class EFCoreStartup : IStartupConfiguration
    {
        private readonly Container container;
        private readonly IWebHostEnvironment env;

        public EFCoreStartup(Container container, IWebHostEnvironment env)
        {
            this.container = container.NotNull().Expect(nameof(container));
            this.env = env.NotNull().Expect(nameof(env));
        }

        public void Configure()
        {
            if (env.EnvironmentName.Equals("Development", StringComparison.OrdinalIgnoreCase))
            {
                using (AsyncScopedLifestyle.BeginScope(container))
                {
                    using var db = container.GetInstance<BusinessAppDbContext>();
                    db.Database.Migrate();
                }
            }
        }
    }
}
