using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace BusinessApp.Infrastructure.EntityFramework
{
    public class BusinessAppDbContext : DbContext, IRequestStore
    {
        public BusinessAppDbContext(DbContextOptions<BusinessAppDbContext> opts)
            : base(opts)
        { }

        public async Task<IEnumerable<RequestMetadata>> GetAllAsync()
            => await Set<RequestMetadata>().ToListAsync();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            _ = modelBuilder
                .ApplyConfigurationsFromAssembly(typeof(BusinessAppDbContext).Assembly);

            base.OnModelCreating(modelBuilder);
        }
    }
}
