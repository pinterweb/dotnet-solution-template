namespace BusinessApp.Data
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using BusinessApp.App;
    using BusinessApp.Domain;
    using Microsoft.EntityFrameworkCore;

    /// <summary>
    /// Entity Framework query handler for many record sets
    /// </summary>
    public class EFQueryStrategyHandler<TQuery, TResult> : IRequestHandler<TQuery, IEnumerable<TResult>>
        where TQuery : IQuery
        where TResult : class
    {
        private readonly BusinessAppDbContext db;
        private readonly IDbSetVisitorFactory<TQuery, TResult> dbSetFactory;
        private readonly IQueryVisitorFactory<TQuery, TResult> queryVisitorFactory;

        public EFQueryStrategyHandler(BusinessAppDbContext db,
            IQueryVisitorFactory<TQuery, TResult> queryVisitorFactory,
            IDbSetVisitorFactory<TQuery, TResult> dbSetFactory)
        {
            this.db = db.NotNull().Expect(nameof(db));
            this.queryVisitorFactory = queryVisitorFactory.NotNull().Expect(nameof(queryVisitorFactory));
            this.dbSetFactory = dbSetFactory.NotNull().Expect(nameof(dbSetFactory));
        }

        public virtual async Task<Result<IEnumerable<TResult>, Exception>> HandleAsync(
            TQuery query, CancellationToken cancelToken)
        {
            var queryableFactory = dbSetFactory.Create(query);
            var queryVisitor = queryVisitorFactory.Create(query);
            var queryable = queryableFactory.Visit(db.Set<TResult>());

            var queryResults =  await queryVisitor.Visit(queryable).ToListAsync();

            return Result.Ok<IEnumerable<TResult>>(queryResults);
        }
    }
}
