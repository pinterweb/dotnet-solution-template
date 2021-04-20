using BusinessApp.Infrastructure.EntityFramework;
using BusinessApp.Domain;
using BusinessApp.Infrastructure;
using BusinessApp.WebApi;
using FakeItEasy;
using FakeItEasy.Creation;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SimpleInjector;

namespace BusinessApp.Test.Shared
{
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

#region Event Modeling
            modelBuilder.ApplyConfiguration(new DeleteEventConfiguration());
#endregion

#region Command Modeling
            modelBuilder.ApplyConfiguration(new DeleteQueryConfiguration());
            modelBuilder.ApplyConfiguration(new PostOrPutBodyConfiguration());
#endregion
        }

        private class DeleteEventConfiguration : EventMetadataEntityConfiguration<Delete.Event>
        {
            protected override string TableName => "DeleteEvent";

            protected override void ConfigureEvent(
                OwnedNavigationBuilder<EventMetadata<Delete.Event>, Delete.Event> builder)
            {
                builder.Property(p => p.Id)
                    .HasConversion(id => (int)id, val => new EntityId(val));
            }
        }

        private class PostOrPutBodyConfiguration : MetadataEntityConfiguration<PostOrPut.Body>
        {
            protected override string TableName => "PostOrPutBody";

            public override void Configure(EntityTypeBuilder<PostOrPut.Body> builder)
            {
                base.Configure(builder);

                builder.ToTable("PostOrPutBody");

                builder.Property<int>("PostOrPutBodyRequestId")
                    .ValueGeneratedOnAdd();

                builder.HasKey("PostOrPutBodyRequestId");

                builder.Property(p => p.Id)
                    .HasColumnName("PostOrPutId")
                    .HasConversion(id => (int)id, val => new EntityId(val));
            }
        }

        private class DeleteQueryConfiguration : MetadataEntityConfiguration<Delete.Query>
        {
            protected override string TableName => "DeleteQuery";

            public override void Configure(EntityTypeBuilder<Delete.Query> builder)
            {
                base.Configure(builder);

                builder.Property<int>("DeleteQueryRequestId")
                    .ValueGeneratedOnAdd();

                builder.HasKey("DeleteQueryRequestId");

                builder.Property(p => p.Id)
                    .HasColumnName("DeleteQueryId")
                    .HasConversion(id => (int)id, val => new EntityId(val));
            }
        }
    }
}
