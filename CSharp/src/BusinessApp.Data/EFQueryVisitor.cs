namespace BusinessApp.Data
{
    using System.Linq;
    using BusinessApp.App;
    using BusinessApp.Domain;
    using Microsoft.EntityFrameworkCore;

    /// <summary>
    /// Factory to create an Entity Framework query visitor
    /// </summary>
    public class EFQueryVisitorFactory<TQuery, TResult> : IQueryVisitorFactory<TQuery, TResult>
        where TQuery : Query
        where TResult : class
    {
        public IQueryVisitor<TResult> Create(TQuery query) => new EFQueryVisitor<TResult>(query);
    }

    /// <summary>
    /// Runs Entity Framework specific logic based on the <see cref="Query"/> data
    /// </summary>
    public class EFQueryVisitor<TResult> : IQueryVisitor<TResult>
        where TResult : class
    {
        private readonly Query query;

        public EFQueryVisitor(Query query)
        {
            this.query = GuardAgainst.Null(query, nameof(query));
        }

        public IQueryable<TResult> Visit(IQueryable<TResult> queryable)
        {
            foreach (var item in query.Embed.Concat(query.Expand))
            {
                queryable = queryable.Include(item.ConvertToPascalCase());
            }

            if (query.Offset.HasValue) queryable = queryable.Skip(query.Offset.Value);
            if (query.Limit.HasValue) queryable = queryable.Take(query.Limit.Value);

            return queryable;
        }
    }
}
