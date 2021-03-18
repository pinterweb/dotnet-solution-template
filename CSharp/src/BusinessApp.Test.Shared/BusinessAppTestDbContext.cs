namespace BusinessApp.Test.Shared
{
    using BusinessApp.Data;
    using BusinessApp.Domain;
    using BusinessApp.WebApi;
    using FakeItEasy;
    using FakeItEasy.Creation;
    using Microsoft.AspNetCore;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Design;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using SimpleInjector;

    public class BusinessAppDbContextFakeBuilder : FakeOptionsBuilder<BusinessAppDbContext>
    {
        protected override void BuildOptions(IFakeOptions<BusinessAppDbContext> options)
        {
            var dbOptions = new DbContextOptionsBuilder<BusinessAppDbContext>()
                .UseSqlServer("Server=(localdb)\\MSSQLLocalDb;Initial Catalog=foobar")
                .EnableSensitiveDataLogging()
                .Options;

            options.WithArgumentsForConstructor(new object[] { dbOptions } );
        }
    }

    public class BusinessAppDbContextDummyFactory : DummyFactory<BusinessAppDbContext>
    {
        protected override BusinessAppDbContext Create() => new BusinessAppTestDbContext();
    }

    public sealed class MigrationsContextFactory : IDesignTimeDbContextFactory<BusinessAppTestDbContext>
    {
        public BusinessAppTestDbContext CreateDbContext(string[] args)
        {
            var config = (IConfiguration)WebHost.CreateDefaultBuilder(args)
                .ConfigureServices(sc => sc.AddSingleton(new Container()))
                .ConfigureAppConfiguration((hostingContext, config) =>
                {
                    config.AddCommandLine(args);
                    config.AddEnvironmentVariables();
                    config.AddJsonFile("appsettings.test.json");
                })
                .UseStartup<Startup>()
                .Build()
                .Services
                .GetService(typeof(IConfiguration));

            var optionsBuilder = new DbContextOptionsBuilder<BusinessAppDbContext>();
            var connection = config.GetConnectionString("local");

            optionsBuilder.UseSqlServer(connection);

            return new BusinessAppTestDbContext(new BusinessAppDbContext(optionsBuilder.Options),
                optionsBuilder.Options);
        }
    }

    public class BusinessAppTestDbContext : BusinessAppDbContext
    {
        public BusinessAppTestDbContext(BusinessAppDbContext db,
            DbContextOptions<BusinessAppDbContext> opts)
            : base(opts)
        { }

        internal BusinessAppTestDbContext()
            : base(
                new DbContextOptionsBuilder<BusinessAppDbContext>()
                    .UseSqlServer("Server=(localdb)\\MSSQLLocalDb;Initial Catalog=foobar")
                    .Options
            )
        { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // additional test models here
            modelBuilder.Entity<DomainEventStub>()
                .Property(p => p.Id)
                .HasConversion(id => id.ToInt64(null), val => new MetadataId(val));
            modelBuilder.Entity<ResponseStub>();
            modelBuilder.Entity<RequestStub>();
            modelBuilder.Entity<ChildResponseStub>();
            modelBuilder.Entity<AggregateRootStub>();

            base.OnModelCreating(modelBuilder);

            modelBuilder.ApplyConfiguration(new DeleteEventConfiguration());
        }

        private class DeleteEventConfiguration : EventMetadataEntityConfiguration<Delete.Event>
        {
            protected override string TableName => "DeleteEvent";
        }
    }
}
