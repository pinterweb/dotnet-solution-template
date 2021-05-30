using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using BusinessApp.Kernel;
using Microsoft.EntityFrameworkCore;

namespace BusinessApp.Infrastructure.Persistence
{
    /// <summary>
    /// Entity Framework query handler for many record sets
    /// </summary>
    public class EFQueryStrategyHandler<TRequest, TResponse> : IRequestHandler<TRequest, IEnumerable<TResponse>>
        where TRequest : notnull, IQuery
        where TResponse : class
    {
        private readonly BusinessAppDbContext db;
        private readonly IDbSetVisitorFactory<TRequest, TResponse> dbSetFactory;
        private readonly IQueryVisitorFactory<TRequest, TResponse> queryVisitorFactory;

        public EFQueryStrategyHandler(BusinessAppDbContext db,
            IQueryVisitorFactory<TRequest, TResponse> queryVisitorFactory,
            IDbSetVisitorFactory<TRequest, TResponse> dbSetFactory)
        {
            this.db = db.NotNull().Expect(nameof(db));
            this.queryVisitorFactory = queryVisitorFactory.NotNull().Expect(nameof(queryVisitorFactory));
            this.dbSetFactory = dbSetFactory.NotNull().Expect(nameof(dbSetFactory));
        }

        public virtual async Task<Result<IEnumerable<TResponse>, Exception>> HandleAsync(
            TRequest request, CancellationToken cancelToken)
        {
            var queryableFactory = dbSetFactory.Create(request);
            var queryVisitor = queryVisitorFactory.Create(request);
            var queryable = queryableFactory.Visit(db.Set<TResponse>());

            var queryResults = await queryVisitor.Visit(queryable).ToListAsync(cancelToken);

            return Result.Ok<IEnumerable<TResponse>>(queryResults);
        }
    }
}
