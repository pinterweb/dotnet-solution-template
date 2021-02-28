namespace BusinessApp.Test.Shared
{
    using BusinessApp.Data;
    using Microsoft.EntityFrameworkCore;

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
                .HasConversion(id => id.ToInt64(null), val => new EventId { Id = val });
            modelBuilder.Entity<ResponseStub>();
            modelBuilder.Entity<ChildResponseStub>();
            base.OnModelCreating(modelBuilder);
        }
    }
}
