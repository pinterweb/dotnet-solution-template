namespace BusinessApp.Data
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.EntityFrameworkCore;
    using BusinessApp.Domain;

    /// <summary>
    /// Repository implementation of the Idetity Map pattern, using Entity Framework's cache
    /// </summary>
    public abstract class EFIdentityMap<TAggregate, TId> : AggregateRootRepository<TAggregate>
        where TAggregate : AggregateRoot<TId>
        where TId : IEntityId
    {
        private readonly BusinessAppDbContext db;
        protected readonly IAggregateRootRepository<TAggregate, TId> dbRepo;

        public EFIdentityMap(BusinessAppDbContext db,
            IAggregateRootRepository<TAggregate, TId> dbRepo)
            : base(db)
        {
            this.db = GuardAgainst.Null(db, nameof(db));
            this.dbRepo = GuardAgainst.Null(dbRepo, nameof(dbRepo));
        }

        public Task<TAggregate> GetByIdAsync(TId id)
        {
            var local = db.Set<TAggregate>().Local.SingleOrDefault(m => id.Equals(m.Id));

            if (local != null)
            {
                return Task.FromResult(local);
            }

            return dbRepo.GetByIdAsync(id);
        }

        public Task<IEnumerable<TAggregate>> GetAllAsync(IEnumerable<TId> ids)
        {
            var locals = db.Set<TAggregate>().Local.Where(m => ids.Contains(m.Id));

            if (locals.Any())
            {
                return Task.FromResult(locals);
            }

            return dbRepo.GetAllAsync(ids);
        }
    }
}
