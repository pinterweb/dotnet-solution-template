namespace BusinessApp.Test
{
    using BusinessApp.Data;
    using BusinessApp.Domain;
    using FakeItEasy;
    using Microsoft.EntityFrameworkCore;

    public class BusinessAppTestDbContext : BusinessAppDbContext
    {
        public BusinessAppTestDbContext(DbContextOptions<BusinessAppDbContext> opts,
            EventUnitOfWork uow)
            : base(opts, uow)
        { }

        public BusinessAppTestDbContext()
            : base(
                new DbContextOptionsBuilder<BusinessAppDbContext>()
                    .UseSqlServer("Server=(localdb)\\MSSQLLocalDb;Initial Catalog=foobar")
                    .Options,
                A.Dummy<EventUnitOfWork>()
            )
        { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // additional test models here
            modelBuilder.Entity<DomainEventStub>();
            modelBuilder.Entity<ResponseStub>();
            base.OnModelCreating(modelBuilder);
        }
    }
}
