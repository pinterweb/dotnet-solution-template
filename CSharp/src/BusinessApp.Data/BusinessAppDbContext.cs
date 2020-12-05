namespace BusinessApp.Data
{
    using Microsoft.EntityFrameworkCore;

    public class BusinessAppDbContext : DbContext
    {
        public BusinessAppDbContext(DbContextOptions<BusinessAppDbContext> opts)
            : base(opts)
        {}

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasSequence<long>("EventIds", schema: "evt");

            modelBuilder.ApplyConfigurationsFromAssembly(
                typeof(BusinessAppDbContext).Assembly
            );

            base.OnModelCreating(modelBuilder);
        }
    }
}
