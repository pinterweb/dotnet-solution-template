using BusinessApp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace BusinessApp.WebApi
{
    /// <summary>
    /// <see cref="IDesignTimeDbContextFactory" /> needed for entity framework migrations
    /// </summary>
    public sealed class EFCoreMigrationsFactory : IDesignTimeDbContextFactory<BusinessAppDbContext>
    {
        public BusinessAppDbContext CreateDbContext(string[] args)
        {
            var config = (IConfiguration?)Program.CreateHostBuilder(args)
                .ConfigureAppConfiguration((_, builder) =>
                {
                    builder.AddJsonFile("appsettings.Development.json");
                    builder.AddEnvironmentVariables(prefix: "BusinessApp_");
                })
                .Build()
                .Services
                .GetService(typeof(IConfiguration));

            var connection = config.GetConnectionString("Main");
            var optionsBuilder = new DbContextOptionsBuilder<BusinessAppDbContext>();

            _ = optionsBuilder.UseSqlServer(connection,
                x => x.MigrationsAssembly("BusinessApp.Infrastructure.Persistence"));

            return new BusinessAppDbContext(optionsBuilder.Options);
        }
    }
}
