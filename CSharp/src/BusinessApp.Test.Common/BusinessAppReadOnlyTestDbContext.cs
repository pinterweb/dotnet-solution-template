namespace BusinessApp.Test
{
    using BusinessApp.Data;
    using Microsoft.EntityFrameworkCore;

    public class BusinessAppReadOnlyTestDbContext : BusinessAppReadOnlyDbContext
    {
        public BusinessAppReadOnlyTestDbContext(DbContextOptions<BusinessAppReadOnlyDbContext> opts)
            : base(opts)
        { }

        public BusinessAppReadOnlyTestDbContext()
            : base(
                new DbContextOptionsBuilder<BusinessAppReadOnlyDbContext>()
                    .UseSqlServer("Server=(localdb)\\MSSQLLocalDb;Initial Catalog=foobar")
                    .Options
            )
        { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // additional test models here
            modelBuilder.Entity<ResponseStub>();
            base.OnModelCreating(modelBuilder);
        }
    }
}
