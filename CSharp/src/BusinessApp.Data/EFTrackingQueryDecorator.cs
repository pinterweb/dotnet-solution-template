namespace BusinessApp.Data
{
    using System.Threading;
    using System.Threading.Tasks;
    using BusinessApp.App;
    using BusinessApp.Domain;
    using Microsoft.EntityFrameworkCore;

    /// <summary>
    /// Entity Framework query decorator to set the query tracking behavior to
    /// none for query the inner handler. This will results in faster queries since
    /// we are not worried about saving any entities during this transaction
    /// </summary>
    public class EFTrackingQueryDecorator<TQuery, TResult> : IQueryHandler<TQuery, TResult>
        where TQuery : Query, IQuery<TResult>
    {
        private readonly BusinessAppDbContext db;
        private readonly IQueryHandler<TQuery, TResult> inner;

        public EFTrackingQueryDecorator(BusinessAppDbContext db,
            IQueryHandler<TQuery, TResult> inner)
        {
            GuardAgainst.Null(db, nameof(db));
            this.inner = GuardAgainst.Null(inner, nameof(inner));

            db.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
        }

        public Task<TResult> HandleAsync(TQuery query, CancellationToken cancellationToken)
        {
            return inner.HandleAsync(query, cancellationToken);
        }
    }
}
