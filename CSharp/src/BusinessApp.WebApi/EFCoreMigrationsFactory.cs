using BusinessApp.Infrastructure.EntityFramework;
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
            var config = (IConfiguration?)Program.CreateWebHostBuilder(args)
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
