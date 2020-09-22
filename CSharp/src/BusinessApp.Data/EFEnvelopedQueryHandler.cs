namespace BusinessApp.Data
{
    using Microsoft.EntityFrameworkCore;
    using System.Threading.Tasks;
    using BusinessApp.App;
    using BusinessApp.Domain;
    using System.Linq;
    using System.Threading;

    public class EFEnvelopedQueryHandler<TQuery, TResult> : IQueryHandler<TQuery, EnvelopeContract<TResult>>
        where TQuery : Query, IQuery<EnvelopeContract<TResult>>
        where TResult : class
    {
        private readonly BusinessAppDbContext db;
        private readonly IDbSetVisitorFactory<TQuery, TResult> dbSetFactory;
        private readonly IQueryVisitorFactory<TQuery, TResult> queryVisitorFactory;

        public EFEnvelopedQueryHandler(BusinessAppDbContext db,
            IQueryVisitorFactory<TQuery, TResult> queryVisitorFactory,
            IDbSetVisitorFactory<TQuery, TResult> dbSetFactory)
        {
            this.db = Guard.Against.Null(db).Expect(nameof(db));
            this.queryVisitorFactory = Guard.Against.Null(queryVisitorFactory).Expect(nameof(queryVisitorFactory));
            this.dbSetFactory = Guard.Against.Null(dbSetFactory).Expect(nameof(dbSetFactory));
        }

        public async Task<EnvelopeContract<TResult>> HandleAsync(TQuery query,
            CancellationToken cancellationToken)
        {
            var dbSetVisitor = dbSetFactory.Create(query);
            var queryVisitor = queryVisitorFactory.Create(query);
            var queryable = dbSetVisitor.Visit(db.Set<TResult>());

            // handle these values here so we can get a total count
            var take = query.Limit;
            var skip = query.Offset ?? 0;
            query.Limit = null;
            query.Offset = null;

            var finalQuery = queryVisitor.Visit(queryable);
            var totalCount = await finalQuery.CountAsync(cancellationToken);

            return new EnvelopeContract<TResult>
            {
                // take must be greater than 0
                Data = await finalQuery.Skip(skip).Take(take ?? (totalCount == 0 ? 1 : totalCount)).ToListAsync(),
                Pagination = new Pagination
                {
                    ItemCount = totalCount
                }
            };
        }
    }
}
