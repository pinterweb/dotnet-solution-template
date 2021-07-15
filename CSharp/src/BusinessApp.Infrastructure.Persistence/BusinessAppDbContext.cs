using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace BusinessApp.Infrastructure.Persistence
{
    /// <summary>
    /// Application's implementation of DbContext
    /// </summary>
    public class BusinessAppDbContext : DbContext
#if DEBUG
        , IRequestStore
#else
#if metadata
        , IRequestStore
#endif
#endif
    {
        public BusinessAppDbContext(DbContextOptions<BusinessAppDbContext> opts)
            : base(opts)
        { }

#if DEBUG
        public async Task<IEnumerable<RequestMetadata>> GetAllAsync()
            => await Set<RequestMetadata>().ToListAsync();
#else
#if metadata
        public async Task<IEnumerable<RequestMetadata>> GetAllAsync()
            => await Set<RequestMetadata>().ToListAsync();
#endif
#endif

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            _ = modelBuilder
                .ApplyConfigurationsFromAssembly(typeof(BusinessAppDbContext).Assembly);

            base.OnModelCreating(modelBuilder);
        }
    }
}
