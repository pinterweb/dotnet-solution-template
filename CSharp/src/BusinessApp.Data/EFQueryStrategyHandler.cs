namespace BusinessApp.Data
{
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using BusinessApp.App;
    using BusinessApp.Domain;
    using Microsoft.EntityFrameworkCore;

    /// <summary>
    /// Entity Framework query handler for many record sets
    /// </summary>
    public class EFQueryStrategyHandler<TQuery, TResult> : IQueryHandler<TQuery, IEnumerable<TResult>>
        where TQuery : Query, IQuery<IEnumerable<TResult>>
        where TResult : class
    {
        private readonly BusinessAppReadOnlyDbContext db;
        private readonly IQueryVisitorFactory<TQuery, TResult> queryVisitorFactory;

        public EFQueryStrategyHandler(BusinessAppReadOnlyDbContext db,
            IQueryVisitorFactory<TQuery, TResult> queryVisitorFactory)
        {
            this.db = GuardAgainst.Null(db, nameof(db));
            this.queryVisitorFactory = GuardAgainst.Null(queryVisitorFactory, nameof(queryVisitorFactory));
        }

        public virtual async Task<IEnumerable<TResult>> HandleAsync(TQuery query, CancellationToken cancellationToken)
        {
            var queryable = db.Set<TResult>();
            var visitor = queryVisitorFactory.Create(query);

            return await visitor.Visit(queryable).ToListAsync();
        }
    }

    /// <summary>
    /// Entity Framework query handler for one record set
    /// </summary>
    public class EFSingleQueryStrategyHandler<TQuery, TResult> : IQueryHandler<TQuery, TResult>
        where TQuery : Query, IQuery<TResult>
        where TResult : class
    {
        private readonly BusinessAppReadOnlyDbContext db;
        private readonly IQueryVisitorFactory<TQuery, TResult> queryVisitorFactory;

        public EFSingleQueryStrategyHandler(BusinessAppReadOnlyDbContext db,
            IQueryVisitorFactory<TQuery, TResult> queryVisitorFactory)
        {
            this.db = GuardAgainst.Null(db, nameof(db));
            this.queryVisitorFactory = GuardAgainst.Null(queryVisitorFactory, nameof(queryVisitorFactory));
        }

        public virtual async Task<TResult> HandleAsync(TQuery query, CancellationToken cancellationToken)
        {
            var queryable = db.Set<TResult>();
            var visitor = queryVisitorFactory.Create(query);

            return await visitor.Visit(queryable).SingleOrDefaultAsync();
        }
    }
}
