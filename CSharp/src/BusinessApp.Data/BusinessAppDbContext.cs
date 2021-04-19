using System.Collections.Generic;
using System.Threading.Tasks;
using BusinessApp.App;
using Microsoft.EntityFrameworkCore;

namespace BusinessApp.Data
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
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(BusinessAppDbContext).Assembly);

            base.OnModelCreating(modelBuilder);
        }
    }
}
