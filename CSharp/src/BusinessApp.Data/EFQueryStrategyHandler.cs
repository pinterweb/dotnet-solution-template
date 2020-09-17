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
        private readonly BusinessAppDbContext db;
        private readonly IDbSetVisitorFactory<TQuery, TResult> dbSetFactory;
        private readonly IQueryVisitorFactory<TQuery, TResult> queryVisitorFactory;

        public EFQueryStrategyHandler(BusinessAppDbContext db,
            IQueryVisitorFactory<TQuery, TResult> queryVisitorFactory,
            IDbSetVisitorFactory<TQuery, TResult> dbSetFactory)
        {
            this.db = GuardAgainst.Null(db, nameof(db));
            this.queryVisitorFactory = GuardAgainst.Null(queryVisitorFactory, nameof(queryVisitorFactory));
            this.dbSetFactory = GuardAgainst.Null(dbSetFactory, nameof(dbSetFactory));
        }

        public virtual async Task<IEnumerable<TResult>> HandleAsync(TQuery query, CancellationToken cancellationToken)
        {
            var queryableFactory = dbSetFactory.Create(query);
            var queryVisitor = queryVisitorFactory.Create(query);
            var queryable = queryableFactory.Visit(db.Set<TResult>());

            return await queryVisitor.Visit(queryable).ToListAsync();
        }
    }

    /// <summary>
    /// Entity Framework query handler for one record set
    /// </summary>
    public class EFSingleQueryStrategyHandler<TQuery, TResult> : IQueryHandler<TQuery, TResult>
        where TQuery : Query, IQuery<TResult>
        where TResult : class
    {
        private readonly BusinessAppDbContext db;
        private readonly IDbSetVisitorFactory<TQuery, TResult> dbSetFactory;
        private readonly IQueryVisitorFactory<TQuery, TResult> queryVisitorFactory;

        public EFSingleQueryStrategyHandler(BusinessAppDbContext db,
            IQueryVisitorFactory<TQuery, TResult> queryVisitorFactory,
            IDbSetVisitorFactory<TQuery, TResult> dbSetFactory)
        {
            this.db = GuardAgainst.Null(db, nameof(db));
            this.queryVisitorFactory = GuardAgainst.Null(queryVisitorFactory, nameof(queryVisitorFactory));
            this.dbSetFactory = GuardAgainst.Null(dbSetFactory, nameof(dbSetFactory));
        }

        public virtual async Task<TResult> HandleAsync(TQuery query, CancellationToken cancellationToken)
        {
            var queryableFactory = dbSetFactory.Create(query);
            var queryVisitor = queryVisitorFactory.Create(query);
            var queryable = queryableFactory.Visit(db.Set<TResult>());

            return await queryVisitor.Visit(queryable).SingleOrDefaultAsync();
        }
    }
}
