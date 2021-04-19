using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using BusinessApp.App;
using BusinessApp.Domain;
using Microsoft.EntityFrameworkCore;

namespace BusinessApp.Data
{
    /// <summary>
    /// Runs Entity Framework specific logic based on the <see cref="IQuery"/> data
    /// </summary>
    public class EFQueryVisitor<TResult> : IQueryVisitor<TResult>
        where TResult : class
    {
        private readonly IQuery query;
        private static IEnumerable<string> IncludablePropNames = typeof(TResult)
           .GetProperties(BindingFlags.Instance | BindingFlags.Public)
           .Select(p => p.Name)
           .ToList();

        public EFQueryVisitor(IQuery query)
        {
            this.query = query.NotNull().Expect(nameof(query));
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
