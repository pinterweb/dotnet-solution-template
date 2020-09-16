namespace BusinessApp.Test
{
    using System;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Infrastructure;
    using Microsoft.EntityFrameworkCore.Migrations;
    using Microsoft.Extensions.Logging;
    using BusinessApp.Data;
    using Microsoft.Extensions.Configuration;
    using BusinessApp.Domain;
    using FakeItEasy;
    using Microsoft.AspNetCore.Hosting;
    using BusinessApp.WebApi;

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

            ConnectionStr = config.GetConnectionString("sqlserver");
		}

		public DatabaseFixture()
		{
            ReadContext = new BusinessAppReadOnlyTestDbContext(
                new DbContextOptionsBuilder<BusinessAppReadOnlyDbContext>()
                    .UseLoggerFactory(EFDebugLoggerFactory)
                    .UseSqlServer(ConnectionStr)
                    .Options
            );

            WriteContext = new BusinessAppDbContext(
                new DbContextOptionsBuilder<BusinessAppDbContext>()
                    .UseLoggerFactory(EFDebugLoggerFactory)
                    .UseSqlServer(ConnectionStr)
                    .Options,
                A.Dummy<EventUnitOfWork>()
            );

            WriteContext.Database.Migrate();
            ReadContext.Database.Migrate();
		}

		public BusinessAppReadOnlyDbContext ReadContext { get; }
		public BusinessAppDbContext WriteContext { get; }

		public void Dispose()
		{
            try
            {
                ReadContext.GetService<IMigrator>().Migrate("0");
                WriteContext.GetService<IMigrator>().Migrate("0");
            }
            catch
            {
                // can't migration back, just delete
                ReadContext.Database.EnsureDeleted();
                WriteContext.Database.EnsureDeleted();
            }

            ReadContext.Dispose();
            WriteContext.Dispose();
		}

        private class Startup
        {
            public void Configure()
            {

            }
        }
	}
}
