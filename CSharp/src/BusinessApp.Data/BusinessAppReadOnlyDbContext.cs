namespace BusinessApp.Data
{
    using Microsoft.EntityFrameworkCore;

    /// <summary>
    /// DbContext serving the query objects
    /// </summary>
    public class BusinessAppReadOnlyDbContext : DbContext
    {
        public BusinessAppReadOnlyDbContext(DbContextOptions<BusinessAppReadOnlyDbContext> opts)
            : base(opts)
        {
            ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(
                typeof(BusinessAppDbContext).Assembly,
                type => type.Name.Contains("Contract")
            );

            base.OnModelCreating(modelBuilder);
        }
    }
}
