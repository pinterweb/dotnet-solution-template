namespace BusinessApp.Test.Shared
{
    using BusinessApp.Data;
    using FakeItEasy;
    using FakeItEasy.Creation;
    using Microsoft.EntityFrameworkCore;

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

    public class BusinessAppTestDbContext : BusinessAppDbContext
    {
        public BusinessAppTestDbContext(DbContextOptions<BusinessAppDbContext> opts)
            : base(opts)
        { }

        public BusinessAppTestDbContext()
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
                .HasConversion(id => id.ToInt64(null), val => new EventId(val));
            modelBuilder.Entity<ResponseStub>();
            modelBuilder.Entity<ChildResponseStub>();
            base.OnModelCreating(modelBuilder);
        }
    }
}
