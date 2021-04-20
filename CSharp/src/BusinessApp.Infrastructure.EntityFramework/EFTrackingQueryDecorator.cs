using System;
using System.Threading;
using System.Threading.Tasks;
using BusinessApp.Infrastructure;
using BusinessApp.Kernel;
using Microsoft.EntityFrameworkCore;

namespace BusinessApp.Infrastructure.EntityFramework
{
    /// <summary>
    /// Entity Framework query decorator to set the query tracking behavior to
    /// none for query the inner handler. This will results in faster queries since
    /// we are not worried about saving any entities during this transaction
    /// </summary>
    public class EFTrackingQueryDecorator<TQuery, TResult> : IRequestHandler<TQuery, TResult>
        where TQuery : notnull, IQuery
    {
        private readonly IRequestHandler<TQuery, TResult> inner;

        public EFTrackingQueryDecorator(BusinessAppDbContext db,
            IRequestHandler<TQuery, TResult> inner)
        {
            db.NotNull().Expect(nameof(db));
            this.inner = inner.NotNull().Expect(nameof(inner));

            db.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
        }

        public Task<Result<TResult, Exception>> HandleAsync(TQuery query,
            CancellationToken cancelToken)
        {
            return inner.HandleAsync(query, cancelToken);
        }
    }
}
