namespace BusinessApp.Data
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using BusinessApp.App;
    using BusinessApp.Domain;
    using Microsoft.EntityFrameworkCore;

    /// <summary>
    /// Runs Entity Framework specific logic based on the <see cref="Query"/> data
    /// </summary>
    public class EFQueryVisitor<TResult> : IQueryVisitor<TResult>
        where TResult : class
    {
        private readonly Query query;
        private static IEnumerable<string> IncludablePropNames = typeof(TResult)
           .GetProperties(BindingFlags.Instance | BindingFlags.Public)
           .Where(prop =>
               (
                   prop.PropertyType.IsGenericIEnumerable() ?
                   prop.PropertyType.GetGenericArguments()[0] :
                   prop.PropertyType
               ) != null
           )
           .Select(p => p.Name)
           .ToList();

        public EFQueryVisitor(Query query)
        {
            this.query = GuardAgainst.Null(query, nameof(query));
        }

        public IQueryable<TResult> Visit(IQueryable<TResult> queryable)
        {
            var includables = query.Embed
                .Concat(query.Expand)
                .Select(i => i.ConvertToPascalCase())
                .Where(i => IncludablePropNames.Contains(i.Split('.')[0]));

            foreach (var item in includables)
            {
                queryable = queryable.Include(item);
            }

            if (query.Offset.HasValue) queryable = queryable.Skip(query.Offset.Value);
            if (query.Limit.HasValue) queryable = queryable.Take(query.Limit.Value);

            return queryable;
        }
    }
}
