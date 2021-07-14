using Microsoft.EntityFrameworkCore;

namespace BusinessApp.Infrastructure.Persistence
{
    /// <summary>
    /// Application's implementation of DbContext
    /// </summary>
    public class BusinessAppDbContext : DbContext
    {
        public BusinessAppDbContext(DbContextOptions<BusinessAppDbContext> opts)
            : base(opts)
        { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            _ = modelBuilder
                .ApplyConfigurationsFromAssembly(typeof(BusinessAppDbContext).Assembly);

            base.OnModelCreating(modelBuilder);
        }
    }
}
