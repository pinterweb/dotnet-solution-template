namespace BusinessApp.Data.IntegrationTest
{
    using BusinessApp.Domain;
    using FakeItEasy;
    using Microsoft.EntityFrameworkCore;

    public class BusinessAppDbContextDummyFactory : DummyFactory<BusinessAppDbContext>
    {
        public static BusinessAppDbContext Create(EventUnitOfWork uow = null)
        {
            return new FakeBusinessAppDbContext(new DbContextOptionsBuilder<BusinessAppDbContext>()
                .UseSqlServer("Server=(localdb)\\MSSQLLocalDb;Initial Catalog=foobar").Options,
                uow ?? A.Dummy<EventUnitOfWork>());
        }

        protected override BusinessAppDbContext Create()
        {
            return new BusinessAppDbContext(new DbContextOptionsBuilder<BusinessAppDbContext>()
                .UseSqlServer("Server=(localdb)\\MSSQLLocalDb;Initial Catalog=foobar").Options,
                A.Dummy<EventUnitOfWork>());
        }

        private class FakeBusinessAppDbContext : BusinessAppDbContext
        {
            public FakeBusinessAppDbContext(DbContextOptions<BusinessAppDbContext> opts,
                EventUnitOfWork uow)
                : base(opts, uow)
            {
            }

            protected override void OnModelCreating(ModelBuilder modelBuilder)
            {
                modelBuilder.ApplyConfigurationsFromAssembly(
                    this.GetType().Assembly,
                    type => type.Name.EndsWith("ModelConfiguration"));

                base.OnModelCreating(modelBuilder);
            }
        }
    }
}
