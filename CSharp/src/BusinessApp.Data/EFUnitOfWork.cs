namespace BusinessApp.Data
{
    using System;
    using System.Data;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using BusinessApp.App;
    using BusinessApp.Domain;
    using Microsoft.EntityFrameworkCore;

    public class EFUnitOfWork : IUnitOfWork, ITransactionFactory
    {
        private bool transactionFromFactory = false;
        public readonly BusinessAppDbContext db;
        public readonly IUnitOfWork inner;

        public EFUnitOfWork(BusinessAppDbContext db, IUnitOfWork inner)
        {
            this.db = db.NotNull().Expect(nameof(db));
            this.inner = inner.NotNull().Expect(nameof(inner));
        }

        public event EventHandler Committing = delegate {};
        public event EventHandler Committed = delegate {};

        public void Track(AggregateRoot aggregate)
        {
            inner.Track(aggregate);
        }

        public void Add(AggregateRoot aggregate)
        {
            inner.Add(aggregate);
        }

        public void Add(IDomainEvent @event)
        {
            inner.Add(@event);
        }

        public void Remove(AggregateRoot aggregate)
        {
            inner.Remove(aggregate);
        }

        public async Task CommitAsync(CancellationToken cancellationToken)
        {
            Volatile.Read(ref Committing).Invoke(this, EventArgs.Empty);

            await inner.CommitAsync(cancellationToken);

            try
            {
                await db.SaveChangesAsync(false, cancellationToken);
            }
            catch (DbUpdateConcurrencyException ex)
            {
                throw new DBConcurrencyException("An error occurred while saving your data. " +
                    "The data may have been modified or deleted while you were working " +
                    "Please make sure you are working with the most up to date data"
                    , ex
                );
            }

            if (db.Database.CurrentTransaction != null && transactionFromFactory)
            {
                await db.Database.CurrentTransaction.CommitAsync(cancellationToken);
                transactionFromFactory = false;
            }

            db.ChangeTracker.AcceptAllChanges();

            Volatile.Read(ref Committed).Invoke(this, EventArgs.Empty);
        }

        public async Task RevertAsync(CancellationToken cancellationToken)
        {
            await inner.RevertAsync(cancellationToken);

            await db.SaveChangesAsync(false, cancellationToken);

            if (db.Database.CurrentTransaction != null && transactionFromFactory)
            {
                await db.Database.CurrentTransaction.CommitAsync(cancellationToken);
                transactionFromFactory = false;
            }

            db.ChangeTracker.AcceptAllChanges();

            Volatile.Read(ref Committed).Invoke(this, EventArgs.Empty);
        }

        public IUnitOfWork Begin()
        {
            db.Database.BeginTransaction();

            transactionFromFactory = true;

            return this;
        }

        public TRoot Find<TRoot>(Func<TRoot, bool> filter) where TRoot : AggregateRoot
        {
            return inner.Find<TRoot>(filter);
        }
    }
}
