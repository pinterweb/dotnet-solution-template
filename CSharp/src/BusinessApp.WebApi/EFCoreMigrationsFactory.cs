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
    public sealed class MigrationsContextFactory : IDesignTimeDbContextFactory<BusinessAppDbContext>
    {
        public BusinessAppDbContext CreateDbContext(string[] args)
        {
            var config = (IConfiguration?)WebHost.CreateDefaultBuilder(args)
                .ConfigureServices(sc => sc.AddSingleton(new Container()))
                .ConfigureAppConfiguration(builder =>
                {
                    builder.AddCommandLine(args);
                    builder.AddEnvironmentVariables();
                })
                .UseStartup<Startup>()
                .Build()
                .Services
                .GetService(typeof(IConfiguration));

            var connection = config.GetConnectionString("Main");
            var optionsBuilder = new DbContextOptionsBuilder<BusinessAppDbContext>();

            optionsBuilder.UseSqlServer(connection, x => x.MigrationsAssembly("BusinessApp.Infrastructure.EntityFramework"));

            return new BusinessAppDbContext(optionsBuilder.Options);
        }
    }
}
