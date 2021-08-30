using System;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BusinessApp.Kernel;
using Microsoft.EntityFrameworkCore;

namespace BusinessApp.Infrastructure.Persistence
{
    /// <summary>
    /// Unit of work pattern wrapping Entity Framework
    /// </summary>
    public class EFUnitOfWork : IUnitOfWork
    {
        private readonly BusinessAppDbContext db;

        public EFUnitOfWork(BusinessAppDbContext db) => this.db = db.NotNull().Expect(nameof(db));

        public event EventHandler Committing = delegate { };
        public event EventHandler Committed = delegate { };

        public void Track<T>(T aggregate) where T : AggregateRoot => db.Attach(aggregate);

        public void Add<T>(T aggregate) where T : AggregateRoot => db.Add(aggregate);

        public void Remove<T>(T aggregate) where T : AggregateRoot => db.Remove(aggregate);

        public async Task CommitAsync(CancellationToken cancelToken)
        {
            Volatile.Read(ref Committing).Invoke(this, EventArgs.Empty);

            try
            {
                _ = await db.SaveChangesAsync(cancelToken);
            }
            catch (DbUpdateConcurrencyException ex)
            {
                throw new DBConcurrencyException("An error occurred while saving your data. " +
                    "The data may have been modified or deleted while you were working " +
                    "Please make sure you are working with the most up to date data"
                    , ex
                );
            }

            Volatile.Read(ref Committed).Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// Resaves tracked entities. Assumes entity data has been rollbacked/chanaged
        /// before being called
        /// </summary>
        public async Task RevertAsync(CancellationToken cancelToken)
        {
            _ = await db.SaveChangesAsync(cancelToken);

            Volatile.Read(ref Committed).Invoke(this, EventArgs.Empty);
        }

        public T? Find<T>(Func<T, bool> filter) where T : AggregateRoot
            => db.ChangeTracker.Entries<T>()
                .Select(e => e.Entity)
                .SingleOrDefault(filter);
    }
}
