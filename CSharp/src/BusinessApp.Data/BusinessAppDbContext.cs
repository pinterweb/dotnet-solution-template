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

    /// <summary>
    /// DbContext serving the domain Aggregate(s)
    /// </summary>
    public class BusinessAppDbContext : DbContext, IUnitOfWork, ITransactionFactory
    {
        private readonly EventUnitOfWork eventUow;
        private bool transactionFromFactory = false;

        public BusinessAppDbContext(
            DbContextOptions<BusinessAppDbContext> opts,
            EventUnitOfWork eventUow
        )
            : base(opts)
        {
            this.eventUow = GuardAgainst.Null(eventUow, nameof(eventUow));
        }

        internal event Action Saving = delegate { };
        internal event Action Saved = delegate { };

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(
                typeof(BusinessAppDbContext).Assembly,
                type => !type.Name.Contains("Contract")
            );

            base.OnModelCreating(modelBuilder);
        }

        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            Volatile.Read(ref Saving).Invoke();

            var changes = await base.SaveChangesAsync(cancellationToken);

            Volatile.Read(ref Saved).Invoke();

            return changes;
        }

        public override void Dispose()
        {
            Saved = delegate { };
            Saving = delegate { };
            base.Dispose();
        }

        void IUnitOfWork.Add(AggregateRoot aggregate)
        {
            eventUow.Add(aggregate);
            base.Add(aggregate);
        }

        void IUnitOfWork.Remove(AggregateRoot aggregate)
        {
            eventUow.Remove(aggregate);
            base.Remove(aggregate);
        }

        async Task IUnitOfWork.CommitAsync(CancellationToken cancellationToken)
        {
            var emitters = ChangeTracker.Entries<IEventEmitter>()
                .Where(e => e.State == EntityState.Modified || e.State == EntityState.Unchanged)
                .Select(e => e.Entity);

            foreach (var a in emitters) eventUow.Add(a);

            await eventUow.CommitAsync(cancellationToken);

            int changes = 0;

            try
            {
                changes = await base.SaveChangesAsync(false, cancellationToken);
            }
            catch (DbUpdateConcurrencyException ex)
            {
                throw new DBConcurrencyException("An error occurred while saving your data. " +
                    "The data may have been modified or deleted while you were working " +
                    "Please make sure you are working with the most up to date data"
                    , ex
                );
            }

            if (Database.CurrentTransaction != null && transactionFromFactory)
            {
                await Database.CurrentTransaction.CommitAsync(cancellationToken);
                transactionFromFactory = false;
            }
        }

        Task IUnitOfWork.RevertAsync(CancellationToken cancellationToken)
        {
            return eventUow.RevertAsync(cancellationToken);
        }

        public IUnitOfWork Begin()
        {
            Database.BeginTransaction();

            transactionFromFactory = true;

            return this;
        }
    }
}
