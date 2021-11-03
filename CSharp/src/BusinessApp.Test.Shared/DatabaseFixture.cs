using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.Logging;
using BusinessApp.Infrastructure.Persistence;
using Microsoft.Extensions.Configuration;
using BusinessApp.WebApi;
using Microsoft.EntityFrameworkCore.Infrastructure;
using System.Threading;

namespace BusinessApp.Test.Shared
{
    public abstract class DatabaseFixture : IDisposable
    {
        public static readonly ILoggerFactory EFDebugLoggerFactory
            = LoggerFactory.Create(builder =>
                builder
                    .AddFilter((category, level) =>
                        category == DbLoggerCategory.Database.Command.Name
                        && level == LogLevel.Information)
                    .AddConsole()
                    .AddDebug());

        protected event Action<BusinessAppDbContext> DatabaseMigrated = delegate { };

        private readonly BusinessAppDbContext realDb;

        public DatabaseFixture()
        {
            var config = (IConfiguration)Program.CreateHostBuilder(Array.Empty<string>())
                .ConfigureAppConfiguration((_, builder) => Configure(builder))
                .Build()
                .Services
                .GetService(typeof(IConfiguration));

            var connectionStr = config.GetConnectionString(ConnectionStringName);

            var options = new DbContextOptionsBuilder<BusinessAppDbContext>()
                .UseLoggerFactory(EFDebugLoggerFactory)
                .UseSqlServer(connectionStr)
                .EnableSensitiveDataLogging()
                .Options;

            realDb = new BusinessAppDbContext(options);
            DbContext = new BusinessAppTestDbContext(realDb, options);

            DbContext.Database.Migrate();

            Volatile.Read(ref DatabaseMigrated).Invoke(DbContext);
        }

        public BusinessAppDbContext DbContext { get; }

        protected abstract string ConnectionStringName { get; }

        public void Dispose()
        {
            try
            {
                DbContext.GetService<IMigrator>().Migrate("0");
                realDb.GetService<IMigrator>().Migrate("0");
            }
            catch
            {
                // can't migration back, just delete
                DbContext.Database.EnsureDeleted();
            }

            DbContext.Dispose();
        }

        /// <summary>
        /// Configures the integration test env and returns the connection string name to use
        /// </summary>
        protected abstract void Configure(IConfigurationBuilder builder);
    }
}
