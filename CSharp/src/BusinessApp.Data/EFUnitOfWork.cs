namespace BusinessApp.Data
{
    using System;
    using System.Data;
    using System.Threading;
    using System.Threading.Tasks;
    using BusinessApp.App;
    using BusinessApp.Domain;
    using Microsoft.EntityFrameworkCore;

    public class EFUnitOfWork : IUnitOfWork, ITransactionFactory
    {
        private bool transactionFromFactory = false;
        public readonly BusinessAppDbContext db;
        public readonly EventUnitOfWork inner;

        public EFUnitOfWork(BusinessAppDbContext db, EventUnitOfWork inner)
        {
            this.db = GuardAgainst.Null(db, nameof(db));
            this.inner = GuardAgainst.Null(inner, nameof(inner));
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

        public void Remove(AggregateRoot aggregate)
        {
            inner.Remove(aggregate);
        }

        public async Task CommitAsync(CancellationToken cancellationToken)
        {
            await inner.CommitAsync(cancellationToken);

            Volatile.Read(ref Committing).Invoke(this, EventArgs.Empty);

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
    }
}