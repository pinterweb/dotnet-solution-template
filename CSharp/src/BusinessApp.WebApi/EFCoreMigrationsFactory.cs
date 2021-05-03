using BusinessApp.Infrastructure.EntityFramework;
using Microsoft.AspNetCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Hosting;
using SimpleInjector;

namespace BusinessApp.WebApi
{
    /// <summary>
    /// <see cref="IDesignTimeDbContextFactory" /> needed for entity framework migrations
    /// </summary>
    public sealed class MigrationsContextFactory : IDesignTimeDbContextFactory<BusinessAppDbContext>
    {
        public BusinessAppDbContext CreateDbContext(string[] args)
        {
            var config = (IConfiguration?)WebHost.CreateDefaultBuilder(args)
                .ConfigureServices(sc => sc.AddSingleton(new Container()))
                .ConfigureAppConfiguration(builder =>
                {
                    _ = builder
                        .AddCommandLine(args)
                        .AddEnvironmentVariables();
                })
                .UseStartup<Startup>()
                .Build()
                .Services
                .GetService(typeof(IConfiguration));

            var connection = config.GetConnectionString("Main");
            var optionsBuilder = new DbContextOptionsBuilder<BusinessAppDbContext>();

            _ = optionsBuilder.UseSqlServer(connection,
                x => x.MigrationsAssembly("BusinessApp.Infrastructure.EntityFramework"));

            return new BusinessAppDbContext(optionsBuilder.Options);
        }
    }
}
