namespace BusinessApp.Data
{
    using System;
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
    public class EFTrackingQueryDecorator<TQuery, TResult> : IRequestHandler<TQuery, TResult>
        where TQuery : IQuery
    {
        private readonly IRequestHandler<TQuery, TResult> inner;

        public EFTrackingQueryDecorator(BusinessAppDbContext db,
            IRequestHandler<TQuery, TResult> inner)
        {
            db.NotNull().Expect(nameof(db));
            this.inner = inner.NotNull().Expect(nameof(inner));

            db.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
        }

        public Task<Result<TResult, IFormattable>> HandleAsync(TQuery query,
            CancellationToken cancellationToken)
        {
            return inner.HandleAsync(query, cancellationToken);
        }
    }
}
