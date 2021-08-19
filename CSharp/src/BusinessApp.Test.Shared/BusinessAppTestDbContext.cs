using BusinessApp.Infrastructure;
using BusinessApp.Infrastructure.Persistence;
using BusinessApp.Kernel;
using BusinessApp.WebApi;
using FakeItEasy;
using FakeItEasy.Creation;
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

    public sealed class EFCoreMigrationsFactory : IDesignTimeDbContextFactory<BusinessAppTestDbContext>
    {
        public BusinessAppTestDbContext CreateDbContext(string[] args)
        {
            var config = (IConfiguration)Program.CreateWebHostBuilder(args)
                .ConfigureServices(sc => sc.AddSingleton(new Container()))
                .ConfigureAppConfiguration((hostingContext, config) =>
                {
                    _ = config.AddCommandLine(args)
                        .AddJsonFile("appsettings.Migrations.json")
                        .AddEnvironmentVariables(prefix: "BusinessApp_");
                })
                .Build()
                .Services
                .GetService(typeof(IConfiguration));

            var optionsBuilder = new DbContextOptionsBuilder<BusinessAppDbContext>();
            var connection = config.GetConnectionString("Test");

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
#if DEBUG
            modelBuilder.Entity<EventStub>()
                .Property(p => p.Id)
                .HasConversion(id => id.ToInt64(null), val => new MetadataId(val));
#elif events
            // additional test models here
            modelBuilder.Entity<DomainEventStub>()
                .Property(p => p.Id)
                .HasConversion(id => id.ToInt64(null), val => new MetadataId(val));
#endif
            modelBuilder.Entity<ResponseStub>();
            modelBuilder.Entity<RequestStub>();
            modelBuilder.Entity<ChildResponseStub>();
            modelBuilder.Entity<AggregateRootStub>();

            base.OnModelCreating(modelBuilder);

#if DEBUG
            modelBuilder.ApplyConfiguration(new DeleteEventConfiguration());
#elif events
            modelBuilder.ApplyConfiguration(new DeleteEventConfiguration());
#endif

#region Command Modeling
            modelBuilder.ApplyConfiguration(new DeleteQueryConfiguration());
            modelBuilder.ApplyConfiguration(new PostOrPutBodyConfiguration());
#endregion
        }

#if DEBUG
        private class DeleteEventConfiguration : EventMetadataEntityConfiguration<Delete.WebDomainEvent>
        {
            protected override string TableName => "DeleteEvent";

            protected override void ConfigureEvent(
                OwnedNavigationBuilder<EventMetadata<Delete.WebDomainEvent>, Delete.WebDomainEvent> builder)
            {
                builder.Property(p => p.Id)
                    .HasConversion(id => (int)id, val => new EntityId(val));
            }
        }
#elif events
        private class DeleteEventConfiguration : EventMetadataEntityConfiguration<Delete.WebDomainEvent>
        {
            protected override string TableName => "DeleteEvent";

            protected override void ConfigureEvent(
                OwnedNavigationBuilder<EventMetadata<Delete.WebDomainEvent>, Delete.WebDomainEvent> builder)
            {
                builder.Property(p => p.Id)
                    .HasConversion(id => (int)id, val => new EntityId(val));
            }
        }
#endif

        private class PostOrPutBodyConfiguration : MetadataEntityConfiguration<PostOrPut.Body>
        {
            public override void Configure(EntityTypeBuilder<PostOrPut.Body> builder)
            {
                builder.ToTable("PostOrPutBody");

                builder.Property<int>("PostOrPutBodyRequestId")
                    .ValueGeneratedOnAdd();

                builder.HasKey("PostOrPutBodyRequestId");

                builder.Property(p => p.Id)
                    .HasColumnName("PostOrPutId")
                    .HasConversion(id => (int)id, val => new EntityId(val));

                base.Configure(builder);
            }
        }

        private class DeleteQueryConfiguration : MetadataEntityConfiguration<Delete.Query>
        {
            public override void Configure(EntityTypeBuilder<Delete.Query> builder)
            {
                builder.ToTable("DeleteQuery");

                builder.Property<int>("DeleteQueryRequestId")
                    .ValueGeneratedOnAdd();

                builder.HasKey("DeleteQueryRequestId");

                builder.Property(p => p.Id)
                    .HasColumnName("DeleteQueryId")
                    .HasConversion(id => (int)id, val => new EntityId(val));

                base.Configure(builder);
            }
        }
    }
}
