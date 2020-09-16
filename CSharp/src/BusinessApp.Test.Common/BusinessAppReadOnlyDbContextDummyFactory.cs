namespace BusinessApp.Test
{
    using BusinessApp.Data;
    using Microsoft.EntityFrameworkCore;

    public class FakeBusinessAppReadOnlyDbContext : BusinessAppReadOnlyDbContext
    {
        public FakeBusinessAppReadOnlyDbContext(DbContextOptions<BusinessAppReadOnlyDbContext> opts)
            : base(opts)
        { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(
                this.GetType().Assembly,
                type => type.Name.EndsWith("ReadOnlyModelConfiguration"));

            base.OnModelCreating(modelBuilder);
        }
    }
}
