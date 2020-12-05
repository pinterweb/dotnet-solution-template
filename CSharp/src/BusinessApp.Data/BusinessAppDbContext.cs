namespace BusinessApp.Data
{
    using Microsoft.EntityFrameworkCore;

    public class BusinessAppDbContext : DbContext, IDatabase
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

        void IDatabase.AddOrReplace<TEntity>(TEntity entity) where TEntity : class
        {
            ChangeTracker.TrackGraph(
                entity,
                (node) =>
                {
                    var exists = node.Entry.GetDatabaseValues() != null;

                    if (exists)
                    {
                        node.Entry.State = EntityState.Modified;
                    }
                    else
                    {
                        node.Entry.State = EntityState.Added;
                    }
                }
            );
        }

        void IDatabase.Remove<TEntity>(TEntity entity) where TEntity : class
        {
            Remove(entity);
        }
    }
}
