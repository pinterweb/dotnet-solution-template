using System;
using System.Threading;
using System.Threading.Tasks;
using BusinessApp.Kernel;
using Microsoft.EntityFrameworkCore;

namespace BusinessApp.Infrastructure.Persistence
{
    /// <summary>
    /// Entity Framework query decorator to set the query tracking behavior to
    /// none for query the inner handler. This will results in faster queries since
    /// we are not worried about saving any entities during this transaction
    /// </summary>
    public class EFTrackingQueryDecorator<TRequest, TResponse> : IRequestHandler<TRequest, TResponse>
        where TRequest : notnull, IQuery
    {
        private readonly IRequestHandler<TRequest, TResponse> inner;

        public EFTrackingQueryDecorator(BusinessAppDbContext db,
            IRequestHandler<TRequest, TResponse> inner)
        {
            this.inner = inner.NotNull().Expect(nameof(inner));

            db.NotNull().Expect(nameof(db))
                .ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
        }

        public Task<Result<TResponse, Exception>> HandleAsync(TRequest request,
            CancellationToken cancelToken) => inner.HandleAsync(request, cancelToken);
    }
}
