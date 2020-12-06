namespace BusinessApp.Test
{
    using System;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Migrations;
    using Microsoft.Extensions.Logging;
    using BusinessApp.Data;
    using Microsoft.Extensions.Configuration;
    using Microsoft.AspNetCore.Hosting;
    using BusinessApp.WebApi;
    using Microsoft.EntityFrameworkCore.Infrastructure;

    public class DatabaseFixture : IDisposable
	{
        public static readonly ILoggerFactory EFDebugLoggerFactory
            = LoggerFactory.Create(builder =>
            {
                builder
                    .AddFilter((category, level) =>
                        category == DbLoggerCategory.Database.Command.Name
                        && level == LogLevel.Information)
                    .AddDebug();
            });

        private static readonly string ConnectionStr;
        private readonly BusinessAppDbContext realDb;

        static DatabaseFixture()
		{
            var config = (IConfiguration)Program.CreateWebHostBuilder(new string[0])
                .ConfigureAppConfiguration((_, builder) =>
                {
                    builder.AddJsonFile("appsettings.test.json");
                })
                .UseStartup<Startup>()
                .Build()
                .Services
                .GetService(typeof(IConfiguration));

            ConnectionStr = config.GetConnectionString("local");
		}

		public DatabaseFixture()
		{
            realDb = new BusinessAppDbContext(
                new DbContextOptionsBuilder<BusinessAppDbContext>()
                    .UseLoggerFactory(EFDebugLoggerFactory)
                    .UseSqlServer(ConnectionStr)
                    .Options
            );
            DbContext = new BusinessAppTestDbContext(
                new DbContextOptionsBuilder<BusinessAppDbContext>()
                    .UseLoggerFactory(EFDebugLoggerFactory)
                    .UseSqlServer(ConnectionStr)
                    .EnableSensitiveDataLogging()
                    .Options
            );

            DbContext.Database.Migrate();
		}

		public BusinessAppDbContext DbContext { get; }

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

        private sealed class Startup
        {
            public void Configure()
            {

            }
        }
	}
}
