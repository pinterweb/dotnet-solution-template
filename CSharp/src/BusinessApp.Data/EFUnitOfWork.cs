using System;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BusinessApp.Infrastructure;
using BusinessApp.Domain;
using Microsoft.EntityFrameworkCore;

namespace BusinessApp.Data
{
    public class EFUnitOfWork : IUnitOfWork, ITransactionFactory
    {
        private bool transactionFromFactory = false;
        public readonly BusinessAppDbContext db;

        public EFUnitOfWork(BusinessAppDbContext db)
        {
            this.db = db.NotNull().Expect(nameof(db));
        }

        public event EventHandler Committing = delegate {};
        public event EventHandler Committed = delegate {};

        public void Track<T>(T aggregate) where T : AggregateRoot
        {
            db.Attach(aggregate);
        }

        public void Add<T>(T aggregate) where T : AggregateRoot
        {
            db.Add(aggregate);
        }

        public void Remove<T>(T aggregate) where T : AggregateRoot
        {
            db.Remove(aggregate);
        }

        public async Task CommitAsync(CancellationToken cancelToken)
        {
            Volatile.Read(ref Committing).Invoke(this, EventArgs.Empty);

            try
            {
                await db.SaveChangesAsync(false, cancelToken);
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
                await db.Database.CurrentTransaction.CommitAsync(cancelToken);
                transactionFromFactory = false;
            }

            db.ChangeTracker.AcceptAllChanges();

            Volatile.Read(ref Committed).Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// Resaves tracked entities. Assumes entity data has been rollbacked/chanaged
        /// before being called
        /// </summary>
        public async Task RevertAsync(CancellationToken cancelToken)
        {
            await db.SaveChangesAsync(false, cancelToken);

            if (db.Database.CurrentTransaction != null && transactionFromFactory)
            {
                await db.Database.CurrentTransaction.CommitAsync(cancelToken);
                transactionFromFactory = false;
            }

            db.ChangeTracker.AcceptAllChanges();

            Volatile.Read(ref Committed).Invoke(this, EventArgs.Empty);
        }

        public Result<IUnitOfWork, Exception> Begin()
        {
            try
            {
                db.Database.BeginTransaction();
                transactionFromFactory = true;
                return Result.Ok<IUnitOfWork>(this);
            }
            catch (InvalidOperationException e) when (AlreadyInTransaction(e.Message))
            {
                return Result.Error<IUnitOfWork>(e);
            }
        }

        public T? Find<T>(Func<T, bool> filter) where T : AggregateRoot
        {
            return db.ChangeTracker.Entries<T>()
                .Select(e => e.Entity)
                .SingleOrDefault(filter);
        }

        private static bool AlreadyInTransaction(string msg)
            => msg == "The connection is already in a transaction and cannot participate in "
                + "another transaction.";
    }
}
