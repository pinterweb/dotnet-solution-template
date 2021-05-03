using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using BusinessApp.Kernel;
using System.Linq;
using System.Threading;
using System;

namespace BusinessApp.Infrastructure.EntityFramework
{
    /// <summary>
    /// Handles a request for an <see cref="EnvelopeContract" />
    /// </summary>
    public class EFEnvelopedQueryHandler<TRequest, TResponse> : IRequestHandler<TRequest, EnvelopeContract<TResponse>>
        where TRequest : notnull, IQuery
        where TResponse : class
    {
        private readonly BusinessAppDbContext db;
        private readonly IDbSetVisitorFactory<TRequest, TResponse> dbSetFactory;
        private readonly IQueryVisitorFactory<TRequest, TResponse> queryVisitorFactory;

        public EFEnvelopedQueryHandler(BusinessAppDbContext db,
            IQueryVisitorFactory<TRequest, TResponse> queryVisitorFactory,
            IDbSetVisitorFactory<TRequest, TResponse> dbSetFactory)
        {
            this.db = db.NotNull().Expect(nameof(db));
            this.queryVisitorFactory = queryVisitorFactory.NotNull().Expect(nameof(queryVisitorFactory));
            this.dbSetFactory = dbSetFactory.NotNull().Expect(nameof(dbSetFactory));
        }

        public async Task<Result<EnvelopeContract<TResponse>, Exception>> HandleAsync(
            TRequest request, CancellationToken cancelToken)
        {
            var dbSetVisitor = dbSetFactory.Create(request);
            var queryVisitor = queryVisitorFactory.Create(request);
            var queryable = dbSetVisitor.Visit(db.Set<TResponse>());

            // handle these values here so we can get a total count
            var take = request.Limit;
            var skip = request.Offset ?? 0;
            request.Limit = null;
            request.Offset = null;

            var finalQuery = queryVisitor.Visit(queryable);
            var totalCount = await finalQuery.CountAsync(cancelToken);

            var data = await finalQuery
                .Skip(skip)
                .Take(take ?? (totalCount == 0 ? 1 : totalCount))
                .ToListAsync(cancelToken);

            var page = new Pagination
            {
                ItemCount = totalCount
            };

            return Result.Ok(new EnvelopeContract<TResponse>(data, page));
        }
    }
}
